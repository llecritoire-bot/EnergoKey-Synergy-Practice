#!/usr/bin/env python3
"""Read-only structural verifier for the EnergoKey delivery.

The verifier never creates, edits, deletes, stages, or commits files.  It only
reports facts that can be established from the filesystem and Git.  Runtime or
visual claims that cannot be established automatically are reported as
``NOT VERIFIED`` rather than inferred.

Typical use after the project and evidence package are complete::

    python3 Tools/verify_deliverable.py
    python3 Tools/verify_deliverable.py --matrix /path/to/03_SCREENSHOT_MATRIX.md
    python3 Tools/verify_deliverable.py --root /path/to/EnergoKey

Exit codes:

* 0 - every automated check passed and no unresolved check remains;
* 1 - at least one check failed;
* 2 - no check failed, but at least one check remains NOT VERIFIED.
"""

from __future__ import annotations

import argparse
import os
import re
import shutil
import struct
import subprocess
import sys
import textwrap
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Sequence, Tuple


PASS = "PASS"
FAIL = "FAIL"
NOT_VERIFIED = "NOT VERIFIED"

EXPECTED_UNITY_VERSION = "6000.6.2f1"
EXPECTED_PRODUCT_VERSION = "1.0.0"

FORBIDDEN_DIRECTORY_NAMES = {
    "library",
    "temp",
    "logs",
    "obj",
    "usersettings",
    "build",
}

EXPECTED_STAGE_TAGS = (
    "up-pm12-prototype",
    "pp-pm12-three-levels",
    "pm13-before-visual-style",
    "up-pm13-styled",
    "profiler-before",
    "pp-pm13-release",
)

# Used only if no matrix file can be found.  The missing matrix is still
# reported as NOT VERIFIED, so this fallback cannot masquerade as a source
# check.  It keeps the delivery name checks useful in a detached project copy.
BUILTIN_SCREENSHOT_NAMES = (
    "UP12_01_ProjectFolders.png",
    "UP12_02_SceneOverview.png",
    "UP12_03_GameRunning.png",
    "UP12_04_PlayerInspector.png",
    "UP12_05_CollectibleInspector.png",
    "UP12_06_HazardInspector.png",
    "UP12_07_InitialState.png",
    "UP12_08_Gameplay.png",
    "UP12_09_SpecialRule.png",
    "UP12_10_Victory.png",
    "UP12_11_Defeat.png",
    "UP12_12_Restart.png",
    "PP12_01_Level1.png",
    "PP12_02_Level2.png",
    "PP12_03_Level3.png",
    "PP12_04_MainMenu_LevelSelect.png",
    "PP12_05_Pause.png",
    "PP12_06_Victory.png",
    "PP12_07_Defeat.png",
    "PP12_08_SavedProgressAfterRestart.png",
    "PP12_09_SavedVolume.png",
    "PP12_10_ProfilerBefore.png",
    "PP12_11_ProfilerAfter.png",
    "PP12_12_Settings.png",
    "UP13_01_BeforeStyling.png",
    "UP13_02_ImportedSpriteInspector.png",
    "UP13_03_ObjectInspector.png",
    "UP13_04_PrefabsFolder.png",
    "UP13_05_CanvasScaler.png",
    "UP13_06_RectTransformAnchors.png",
    "UP13_07_ReadyHUD.png",
    "UP13_08_EventFeedback.png",
    "UP13_09_MainMenu.png",
    "UP13_10_Pause.png",
    "UP13_11_Victory.png",
    "UP13_12_Defeat.png",
    "UP13_13_FinalProduct.png",
    "PP13_01_BeforeReleaseChanges.png",
    "PP13_02_16x9.png",
    "PP13_03_18x9.png",
    "PP13_04_4x3.png",
    "PP13_05_SafeAreaSimulation.png",
    "PP13_06_Tutorial.png",
    "PP13_07_PauseTransition.png",
    "PP13_08_ProductPageMockup.png",
    "PP13_09_AboutScreen.png",
    "PP13_10_BeforeUserFix.png",
    "PP13_11_AfterUserFix.png",
    "STORE_01_Menu.png",
    "STORE_02_Tutorial.png",
    "STORE_03_Gameplay.png",
    "STORE_04_Pause.png",
    "STORE_05_Result.png",
    "STORE_06_About.png",
)

REQUIRED_PREFAB_GROUPS: Dict[str, Tuple[str, ...]] = {
    "Player": ("player", "playerprefab"),
    "Collectible": ("collectible", "collectibleprefab", "crystal", "crystalprefab"),
    "Hazard": ("hazard", "hazardprefab"),
    "UI Button": ("uibutton", "uibuttonprefab", "buttonui", "buttonuiprefab"),
}

TEXT_ASSET_EXTENSIONS = {
    ".asset",
    ".cs",
    ".json",
    ".prefab",
    ".txt",
    ".unity",
    ".uss",
    ".uxml",
    ".xml",
    ".yaml",
    ".yml",
}

VERSION_TOKEN_RE = re.compile(r"(?<![0-9])1\.0\.0(?![0-9])")
FULL_GIT_HASH_RE = re.compile(r"(?i)(?<![0-9a-f])[0-9a-f]{40}(?![0-9a-f])")
MATRIX_PNG_RE = re.compile(r"`([^`\r\n]+\.png)`", re.IGNORECASE)


@dataclass(frozen=True)
class CheckResult:
    status: str
    code: str
    message: str
    detail: str = ""


class Reporter:
    def __init__(self) -> None:
        self.results: List[CheckResult] = []

    def add(self, status: str, code: str, message: str, detail: str = "") -> None:
        if status not in {PASS, FAIL, NOT_VERIFIED}:
            raise ValueError("Unsupported status: %s" % status)
        self.results.append(CheckResult(status, code, message, detail))

    def print_report(self) -> int:
        for result in self.results:
            print(f"{result.status:<12} {result.code}: {result.message}")
            if result.detail:
                wrapped = textwrap.fill(
                    result.detail,
                    width=100,
                    initial_indent="  ",
                    subsequent_indent="  ",
                )
                print(wrapped)

        counts = {status: 0 for status in (PASS, FAIL, NOT_VERIFIED)}
        for result in self.results:
            counts[result.status] += 1

        print()
        print(
            "SUMMARY: "
            f"PASS={counts[PASS]} "
            f"FAIL={counts[FAIL]} "
            f"NOT VERIFIED={counts[NOT_VERIFIED]}"
        )

        if counts[FAIL]:
            return 1
        if counts[NOT_VERIFIED]:
            return 2
        return 0


def unique_in_order(values: Iterable[str]) -> List[str]:
    seen = set()
    result: List[str] = []
    for value in values:
        if value not in seen:
            seen.add(value)
            result.append(value)
    return result


def relative_display(path: Path, root: Path) -> str:
    try:
        return str(path.resolve().relative_to(root.resolve()))
    except (OSError, ValueError):
        return str(path)


def read_text(path: Path) -> Optional[str]:
    try:
        return path.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None


def check_required_structure(root: Path, reporter: Reporter) -> None:
    required_dirs = (
        "Assets",
        "Packages",
        "ProjectSettings",
        "EVIDENCE",
        "EVIDENCE/screenshots",
        "Release",
        "Release/Docs",
        "Release/Screenshots",
        "Tools",
    )
    required_files = (
        ".gitignore",
        "README.md",
        "Packages/manifest.json",
        "Packages/packages-lock.json",
        "ProjectSettings/ProjectVersion.txt",
        "ProjectSettings/ProjectSettings.asset",
    )

    missing_dirs = [item for item in required_dirs if not (root / item).is_dir()]
    if missing_dirs:
        reporter.add(
            FAIL,
            "unity.required_directories",
            "Missing required project directories",
            ", ".join(missing_dirs),
        )
    else:
        reporter.add(
            PASS,
            "unity.required_directories",
            f"All {len(required_dirs)} required project directories exist",
        )

    missing_files = [item for item in required_files if not (root / item).is_file()]
    if missing_files:
        reporter.add(
            FAIL,
            "unity.required_files",
            "Missing required Unity or delivery files",
            ", ".join(missing_files),
        )
    else:
        reporter.add(
            PASS,
            "unity.required_files",
            f"All {len(required_files)} required Unity and delivery files exist",
        )

    scene_files = sorted((root / "Assets").rglob("*.unity")) if (root / "Assets").is_dir() else []
    if scene_files:
        reporter.add(
            PASS,
            "unity.scene_files",
            f"Found {len(scene_files)} Unity scene file(s)",
            ", ".join(relative_display(item, root) for item in scene_files),
        )
    else:
        reporter.add(FAIL, "unity.scene_files", "No .unity scene files were found under Assets")

    project_version = root / "ProjectSettings" / "ProjectVersion.txt"
    project_version_text = read_text(project_version)
    if project_version_text is None:
        reporter.add(FAIL, "unity.editor_version", "ProjectVersion.txt is missing or unreadable")
    elif re.search(
        rf"(?m)^\s*m_EditorVersion:\s*{re.escape(EXPECTED_UNITY_VERSION)}\s*$",
        project_version_text,
    ):
        reporter.add(
            PASS,
            "unity.editor_version",
            f"Unity editor version is {EXPECTED_UNITY_VERSION}",
        )
    else:
        reporter.add(
            FAIL,
            "unity.editor_version",
            f"ProjectVersion.txt does not declare {EXPECTED_UNITY_VERSION}",
        )


def check_forbidden_directories(root: Path, reporter: Reporter) -> None:
    found: List[Path] = []
    for current, dirs, _files in os.walk(root, followlinks=False):
        current_path = Path(current)
        kept: List[str] = []
        for dirname in dirs:
            if dirname == ".git":
                continue
            candidate = current_path / dirname
            if dirname.casefold() in FORBIDDEN_DIRECTORY_NAMES:
                found.append(candidate)
                continue
            kept.append(dirname)
        dirs[:] = kept

    if found:
        reporter.add(
            FAIL,
            "delivery.forbidden_directories",
            "Forbidden generated directories are present inside the delivery",
            ", ".join(relative_display(item, root) for item in sorted(found)),
        )
    else:
        reporter.add(
            PASS,
            "delivery.forbidden_directories",
            "Library, Temp, Logs, obj, UserSettings, and Build directories are absent",
        )


def find_matrix(root: Path, explicit: Optional[str]) -> Optional[Path]:
    if explicit:
        candidate = Path(explicit).expanduser()
        if not candidate.is_absolute():
            candidate = Path.cwd() / candidate
        return candidate.resolve()

    candidates = (
        root / "Documentation" / "03_SCREENSHOT_MATRIX.md",
        root.parent / "codex_synergy_practice_pack" / "03_SCREENSHOT_MATRIX.md",
    )
    for candidate in candidates:
        if candidate.is_file():
            return candidate.resolve()
    return None


def load_screenshot_names(
    matrix_path: Optional[Path], reporter: Reporter
) -> Tuple[List[str], bool]:
    if matrix_path is None:
        reporter.add(
            NOT_VERIFIED,
            "screenshots.matrix_source",
            "03_SCREENSHOT_MATRIX.md was not found; using the built-in canonical name list",
        )
        return list(BUILTIN_SCREENSHOT_NAMES), False

    matrix_text = read_text(matrix_path)
    if matrix_text is None:
        reporter.add(
            FAIL,
            "screenshots.matrix_source",
            "The selected screenshot matrix is missing or unreadable",
            str(matrix_path),
        )
        return list(BUILTIN_SCREENSHOT_NAMES), False

    parsed = unique_in_order(Path(match).name for match in MATRIX_PNG_RE.findall(matrix_text))
    if not parsed:
        reporter.add(
            FAIL,
            "screenshots.matrix_source",
            "No backticked PNG names were parsed from the screenshot matrix",
            str(matrix_path),
        )
        return list(BUILTIN_SCREENSHOT_NAMES), False

    reporter.add(
        PASS,
        "screenshots.matrix_source",
        f"Parsed {len(parsed)} unique PNG name(s) from the screenshot matrix",
        str(matrix_path),
    )

    missing_canonical = sorted(set(BUILTIN_SCREENSHOT_NAMES) - set(parsed))
    store_count = sum(1 for name in parsed if name.upper().startswith("STORE_"))
    if missing_canonical or store_count < 6:
        detail_parts = []
        if missing_canonical:
            detail_parts.append("Missing canonical names: " + ", ".join(missing_canonical))
        if store_count < 6:
            detail_parts.append(f"STORE names found: {store_count}; required: at least 6")
        reporter.add(
            FAIL,
            "screenshots.matrix_completeness",
            "The matrix does not contain the complete required screenshot name set",
            "; ".join(detail_parts),
        )
    else:
        reporter.add(
            PASS,
            "screenshots.matrix_completeness",
            f"The matrix contains all canonical names, including {store_count} STORE files",
        )

    return parsed, True


def image_dimensions(path: Path) -> Tuple[Optional[Tuple[int, int]], str]:
    """Return image dimensions without requiring third-party packages.

    PNG is read directly from its signature and IHDR chunk.  Pillow is used only
    as a graceful fallback when available.
    """

    try:
        with path.open("rb") as handle:
            header = handle.read(24)
        if len(header) >= 24 and header[:8] == b"\x89PNG\r\n\x1a\n" and header[12:16] == b"IHDR":
            width, height = struct.unpack(">II", header[16:24])
            if width > 0 and height > 0:
                return (width, height), "PNG IHDR"
    except OSError as exc:
        return None, str(exc)

    try:
        from PIL import Image  # type: ignore
    except (ImportError, ModuleNotFoundError):
        return None, "not a readable PNG and Pillow is unavailable"

    try:
        with Image.open(path) as image:
            return (int(image.width), int(image.height)), f"Pillow ({image.format or 'unknown'})"
    except Exception as exc:  # Pillow exposes several decoder-specific errors.
        return None, f"Pillow could not read the image: {exc}"


def check_screenshot_files(root: Path, names: Sequence[str], reporter: Reporter) -> None:
    missing: List[str] = []
    unreadable: List[str] = []
    for name in names:
        if name.upper().startswith("STORE_"):
            path = root / "Release" / "Screenshots" / name
        else:
            path = root / "EVIDENCE" / "screenshots" / name

        if not path.is_file():
            missing.append(relative_display(path, root))
            continue

        dimensions, reason = image_dimensions(path)
        if dimensions is None or "PNG" not in reason.upper():
            unreadable.append(f"{relative_display(path, root)} ({reason})")

    if missing or unreadable:
        detail_parts = []
        if missing:
            detail_parts.append("Missing: " + ", ".join(missing))
        if unreadable:
            detail_parts.append("Unreadable: " + ", ".join(unreadable))
        reporter.add(
            FAIL,
            "screenshots.required_files",
            "Required screenshot files are missing or unreadable",
            "; ".join(detail_parts),
        )
    else:
        reporter.add(
            PASS,
            "screenshots.required_files",
            f"All {len(names)} matrix PNG files exist and have readable image headers",
        )

    reporter.add(
        NOT_VERIFIED,
        "screenshots.authenticity",
        "Automatic checks cannot prove that the PNGs are genuine Unity/game captures or match their captions",
        "Perform a human visual review and retain NOT VERIFIED until that review is recorded.",
    )


def check_exact_image(
    root: Path,
    relative_path: str,
    expected_size: Tuple[int, int],
    code: str,
    reporter: Reporter,
) -> None:
    path = root / relative_path
    if not path.is_file():
        reporter.add(FAIL, code, f"Missing required image {relative_path}")
        return

    dimensions, reason = image_dimensions(path)
    if dimensions is None:
        reporter.add(
            NOT_VERIFIED,
            code,
            f"Could not determine dimensions of {relative_path}",
            reason,
        )
    elif "PNG" not in reason.upper():
        reporter.add(
            FAIL,
            code,
            f"{relative_path} is not encoded as PNG",
            reason,
        )
    elif dimensions != expected_size:
        reporter.add(
            FAIL,
            code,
            f"{relative_path} has size {dimensions[0]}x{dimensions[1]}, expected {expected_size[0]}x{expected_size[1]}",
        )
    else:
        reporter.add(
            PASS,
            code,
            f"{relative_path} has the required size {expected_size[0]}x{expected_size[1]}",
        )


def check_release_images(root: Path, reporter: Reporter) -> None:
    check_exact_image(root, "Release/icon_512.png", (512, 512), "release.icon", reporter)
    check_exact_image(
        root,
        "Release/promo_1280x720.png",
        (1280, 720),
        "release.promo",
        reporter,
    )

    screenshots_dir = root / "Release" / "Screenshots"
    pngs = sorted(screenshots_dir.rglob("*.png")) if screenshots_dir.is_dir() else []
    readable: List[Path] = []
    unreadable: List[str] = []
    for path in pngs:
        dimensions, reason = image_dimensions(path)
        if dimensions is None or "PNG" not in reason.upper():
            unreadable.append(f"{relative_display(path, root)} ({reason})")
        else:
            readable.append(path)

    if len(readable) < 6:
        detail = f"Readable PNG count: {len(readable)}; required: at least 6"
        if unreadable:
            detail += "; unreadable: " + ", ".join(unreadable)
        reporter.add(
            FAIL,
            "release.screenshot_count",
            "Release/Screenshots does not contain at least six readable PNG files",
            detail,
        )
    else:
        detail = ", ".join(relative_display(path, root) for path in readable)
        if unreadable:
            detail += "; unreadable extras: " + ", ".join(unreadable)
        reporter.add(
            PASS,
            "release.screenshot_count",
            f"Release/Screenshots contains {len(readable)} readable PNG files",
            detail,
        )

    product_mockup_candidates = (
        root / "Release" / "product_page_mockup.png",
        root / "Release" / "product_page_mockup.html",
        root / "Release" / "Store" / "product_page_mockup.png",
        root / "Release" / "Store" / "product_page_mockup.html",
    )
    existing_mockups = [path for path in product_mockup_candidates if path.is_file()]
    if not existing_mockups:
        reporter.add(FAIL, "release.product_mockup", "Product page mockup is missing")
    else:
        png_mockups = [path for path in existing_mockups if path.suffix.casefold() == ".png"]
        unreadable_mockups = []
        for path in png_mockups:
            dimensions, reason = image_dimensions(path)
            if dimensions is None or "PNG" not in reason.upper():
                unreadable_mockups.append(f"{relative_display(path, root)} ({reason})")
        if unreadable_mockups:
            reporter.add(
                FAIL,
                "release.product_mockup",
                "A product page mockup PNG is unreadable",
                ", ".join(unreadable_mockups),
            )
        else:
            reporter.add(
                PASS,
                "release.product_mockup",
                "Product page mockup exists",
                ", ".join(relative_display(path, root) for path in existing_mockups),
            )


def clean_markdown_text(text: str) -> str:
    text = re.sub(r"!\[([^\]]*)\]\([^)]*\)", r"\1", text)
    text = re.sub(r"\[([^\]]+)\]\([^)]*\)", r"\1", text)
    text = re.sub(r"<[^>]+>", " ", text)
    text = re.sub(r"[`*_~]", "", text)
    text = re.sub(r"^\s{0,3}#{1,6}\s*", "", text)
    text = re.sub(r"^\s*(?:[-+*]|\d+[.)])\s+", "", text)
    return re.sub(r"\s+", " ", text).strip()


def description_marker(line: str) -> Optional[str]:
    plain = clean_markdown_text(line).casefold()
    if ("кратк" in plain and "описан" in plain) or "short description" in plain:
        return "short"
    if ("полн" in plain and "описан" in plain) or "full description" in plain:
        return "full"
    return None


def inline_after_description_label(line: str) -> str:
    for separator in (":", "—"):
        if separator in line:
            before, after = line.split(separator, 1)
            if description_marker(before):
                return clean_markdown_text(after)
    return ""


def extract_description_sections(text: str) -> Dict[str, str]:
    lines = text.splitlines()
    markers: List[Tuple[int, str]] = []
    for index, line in enumerate(lines):
        kind = description_marker(line)
        if kind:
            markers.append((index, kind))

    sections: Dict[str, str] = {}
    for marker_index, (line_index, kind) in enumerate(markers):
        if kind in sections:
            continue
        parts: List[str] = []
        inline = inline_after_description_label(lines[line_index])
        if inline:
            parts.append(inline)

        next_marker_line = markers[marker_index + 1][0] if marker_index + 1 < len(markers) else len(lines)
        for index in range(line_index + 1, next_marker_line):
            raw = lines[index]
            if raw.lstrip().startswith("#"):
                break
            cleaned = clean_markdown_text(raw)
            if cleaned:
                parts.append(cleaned)
        sections[kind] = re.sub(r"\s+", " ", " ".join(parts)).strip()
    return sections


def check_release_documents(root: Path, reporter: Reporter) -> None:
    required_documents = (
        "Release/Docs/DESCRIPTION.md",
        "Release/Docs/PLAYER_GUIDE.md",
        "Release/Docs/CREDITS.md",
        "Release/Docs/CHANGELOG.md",
        "Release/storyboard.md",
    )
    missing = [item for item in required_documents if not (root / item).is_file()]
    empty = [
        item
        for item in required_documents
        if (root / item).is_file() and (root / item).stat().st_size == 0
    ]
    if missing or empty:
        detail_parts = []
        if missing:
            detail_parts.append("Missing: " + ", ".join(missing))
        if empty:
            detail_parts.append("Empty: " + ", ".join(empty))
        reporter.add(
            FAIL,
            "release.documents",
            "Required Release documents are incomplete",
            "; ".join(detail_parts),
        )
    else:
        reporter.add(
            PASS,
            "release.documents",
            f"All {len(required_documents)} required Release documents exist and are non-empty",
        )

    description_path = root / "Release" / "Docs" / "DESCRIPTION.md"
    description_text = read_text(description_path)
    if description_text is None:
        reporter.add(FAIL, "release.description_lengths", "DESCRIPTION.md is missing or unreadable")
        return

    sections = extract_description_sections(description_text)
    short = sections.get("short", "")
    full = sections.get("full", "")

    if not short:
        reporter.add(
            FAIL,
            "release.short_description",
            "Could not find a non-empty short-description section in DESCRIPTION.md",
        )
    elif len(short) > 80:
        reporter.add(
            FAIL,
            "release.short_description",
            f"Short description is {len(short)} characters; maximum is 80",
        )
    else:
        reporter.add(
            PASS,
            "release.short_description",
            f"Short description is {len(short)} characters, within the 80-character limit",
        )

    if not full:
        reporter.add(
            FAIL,
            "release.full_description",
            "Could not find a non-empty full-description section in DESCRIPTION.md",
        )
    elif not 600 <= len(full) <= 1200:
        reporter.add(
            FAIL,
            "release.full_description",
            f"Full description is {len(full)} characters; required range is 600-1200",
        )
    else:
        reporter.add(
            PASS,
            "release.full_description",
            f"Full description is {len(full)} characters, within the 600-1200 range",
        )


def check_product_version(root: Path, reporter: Reporter) -> None:
    readme = root / "README.md"
    readme_text = read_text(readme)
    if readme_text is None:
        reporter.add(FAIL, "version.readme", "README.md is missing or unreadable")
    elif VERSION_TOKEN_RE.search(readme_text):
        reporter.add(PASS, "version.readme", "README.md contains version 1.0.0")
    else:
        reporter.add(FAIL, "version.readme", "README.md does not contain version 1.0.0")

    project_settings = root / "ProjectSettings" / "ProjectSettings.asset"
    project_settings_text = read_text(project_settings)
    if project_settings_text is None:
        reporter.add(
            FAIL,
            "version.player_settings",
            "ProjectSettings/ProjectSettings.asset is missing or unreadable",
        )
    elif re.search(
        rf"(?m)^\s*bundleVersion:\s*{re.escape(EXPECTED_PRODUCT_VERSION)}\s*$",
        project_settings_text,
    ):
        reporter.add(
            PASS,
            "version.player_settings",
            "Unity Player Settings bundleVersion is 1.0.0",
        )
    else:
        reporter.add(
            FAIL,
            "version.player_settings",
            "Unity Player Settings does not declare bundleVersion: 1.0.0",
        )

    assets = root / "Assets"
    about_candidates: List[Tuple[Path, str]] = []
    literal_version_sources: List[Path] = []
    if assets.is_dir():
        for path in assets.rglob("*"):
            if not path.is_file() or path.suffix.casefold() not in TEXT_ASSET_EXTENSIONS:
                continue
            try:
                if path.stat().st_size > 5 * 1024 * 1024:
                    continue
            except OSError:
                continue
            text = read_text(path)
            if text is None:
                continue
            if VERSION_TOKEN_RE.search(text):
                literal_version_sources.append(path)
            name_marker = "about" in path.stem.casefold()
            content_marker = bool(re.search(r"(?i)\babout\b|об\s+игре", text))
            if name_marker or content_marker:
                about_candidates.append((path, text))

    direct_matches = [path for path, text in about_candidates if VERSION_TOKEN_RE.search(text)]
    indirect_matches = [
        path
        for path, text in about_candidates
        if re.search(r"(?i)version|верси", text) and literal_version_sources
    ]
    if direct_matches:
        reporter.add(
            PASS,
            "version.about_source",
            "An About source contains version 1.0.0",
            ", ".join(relative_display(path, root) for path in direct_matches),
        )
    elif indirect_matches:
        reporter.add(
            PASS,
            "version.about_source",
            "About source references a version value and an Assets source defines 1.0.0",
            "About: "
            + ", ".join(relative_display(path, root) for path in indirect_matches)
            + "; version source: "
            + ", ".join(relative_display(path, root) for path in literal_version_sources),
        )
    else:
        reporter.add(
            FAIL,
            "version.about_source",
            "Could not verify version 1.0.0 in, or referenced by, an About source under Assets",
        )

    reporter.add(
        NOT_VERIFIED,
        "version.about_runtime",
        "Source inspection cannot prove what version is actually rendered on the running About screen",
        "Confirm in Unity Game View and attach PP13_09_AboutScreen.png.",
    )


def normalize_prefab_name(path: Path) -> str:
    return re.sub(r"[^a-z0-9]", "", path.stem.casefold())


def check_prefabs_and_meta(root: Path, reporter: Reporter) -> None:
    assets = root / "Assets"
    prefabs = sorted(assets.rglob("*.prefab")) if assets.is_dir() else []
    prefab_names = {path: normalize_prefab_name(path) for path in prefabs}

    for label, tokens in REQUIRED_PREFAB_GROUPS.items():
        matches = [
            path
            for path, normalized in prefab_names.items()
            if normalized in tokens
        ]
        if not matches:
            reporter.add(
                FAIL,
                "prefab." + re.sub(r"[^a-z0-9]+", "_", label.casefold()).strip("_"),
                f"Required {label} prefab was not found under Assets",
            )
            continue

        missing_meta = [
            path.with_name(path.name + ".meta")
            for path in matches
            if not path.with_name(path.name + ".meta").is_file()
        ]
        if missing_meta:
            reporter.add(
                FAIL,
                "prefab." + re.sub(r"[^a-z0-9]+", "_", label.casefold()).strip("_"),
                f"{label} prefab exists but its Unity .meta file is missing",
                ", ".join(relative_display(path, root) for path in missing_meta),
            )
        else:
            reporter.add(
                PASS,
                "prefab." + re.sub(r"[^a-z0-9]+", "_", label.casefold()).strip("_"),
                f"{label} prefab and .meta file exist",
                ", ".join(relative_display(path, root) for path in matches),
            )

    missing_asset_meta: List[Path] = []
    if assets.is_dir():
        for path in assets.rglob("*"):
            if path.name.startswith(".") or path.name.endswith(".meta"):
                continue
            meta = path.with_name(path.name + ".meta")
            if not meta.is_file():
                missing_asset_meta.append(meta)

    if missing_asset_meta:
        shown = missing_asset_meta[:30]
        detail = ", ".join(relative_display(path, root) for path in shown)
        if len(missing_asset_meta) > len(shown):
            detail += f", ... and {len(missing_asset_meta) - len(shown)} more"
        reporter.add(
            FAIL,
            "unity.asset_meta_coverage",
            f"{len(missing_asset_meta)} Assets entries are missing Unity .meta files",
            detail,
        )
    elif assets.is_dir():
        reporter.add(
            PASS,
            "unity.asset_meta_coverage",
            "Every file and subdirectory under Assets has a sibling .meta file",
        )
    else:
        reporter.add(FAIL, "unity.asset_meta_coverage", "Assets directory is missing")


def run_git(root: Path, args: Sequence[str]) -> Tuple[Optional[int], str, str]:
    git = shutil.which("git")
    if git is None:
        return None, "", "git executable is unavailable"
    try:
        completed = subprocess.run(
            [git, *args],
            cwd=str(root),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            timeout=20,
            check=False,
        )
    except (OSError, subprocess.SubprocessError) as exc:
        return None, "", str(exc)
    return completed.returncode, completed.stdout.strip(), completed.stderr.strip()


def check_git(root: Path, reporter: Reporter) -> None:
    returncode, output, error = run_git(root, ("rev-parse", "--show-toplevel"))
    if returncode is None:
        reporter.add(NOT_VERIFIED, "git.repository", "Git could not be executed", error)
        return
    if returncode != 0:
        reporter.add(FAIL, "git.repository", "Project root is not a readable Git repository", error)
        return

    try:
        actual_root = Path(output).resolve()
        expected_root = root.resolve()
    except OSError:
        actual_root = Path(output)
        expected_root = root
    if actual_root != expected_root:
        reporter.add(
            FAIL,
            "git.repository",
            "The selected project root is not the Git repository root",
            f"Selected: {expected_root}; Git root: {actual_root}",
        )
    else:
        reporter.add(PASS, "git.repository", "Project root is a Git repository root")

    head_rc, head, head_error = run_git(root, ("rev-parse", "HEAD"))
    if head_rc != 0 or not re.fullmatch(r"(?i)[0-9a-f]{40,64}", head):
        reporter.add(FAIL, "git.head", "Could not resolve a full HEAD commit hash", head_error)
        head = ""
    else:
        reporter.add(PASS, "git.head", f"HEAD is {head}")

    tags_rc, tags_output, tags_error = run_git(root, ("tag", "--list"))
    if tags_rc != 0:
        reporter.add(NOT_VERIFIED, "git.tags", "Could not list Git tags", tags_error)
        tags: List[str] = []
    else:
        tags = [line.strip() for line in tags_output.splitlines() if line.strip()]
        missing = [tag for tag in EXPECTED_STAGE_TAGS if tag not in tags]
        if missing:
            reporter.add(
                FAIL,
                "git.tags",
                "Required stage tags are missing",
                ", ".join(missing),
            )
        else:
            reporter.add(
                PASS,
                "git.tags",
                f"All {len(EXPECTED_STAGE_TAGS)} stage tags exist",
                ", ".join(EXPECTED_STAGE_TAGS),
            )

    status_rc, status_output, status_error = run_git(
        root, ("status", "--porcelain=v1", "--untracked-files=all")
    )
    if status_rc != 0:
        reporter.add(NOT_VERIFIED, "git.clean_worktree", "Could not inspect Git worktree", status_error)
    elif status_output:
        reporter.add(
            FAIL,
            "git.clean_worktree",
            "The delivery worktree has uncommitted or untracked files",
            status_output,
        )
    else:
        reporter.add(PASS, "git.clean_worktree", "The delivery worktree is clean")

    if head and "pp-pm13-release" in tags:
        tag_rc, tag_commit, tag_error = run_git(root, ("rev-list", "-n", "1", "pp-pm13-release"))
        if tag_rc != 0:
            reporter.add(
                NOT_VERIFIED,
                "git.final_tag_target",
                "Could not resolve pp-pm13-release",
                tag_error,
            )
        elif tag_commit == head:
            reporter.add(PASS, "git.final_tag_target", "pp-pm13-release points to HEAD")
        else:
            ancestor_rc, _ancestor_output, _ancestor_error = run_git(
                root, ("merge-base", "--is-ancestor", tag_commit, head)
            )
            if ancestor_rc == 0:
                reporter.add(
                    NOT_VERIFIED,
                    "git.final_tag_target",
                    "pp-pm13-release is an ancestor of HEAD, not HEAD itself",
                    f"Tag: {tag_commit}; HEAD: {head}",
                )
            else:
                reporter.add(
                    FAIL,
                    "git.final_tag_target",
                    "pp-pm13-release does not identify the current delivery history",
                    f"Tag: {tag_commit}; HEAD: {head}",
                )

    readme_text = read_text(root / "README.md") or ""
    documented_hashes = unique_in_order(match.lower() for match in FULL_GIT_HASH_RE.findall(readme_text))
    if not documented_hashes:
        reporter.add(FAIL, "git.readme_hash", "README.md does not contain a full 40-character commit hash")
    elif not head:
        reporter.add(
            NOT_VERIFIED,
            "git.readme_hash",
            "README.md contains a full hash, but HEAD could not be resolved for comparison",
            ", ".join(documented_hashes),
        )
    elif head.lower() in documented_hashes:
        reporter.add(PASS, "git.readme_hash", "README.md records the current full HEAD hash")
    else:
        valid_ancestors: List[str] = []
        invalid_hashes: List[str] = []
        for documented in documented_hashes:
            exists_rc, _exists_output, _exists_error = run_git(
                root, ("cat-file", "-e", documented + "^{commit}")
            )
            if exists_rc != 0:
                invalid_hashes.append(documented)
                continue
            ancestor_rc, _ancestor_output, _ancestor_error = run_git(
                root, ("merge-base", "--is-ancestor", documented, head)
            )
            if ancestor_rc == 0:
                valid_ancestors.append(documented)

        if valid_ancestors:
            reporter.add(
                NOT_VERIFIED,
                "git.readme_hash",
                "README.md records an ancestor commit, not the current HEAD",
                "Recorded ancestor(s): " + ", ".join(valid_ancestors) + f"; HEAD: {head}",
            )
        else:
            detail = "Recorded: " + ", ".join(documented_hashes) + f"; HEAD: {head}"
            if invalid_hashes:
                detail += "; unresolved hashes: " + ", ".join(invalid_hashes)
            reporter.add(
                FAIL,
                "git.readme_hash",
                "README.md does not record the current or an ancestor delivery commit",
                detail,
            )


def parse_args(argv: Optional[Sequence[str]] = None) -> argparse.Namespace:
    default_root = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(
        description="Read-only structural verification of the EnergoKey Unity delivery."
    )
    parser.add_argument(
        "--root",
        default=str(default_root),
        help="Unity project root (default: parent of this Tools directory)",
    )
    parser.add_argument(
        "--matrix",
        help=(
            "Path to 03_SCREENSHOT_MATRIX.md. If omitted, the verifier tries "
            "Documentation/03_SCREENSHOT_MATRIX.md and then "
            "../codex_synergy_practice_pack/03_SCREENSHOT_MATRIX.md."
        ),
    )
    return parser.parse_args(argv)


def main(argv: Optional[Sequence[str]] = None) -> int:
    args = parse_args(argv)
    root = Path(args.root).expanduser().resolve()
    reporter = Reporter()

    if not root.is_dir():
        reporter.add(FAIL, "project.root", "Project root does not exist or is not a directory", str(root))
        return reporter.print_report()

    reporter.add(PASS, "project.root", "Project root is readable", str(root))
    check_required_structure(root, reporter)
    check_forbidden_directories(root, reporter)

    matrix_path = find_matrix(root, args.matrix)
    screenshot_names, _matrix_verified = load_screenshot_names(matrix_path, reporter)
    check_screenshot_files(root, screenshot_names, reporter)

    check_release_images(root, reporter)
    check_release_documents(root, reporter)
    check_product_version(root, reporter)
    check_prefabs_and_meta(root, reporter)
    check_git(root, reporter)

    return reporter.print_report()


if __name__ == "__main__":
    sys.exit(main())
