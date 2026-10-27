#!/usr/bin/env python3
"""Redact machine-local metadata from distributable text evidence.

The sanitizer deliberately operates on a narrow allow-list of documentation and
evidence locations.  It never scans source assets, binary media, or ``.git``.
Use ``--check`` in verification jobs; that mode performs no writes and exits
with status 1 when redactions would be required.
"""

from __future__ import annotations

import argparse
import ipaddress
import re
import sys
from collections import Counter
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable, Iterable, Match, Pattern


TEXT_EXTENSIONS = frozenset(
    {
        ".csv",
        ".json",
        ".log",
        ".md",
        ".txt",
        ".xml",
        ".yaml",
        ".yml",
    }
)

# Directory names are matched as complete path components, not substrings.
ALLOWED_DIRECTORY_NAMES = frozenset(
    {
        "Documentation",
        "EVIDENCE",
        "POST_COMMIT_VERIFICATION",
        "Release",
    }
)

# These project-level documentation files can also appear one level below a
# packaging root (for example, ``EnergoKey/README.md``).
ALLOWED_DOCUMENT_NAMES = frozenset(
    {
        "ASSET_GENERATION.md",
        "CHANGELOG.md",
        "CREDITS.md",
        "DELIVERY_MANIFEST.txt",
        "MISSING_EVIDENCE.md",
        "PLAYER_GUIDE.md",
        "README.md",
        "RESTORE_GIT_HISTORY.md",
    }
)

EXCLUDED_DIRECTORY_NAMES = frozenset(
    {
        ".git",
        "Build",
        "Builds",
        "Library",
        "Logs",
        "Temp",
        "UserSettings",
        "obj",
    }
)

REDACTIONS = {
    "user_home": "[USER_HOME]",
    "hostname": "[LOCAL_HOST]",
    "license_client_username": "[LOCAL_USER]",
    "local_ipv4": "[LOCAL_IPV4]",
    "unity_machine_id": "[UNITY_MACHINE_ID]",
    "unity_session_id": "[UNITY_SESSION_ID]",
    "unity_correlation_id": "[UNITY_CORRELATION_ID]",
    "unity_external_correlation_id": "[UNITY_EXTERNAL_CORRELATION_ID]",
}

LOCAL_IPV4_NETWORKS = tuple(
    ipaddress.ip_network(network)
    for network in (
        "10.0.0.0/8",
        "100.64.0.0/10",  # Carrier-grade NAT, also commonly used by Tailscale.
        "127.0.0.0/8",
        "169.254.0.0/16",
        "172.16.0.0/12",
        "192.168.0.0/16",
    )
)

UNIX_USER_HOME_RE = re.compile(
    r"(?<![A-Za-z0-9_])/(?:Users|home)/[^/\s\"'<>:]+"
)
WINDOWS_USER_HOME_RE = re.compile(
    r"(?i)(?<![A-Za-z0-9_])(?:[A-Z]:[\\/]+Users[\\/]+)"
    r"[^\\/\s\"'<>|:]+"
)

LOCAL_HOST_RE = re.compile(
    r"(?i)\b[A-Za-z0-9](?:[A-Za-z0-9-]{0,62}\.)+local\b"
)
HOST_FIELD_RE = re.compile(
    r"(?im)(?P<prefix>\b(?:computer[ _-]*name|host[ _-]*name|hostname)"
    r"\s*[:=]\s*[\"']?)(?P<value>[A-Za-z0-9][A-Za-z0-9.-]{0,252})"
    r"(?P<suffix>[\"']?)"
)
UNITY_PLAYER_HOST_RE = re.compile(
    r"(?i)(?P<prefix>\b[A-Za-z]+(?:Editor|Player)\(\d+,)"
    r"(?P<value>[^)\r\n]+)(?P<suffix>\))"
)

LICENSE_CLIENT_NOTIFICATIONS_RE = re.compile(
    r"\bLicenseClient-(?P<username>[A-Za-z0-9._-]+)-notifications\b"
)
LICENSE_CLIENT_RE = re.compile(
    r"\bLicenseClient-(?P<username>[A-Za-z0-9._-]+)"
    r"(?![A-Za-z0-9._-])"
)

IPV4_CANDIDATE_RE = re.compile(
    r"(?<![\d.])(?:\d{1,3}\.){3}\d{1,3}(?![\d.])"
)


def _unity_id_pattern(label: str) -> Pattern[str]:
    """Build a pattern that handles Unity log, Markdown, and JSON fields."""

    return re.compile(
        rf"(?i)(?P<prefix>[\"']?\b{label}\b[\"']?\s*[:=]\s*[\"']?)"
        r"(?P<value>[A-Za-z0-9+/=_-]{6,})(?P<suffix>[\"']?)"
    )


UNITY_ID_RULES = (
    (
        "unity_external_correlation_id",
        _unity_id_pattern(r"external[ _-]*correlation[ _-]*id"),
    ),
    ("unity_machine_id", _unity_id_pattern(r"machine[ _-]*id")),
    ("unity_session_id", _unity_id_pattern(r"session[ _-]*id")),
    ("unity_correlation_id", _unity_id_pattern(r"correlation[ _-]*id")),
)


@dataclass
class FileResult:
    path: Path
    counts: Counter[str] = field(default_factory=Counter)
    changed: bool = False

    @property
    def total(self) -> int:
        return sum(self.counts.values())


@dataclass
class SanitizationReport:
    scanned_files: int = 0
    changed_files: list[Path] = field(default_factory=list)
    skipped_binary_files: int = 0
    skipped_non_utf8_files: int = 0
    counts: Counter[str] = field(default_factory=Counter)

    @property
    def total_replacements(self) -> int:
        return sum(self.counts.values())


Replacement = str | Callable[[Match[str]], str]


def _sub_counted(
    text: str,
    pattern: Pattern[str],
    replacement: Replacement,
) -> tuple[str, int]:
    """Apply one rule and count only substitutions that alter the match."""

    changed = 0

    def replace(match: Match[str]) -> str:
        nonlocal changed
        value = replacement(match) if callable(replacement) else replacement
        if value != match.group(0):
            changed += 1
        return value

    return pattern.sub(replace, text), changed


def _replace_group_value(placeholder: str) -> Callable[[Match[str]], str]:
    def replace(match: Match[str]) -> str:
        if match.group("value") == placeholder:
            return match.group(0)
        return f"{match.group('prefix')}{placeholder}{match.group('suffix')}"

    return replace


def _replace_local_ipv4(match: Match[str]) -> str:
    candidate = match.group(0)
    try:
        address = ipaddress.ip_address(candidate)
    except ValueError:
        return candidate
    if isinstance(address, ipaddress.IPv4Address) and any(
        address in network for network in LOCAL_IPV4_NETWORKS
    ):
        return REDACTIONS["local_ipv4"]
    return candidate


def sanitize_text(text: str) -> tuple[str, Counter[str]]:
    """Return sanitized text and deterministic per-category counts."""

    counts: Counter[str] = Counter()

    for rule_name, pattern in UNITY_ID_RULES:
        text, count = _sub_counted(
            text,
            pattern,
            _replace_group_value(REDACTIONS[rule_name]),
        )
        counts[rule_name] += count

    text, count = _sub_counted(
        text,
        LICENSE_CLIENT_NOTIFICATIONS_RE,
        lambda match: (
            f"LicenseClient-{REDACTIONS['license_client_username']}-notifications"
        ),
    )
    counts["license_client_username"] += count

    text, count = _sub_counted(
        text,
        LICENSE_CLIENT_RE,
        lambda match: f"LicenseClient-{REDACTIONS['license_client_username']}",
    )
    counts["license_client_username"] += count

    for pattern in (UNITY_PLAYER_HOST_RE, HOST_FIELD_RE):
        text, count = _sub_counted(
            text,
            pattern,
            _replace_group_value(REDACTIONS["hostname"]),
        )
        counts["hostname"] += count

    text, count = _sub_counted(text, LOCAL_HOST_RE, REDACTIONS["hostname"])
    counts["hostname"] += count

    text, count = _sub_counted(text, IPV4_CANDIDATE_RE, _replace_local_ipv4)
    counts["local_ipv4"] += count

    for pattern in (WINDOWS_USER_HOME_RE, UNIX_USER_HOME_RE):
        text, count = _sub_counted(text, pattern, REDACTIONS["user_home"])
        counts["user_home"] += count

    # Keep the result compact for callers and stable for reporting.
    return text, Counter({key: value for key, value in counts.items() if value})


def _is_allowed_text_path(root: Path, path: Path) -> bool:
    relative = path.relative_to(root)
    if path.suffix.lower() not in TEXT_EXTENSIONS:
        return False
    if any(part in EXCLUDED_DIRECTORY_NAMES for part in relative.parts[:-1]):
        return False
    if path.name in ALLOWED_DOCUMENT_NAMES:
        return True
    return any(part in ALLOWED_DIRECTORY_NAMES for part in relative.parts[:-1])


def discover_text_files(root: Path) -> Iterable[Path]:
    """Yield only allow-listed candidate files in stable relative-path order."""

    candidates = (
        path
        for path in root.rglob("*")
        if path.is_file()
        and not path.is_symlink()
        and _is_allowed_text_path(root, path)
    )
    yield from sorted(candidates, key=lambda path: path.relative_to(root).as_posix())


def sanitize_file(path: Path, *, check: bool) -> FileResult | str:
    """Sanitize one UTF-8 text file, or return a skip reason."""

    data = path.read_bytes()
    if b"\x00" in data:
        return "binary"
    try:
        text = data.decode("utf-8")
    except UnicodeDecodeError:
        return "non_utf8"

    sanitized, counts = sanitize_text(text)
    changed = sanitized != text
    if changed and not check:
        # write_bytes preserves the existing file mode and does not translate
        # newlines, which keeps the transformation deterministic.
        path.write_bytes(sanitized.encode("utf-8"))
    return FileResult(path=path, counts=counts, changed=changed)


def run(root: Path, *, check: bool) -> SanitizationReport:
    root = root.expanduser().resolve()
    if not root.is_dir():
        raise ValueError("--root must point to an existing directory")

    report = SanitizationReport()
    for path in discover_text_files(root):
        report.scanned_files += 1
        result = sanitize_file(path, check=check)
        if result == "binary":
            report.skipped_binary_files += 1
            continue
        if result == "non_utf8":
            report.skipped_non_utf8_files += 1
            continue
        assert isinstance(result, FileResult)
        report.counts.update(result.counts)
        if result.changed:
            report.changed_files.append(path.relative_to(root))
    return report


def print_report(report: SanitizationReport, *, check: bool) -> None:
    mode = "CHECK" if check else "APPLY"
    print(f"Mode: {mode}")
    print(f"Files scanned: {report.scanned_files}")
    print(f"Files requiring redaction: {len(report.changed_files)}")
    print(f"Replacements total: {report.total_replacements}")
    for name in REDACTIONS:
        print(f"  {name}: {report.counts.get(name, 0)}")
    print(f"Skipped binary files: {report.skipped_binary_files}")
    print(f"Skipped non-UTF-8 files: {report.skipped_non_utf8_files}")
    if report.changed_files:
        print("Affected files:")
        for path in report.changed_files:
            print(f"  {path.as_posix()}")

    skipped = report.skipped_binary_files + report.skipped_non_utf8_files
    if skipped:
        print("Result: FAIL (allow-listed files could not be verified as UTF-8 text)")
    elif check and report.total_replacements:
        print("Result: FAIL (redactions required)")
    elif check:
        print("Result: PASS (no sensitive metadata found)")
    else:
        print("Result: PASS (sanitization complete)")


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Redact local paths, hosts, private IPv4 addresses, licensing "
            "usernames, and Unity runtime IDs from text evidence."
        )
    )
    parser.add_argument(
        "--root",
        type=Path,
        default=Path(__file__).resolve().parents[1],
        help="project or packaging root (default: parent of Tools)",
    )
    parser.add_argument(
        "--check",
        action="store_true",
        help="report required redactions without modifying files",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    try:
        report = run(args.root, check=args.check)
    except (OSError, ValueError) as error:
        print(f"Error: {error}", file=sys.stderr)
        return 2
    print_report(report, check=args.check)
    if report.skipped_binary_files or report.skipped_non_utf8_files:
        return 2
    return 1 if args.check and report.total_replacements else 0


if __name__ == "__main__":
    raise SystemExit(main())
