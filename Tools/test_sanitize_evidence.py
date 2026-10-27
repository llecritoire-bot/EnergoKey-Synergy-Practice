#!/usr/bin/env python3
"""Unit tests for sanitize_evidence.py; all writes stay in temp folders."""

from __future__ import annotations

import tempfile
import unittest
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import sanitize_evidence


class SanitizeTextTests(unittest.TestCase):
    def test_redacts_supported_metadata_and_keeps_public_ipv4(self) -> None:
        source = "\n".join(
            (
                "/Users/alice/Project/EVIDENCE/run.log",
                r"C:\Users\alice\Project\result.xml",
                "LicenseClient-alice-notifications",
                "OSXEditor(0,MacBook-Air-Alice.local)",
                "[IP] 192.168.1.50; tunnel 100.103.28.173; public 8.8.8.8",
                "Machine Id: abcdefghijklmnop=",
                "Session Id: 0011223344556677",
                '"correlationId": "8899aabbccddeeff"',
                "External correlation Id: 1234567890123456",
            )
        )

        sanitized, counts = sanitize_evidence.sanitize_text(source)

        self.assertIn("[USER_HOME]/Project/EVIDENCE/run.log", sanitized)
        self.assertIn(r"[USER_HOME]\Project\result.xml", sanitized)
        self.assertIn("LicenseClient-[LOCAL_USER]-notifications", sanitized)
        self.assertIn("OSXEditor(0,[LOCAL_HOST])", sanitized)
        self.assertEqual(sanitized.count("[LOCAL_IPV4]"), 2)
        self.assertIn("8.8.8.8", sanitized)
        self.assertIn("Machine Id: [UNITY_MACHINE_ID]", sanitized)
        self.assertIn("Session Id: [UNITY_SESSION_ID]", sanitized)
        self.assertIn('"correlationId": "[UNITY_CORRELATION_ID]"', sanitized)
        self.assertIn(
            "External correlation Id: [UNITY_EXTERNAL_CORRELATION_ID]",
            sanitized,
        )
        self.assertEqual(counts["user_home"], 2)
        self.assertEqual(counts["hostname"], 1)
        self.assertEqual(counts["license_client_username"], 1)
        self.assertEqual(counts["local_ipv4"], 2)

    def test_is_idempotent(self) -> None:
        source = (
            "/home/student/project 192.168.0.2 "
            "Session Id: aabbccddeeff0011"
        )
        sanitized, first_counts = sanitize_evidence.sanitize_text(source)
        repeated, second_counts = sanitize_evidence.sanitize_text(sanitized)

        self.assertGreater(sum(first_counts.values()), 0)
        self.assertEqual(repeated, sanitized)
        self.assertEqual(sum(second_counts.values()), 0)


class ScopeAndModeTests(unittest.TestCase):
    def test_scope_excludes_source_and_git_metadata(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            included = root / "EVIDENCE" / "logs" / "run.log"
            readme = root / "EnergoKey" / "README.md"
            excluded_source = root / "Assets" / "note.txt"
            excluded_git = root / ".git" / "logs" / "HEAD"
            for path in (included, readme, excluded_source, excluded_git):
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("/Users/alice/project", encoding="utf-8")

            discovered = {
                path.relative_to(root).as_posix()
                for path in sanitize_evidence.discover_text_files(root)
            }

            self.assertEqual(
                discovered,
                {"EVIDENCE/logs/run.log", "EnergoKey/README.md"},
            )

    def test_scope_does_not_follow_file_symlinks(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            target = root / "outside.log"
            target.write_text("/Users/alice/project", encoding="utf-8")
            symlink = root / "EVIDENCE" / "linked.log"
            symlink.parent.mkdir(parents=True)
            try:
                symlink.symlink_to(target)
            except OSError as error:
                self.skipTest(f"symlinks unavailable: {error}")

            self.assertEqual(list(sanitize_evidence.discover_text_files(root)), [])

    def test_check_is_read_only_then_apply_becomes_clean(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            evidence = root / "EVIDENCE" / "run.log"
            evidence.parent.mkdir(parents=True)
            original = "host: MacBook-Test.local; IP 10.20.30.40"
            evidence.write_text(original, encoding="utf-8")

            check_report = sanitize_evidence.run(root, check=True)
            self.assertEqual(evidence.read_text(encoding="utf-8"), original)
            self.assertEqual(len(check_report.changed_files), 1)

            apply_report = sanitize_evidence.run(root, check=False)
            self.assertEqual(len(apply_report.changed_files), 1)
            self.assertIn("[LOCAL_HOST]", evidence.read_text(encoding="utf-8"))

            clean_report = sanitize_evidence.run(root, check=True)
            self.assertEqual(clean_report.total_replacements, 0)
            self.assertFalse(clean_report.changed_files)


if __name__ == "__main__":
    unittest.main()
