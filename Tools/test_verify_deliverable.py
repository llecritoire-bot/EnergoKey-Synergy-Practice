#!/usr/bin/env python3
"""Focused unit tests for the evidence checks in verify_deliverable.py."""

from __future__ import annotations

import json
import shutil
import struct
import subprocess
import tempfile
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

import verify_deliverable as verifier


def result_for(reporter: verifier.Reporter, code: str) -> verifier.CheckResult:
    return next(result for result in reporter.results if result.code == code)


def write_minimal_png(path: Path, width: int, height: int) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    header = (
        b"\x89PNG\r\n\x1a\n"
        + struct.pack(">I", 13)
        + b"IHDR"
        + struct.pack(">II", width, height)
    )
    path.write_bytes(header)


class PlayModeAndPersistenceTests(unittest.TestCase):
    def test_exact_seventeen_playmode_results_pass(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            evidence = root / "EVIDENCE"
            evidence.mkdir()
            attributes = {
                "testcasecount": "17",
                "result": "Passed",
                "total": "17",
                "passed": "17",
                "failed": "0",
                "inconclusive": "0",
                "skipped": "0",
            }
            test_run = ET.Element("test-run", attributes)
            for index in range(17):
                ET.SubElement(
                    test_run,
                    "test-case",
                    {"name": f"test-{index}", "result": "Passed"},
                )
            ET.ElementTree(test_run).write(
                evidence / "PlayModeTests-final.xml",
                encoding="utf-8",
                xml_declaration=True,
            )

            reporter = verifier.Reporter()
            verifier.check_playmode_results(root, reporter)

            self.assertEqual(result_for(reporter, "tests.playmode").status, verifier.PASS)

    def test_persistence_probe_requires_two_processes_and_values(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            evidence = root / "EVIDENCE"
            evidence.mkdir()
            payload = {
                "status": "PASS",
                "phaseOneUtc": "2026-10-27T10:00:00Z",
                "phaseTwoUtc": "2026-10-27T10:01:00Z",
                "processCount": 2,
                "unlockedLevel": 3,
                "bestScore": 650,
                "masterVolume": 0.37,
                "tutorialSeen": True,
                "valuesMatched": True,
                "isolatedKeysCleaned": True,
            }
            (evidence / "PERSISTENCE_RESTART_PROBE.json").write_text(
                json.dumps(payload),
                encoding="utf-8",
            )

            reporter = verifier.Reporter()
            verifier.check_persistence_restart_probe(root, reporter)
            self.assertEqual(
                result_for(reporter, "evidence.persistence_restart").status,
                verifier.PASS,
            )

            payload["processCount"] = 1
            (evidence / "PERSISTENCE_RESTART_PROBE.json").write_text(
                json.dumps(payload),
                encoding="utf-8",
            )
            failed = verifier.Reporter()
            verifier.check_persistence_restart_probe(root, failed)
            self.assertEqual(
                result_for(failed, "evidence.persistence_restart").status,
                verifier.FAIL,
            )


class ProfilerAndImageTests(unittest.TestCase):
    def test_profiler_pair_uses_same_600_frame_scenario(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            evidence = root / "EVIDENCE"
            evidence.mkdir()
            common = {
                "schema": "energokey-profiler-benchmark-v1",
                "status": "PASS",
                "unityVersion": verifier.EXPECTED_UNITY_VERSION,
                "processor": "Apple M3",
                "scenario": "same scenario",
                "warmupFrames": 120,
                "measuredFrames": 600,
                "notificationCount": 5,
            }
            before = {
                **common,
                "stage": "before-per-frame",
                "revision": "a" * 40,
                "recorderSampleCount": 600,
                "activeMarkerFrames": 600,
                "totalNanoseconds": 1000,
                "meanPerMeasuredFrameNanoseconds": 10,
            }
            after = {
                **common,
                "stage": "after-event-driven",
                "revision": "b" * 40,
                "recorderSampleCount": 5,
                "activeMarkerFrames": 5,
                "totalNanoseconds": 100,
                "meanPerMeasuredFrameNanoseconds": 1,
            }
            (evidence / "PROFILER_BENCHMARK_BEFORE.json").write_text(
                json.dumps(before),
                encoding="utf-8",
            )
            (evidence / "PROFILER_BENCHMARK_AFTER.json").write_text(
                json.dumps(after),
                encoding="utf-8",
            )

            reporter = verifier.Reporter()
            verifier.check_profiler_benchmark(root, reporter)

            self.assertEqual(
                result_for(reporter, "evidence.profiler_benchmark").status,
                verifier.PASS,
            )

    def test_exact_responsive_matrix_and_dimensions(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            directory = root / "EVIDENCE" / "responsive"
            for name, dimensions in verifier.expected_responsive_images().items():
                write_minimal_png(directory / name, *dimensions)

            reporter = verifier.Reporter()
            verifier.check_responsive_images(root, reporter)
            self.assertEqual(
                result_for(reporter, "screenshots.responsive_matrix").status,
                verifier.PASS,
            )

            write_minimal_png(directory / "RESP_1280x720_Extra.png", 1280, 720)
            failed = verifier.Reporter()
            verifier.check_responsive_images(root, failed)
            self.assertEqual(
                result_for(failed, "screenshots.responsive_matrix").status,
                verifier.FAIL,
            )

    def test_persistence_screenshot_hashes_must_be_distinct(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            screenshots = root / "EVIDENCE" / "screenshots"
            screenshots.mkdir(parents=True)
            contents = {
                "PP12_04_MainMenu_LevelSelect.png": b"menu",
                "PP12_08_SavedProgressAfterRestart.png": b"saved progress",
                "PP12_09_SavedVolume.png": b"saved volume",
                "PP12_12_Settings.png": b"settings",
            }
            for name, data in contents.items():
                (screenshots / name).write_bytes(data)

            reporter = verifier.Reporter()
            verifier.check_persistence_screenshot_uniqueness(root, reporter)
            self.assertEqual(
                result_for(reporter, "screenshots.persistence_uniqueness").status,
                verifier.PASS,
            )

            (screenshots / "PP12_08_SavedProgressAfterRestart.png").write_bytes(b"menu")
            failed = verifier.Reporter()
            verifier.check_persistence_screenshot_uniqueness(root, failed)
            self.assertEqual(
                result_for(failed, "screenshots.persistence_uniqueness").status,
                verifier.FAIL,
            )


class StandaloneAndGitTests(unittest.TestCase):
    def test_mac_standalone_report_and_runtime_marker(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            logs = root / "EVIDENCE" / "logs"
            logs.mkdir(parents=True)
            (root / "EVIDENCE" / "MAC_STANDALONE_TEST.md").write_text(
                "\n".join(
                    (
                        "- Сборка: \x60PASS\x60",
                        "- Фактический запуск собранного приложения: \x60PASS\x60",
                        "- Unity: \x606000.6.2f1\x60",
                        "- Версия игры: \x601.0.0\x60",
                        "Apple M3 — физический MacBook.",
                    )
                ),
                encoding="utf-8",
            )
            (logs / "mac-standalone-smoke.log").write_text(
                "ENERGOKEY_RUNTIME_READY: scene=MainMenu; "
                "version=1.0.0; unity=6000.6.2f1",
                encoding="utf-8",
            )

            reporter = verifier.Reporter()
            verifier.check_mac_standalone(root, reporter)

            self.assertEqual(
                result_for(reporter, "evidence.mac_standalone").status,
                verifier.PASS,
            )

    @unittest.skipUnless(shutil.which("git"), "git is unavailable")
    def test_all_stage_tags_can_be_verified_as_2d(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            (root / "ProjectSettings").mkdir()
            (root / "Packages").mkdir()
            (root / "ProjectSettings" / "EditorSettings.asset").write_text(
                "EditorSettings:\n  m_DefaultBehaviorMode: 1\n",
                encoding="utf-8",
            )
            (root / "Packages" / "manifest.json").write_text(
                json.dumps({"dependencies": {"com.unity.2d.sprite": "1.0.0"}}),
                encoding="utf-8",
            )
            commands = (
                ("init",),
                ("config", "user.name", "Test"),
                ("config", "user.email", "test@example.invalid"),
                ("add", "."),
                ("commit", "-m", "2d snapshot"),
            )
            for command in commands:
                subprocess.run(
                    ["git", *command],
                    cwd=root,
                    check=True,
                    stdout=subprocess.DEVNULL,
                    stderr=subprocess.DEVNULL,
                )
            for tag in verifier.EXPECTED_STAGE_TAGS:
                subprocess.run(
                    ["git", "tag", tag],
                    cwd=root,
                    check=True,
                    stdout=subprocess.DEVNULL,
                    stderr=subprocess.DEVNULL,
                )

            reporter = verifier.Reporter()
            verifier.check_stage_tag_2d_configuration(root, reporter)

            self.assertEqual(len(reporter.results), len(verifier.EXPECTED_STAGE_TAGS))
            self.assertTrue(all(result.status == verifier.PASS for result in reporter.results))


if __name__ == "__main__":
    unittest.main()
