#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EnergoKey.Editor
{
    public static class EnergoKeyValidation
    {
        private const string ExpectedUnityVersion = "6000.6.2f1";
        private const string ExpectedGameVersion = "1.0.0";
        private const string EvidenceJsonPath = "EVIDENCE/PROJECT_VALIDATION.json";
        private const string EvidenceMarkdownPath = "EVIDENCE/PROJECT_VALIDATION.md";

        private static readonly string[] RequiredScenePaths =
        {
            "Assets/Scenes/Prototype.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity"
        };

        private static readonly string[] RequiredBuildScenePaths =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity"
        };

        private static readonly ImageRequirement[] ArtRequirements =
        {
            new ImageRequirement("background", 1280, 720),
            new ImageRequirement("player", 256, 256),
            new ImageRequirement("crystal", 256, 256),
            new ImageRequirement("hazard", 256, 256),
            new ImageRequirement("exit_closed", 320, 320),
            new ImageRequirement("exit_open", 320, 320),
            new ImageRequirement("ui_button", 640, 192),
            new ImageRequirement("ui_panel", 720, 440),
            new ImageRequirement("life", 128, 128),
            new ImageRequirement("lock", 128, 128),
            new ImageRequirement("pause", 128, 128),
            new ImageRequirement("arrow_up", 128, 128),
            new ImageRequirement("arrow_down", 128, 128),
            new ImageRequirement("arrow_left", 128, 128),
            new ImageRequirement("arrow_right", 128, 128),
            new ImageRequirement("arrows", 384, 384)
        };

        private static readonly string[] RequiredAudioNames =
        {
            "collect",
            "error",
            "damage",
            "victory",
            "defeat",
            "button"
        };

        private static readonly string[] RequiredDocumentationPaths =
        {
            "Release/Docs/DESCRIPTION.md",
            "Release/Docs/PLAYER_GUIDE.md",
            "Release/Docs/CREDITS.md",
            "Release/Docs/CHANGELOG.md",
            "Release/storyboard.md"
        };

        private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        [MenuItem("EnergoKey/Validation/Run Structural Validation")]
        public static void RunAll()
        {
            ValidationReport report;
            try
            {
                report = ValidateProject();
            }
            catch (Exception exception)
            {
                report = NewReport();
                AddCheck(
                    report,
                    "VAL-FATAL",
                    false,
                    "Validator completes without an unhandled exception",
                    $"{exception.GetType().Name}: {exception.Message}");
                FinalizeReport(report);
                Debug.LogException(exception);
            }

            try
            {
                WriteReports(report);
            }
            catch (Exception exception)
            {
                Debug.LogError($"ENERGOKEY_VALIDATION_REPORT_WRITE_FAILED: {exception}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(2);
                }
                return;
            }

            string message =
                $"ENERGOKEY_VALIDATION_{report.overallStatus}: " +
                $"{report.passedChecks}/{report.totalChecks} checks passed; " +
                $"report={EvidenceMarkdownPath}";

            if (report.failedChecks == 0)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogError(message);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(report.failedChecks == 0 ? 0 : 1);
            }
        }

        private static ValidationReport ValidateProject()
        {
            ValidationReport report = NewReport();

            ValidateEnvironment(report);
            ValidateRules(report);
            ValidatePlayerSettings(report);
            ValidateScenes(report);
            ValidatePrefabs(report);
            ValidateArt(report);
            ValidateAudio(report);
            ValidateRelease(report);
            ValidateTestInfrastructure(report);
            PopulateCounts(report);
            FinalizeReport(report);

            return report;
        }

        private static ValidationReport NewReport()
        {
            ValidationReport report = new ValidationReport
            {
                schemaVersion = "1.0",
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                unityVersion = Application.unityVersion,
                projectRoot = ProjectRoot,
                checks = new List<ValidationCheck>(),
                manifest = new List<ManifestEntry>(),
                limitations = new List<string>(),
                counts = new ProjectCounts()
            };

            report.limitations.Add("Structural validation does not prove that all three levels are playable from start to finish.");
            report.limitations.Add("It does not replace T01–T16 observations, a new-user test, physical-device testing or screenshots.");
            report.limitations.Add("It does not create or validate Profiler before/after measurements.");
            report.limitations.Add("Safe Area and 16:9, 18:9 and 4:3 layouts still require visual checks in Game View.");
            return report;
        }

        private static void ValidateEnvironment(ValidationReport report)
        {
            AddCheck(
                report,
                "ENV-UNITY-VERSION",
                Application.unityVersion == ExpectedUnityVersion,
                ExpectedUnityVersion,
                Application.unityVersion);

            AddFileCheck(report, "ENV-PACKAGES", "Packages/manifest.json");
            AddFileCheck(report, "ENV-PROJECT-VERSION", "ProjectSettings/ProjectVersion.txt");

            string packages = ReadTextOrEmpty("Packages/manifest.json");
            AddCheck(
                report,
                "ENV-TEST-FRAMEWORK",
                packages.Contains("com.unity.test-framework"),
                "Packages/manifest.json contains com.unity.test-framework",
                packages.Contains("com.unity.test-framework") ? "dependency present" : "dependency missing");

            AddCheck(
                report,
                "ENV-2D-SPRITE-PACKAGE",
                packages.Contains("com.unity.2d.sprite"),
                "Packages/manifest.json contains com.unity.2d.sprite",
                packages.Contains("com.unity.2d.sprite") ? "dependency present" : "dependency missing");

            string editorSettings = ReadTextOrEmpty("ProjectSettings/EditorSettings.asset");
            AddCheck(
                report,
                "ENV-2D-DEFAULT-MODE",
                Regex.IsMatch(editorSettings, @"(?m)^\s*m_DefaultBehaviorMode:\s*1\s*$"),
                "ProjectSettings/EditorSettings.asset uses 2D default behavior mode (1)",
                Regex.IsMatch(editorSettings, @"(?m)^\s*m_DefaultBehaviorMode:\s*1\s*$")
                    ? "m_DefaultBehaviorMode: 1"
                    : "2D default behavior mode not found");
        }

        private static void ValidateRules(ValidationReport report)
        {
            AddCheck(report, "RULE-MAX-LEVEL", GameRules.MaxLevel == 3, "3", GameRules.MaxLevel.ToString());
            AddCheck(report, "RULE-START-LIVES", GameRules.StartingLives == 3, "3", GameRules.StartingLives.ToString());

            int[] expectedCrystals = { 3, 4, 5 };
            int[] actualCrystals = Enumerable.Range(1, GameRules.MaxLevel).Select(GameRules.CrystalCountForLevel).ToArray();
            AddCheck(
                report,
                "RULE-CRYSTAL-COUNTS",
                actualCrystals.SequenceEqual(expectedCrystals),
                string.Join(",", expectedCrystals),
                string.Join(",", actualCrystals));

            int[] expectedHazards = { 2, 4, 6 };
            int[] actualHazards = Enumerable.Range(1, GameRules.MaxLevel).Select(GameRules.HazardCountForLevel).ToArray();
            AddCheck(
                report,
                "RULE-HAZARD-COUNTS",
                actualHazards.SequenceEqual(expectedHazards),
                string.Join(",", expectedHazards),
                string.Join(",", actualHazards));

            AddCheck(
                report,
                "RULE-SCORE",
                GameRules.CalculateScore(3, 2) == 400 && GameRules.CalculateScore(-1, -1) == 0,
                "3 crystals + 2 lives = 400; negative inputs clamp to 0",
                $"score(3,2)={GameRules.CalculateScore(3, 2)}; score(-1,-1)={GameRules.CalculateScore(-1, -1)}");

            string[] keys =
            {
                SaveService.UnlockedLevelKey,
                SaveService.BestScoreKey,
                SaveService.MasterVolumeKey,
                SaveService.TutorialSeenKey
            };
            AddCheck(
                report,
                "SAVE-KEYS-UNIQUE",
                keys.Distinct().Count() == keys.Length,
                "4 distinct PlayerPrefs keys",
                $"distinct={keys.Distinct().Count()}, total={keys.Length}");
        }

        private static void ValidatePlayerSettings(ValidationReport report)
        {
            AddCheck(
                report,
                "PLAYER-PRODUCT-NAME",
                PlayerSettings.productName == "Энергоключ",
                "Энергоключ",
                PlayerSettings.productName);
            AddCheck(
                report,
                "PLAYER-VERSION",
                PlayerSettings.bundleVersion == ExpectedGameVersion,
                ExpectedGameVersion,
                PlayerSettings.bundleVersion);

            UIOrientation orientation = PlayerSettings.defaultInterfaceOrientation;
            bool landscapeDefault =
                orientation == UIOrientation.LandscapeLeft ||
                orientation == UIOrientation.LandscapeRight ||
                orientation == UIOrientation.AutoRotation;
            bool landscapeAllowed =
                PlayerSettings.allowedAutorotateToLandscapeLeft &&
                PlayerSettings.allowedAutorotateToLandscapeRight;
            bool portraitDisabled =
                !PlayerSettings.allowedAutorotateToPortrait &&
                !PlayerSettings.allowedAutorotateToPortraitUpsideDown;

            AddCheck(
                report,
                "PLAYER-ORIENTATION",
                landscapeDefault && landscapeAllowed && portraitDisabled,
                "landscape default; both landscape rotations allowed; portrait rotations disabled",
                $"default={orientation}; left={PlayerSettings.allowedAutorotateToLandscapeLeft}; " +
                $"right={PlayerSettings.allowedAutorotateToLandscapeRight}; " +
                $"portrait={PlayerSettings.allowedAutorotateToPortrait}; " +
                $"portraitUpsideDown={PlayerSettings.allowedAutorotateToPortraitUpsideDown}");

            string readme = ReadTextOrEmpty("README.md");
            AddCheck(
                report,
                "PLAYER-README-VERSION",
                readme.Contains(ExpectedGameVersion),
                $"README.md contains {ExpectedGameVersion}",
                readme.Contains(ExpectedGameVersion) ? "version found" : "version not found");
            AddManifestIfPresent(report, "README.md");
        }

        private static void ValidateScenes(ValidationReport report)
        {
            string bootstrapGuid = AssetDatabase.AssetPathToGUID("Assets/Scripts/Core/SceneBootstrap.cs");
            foreach (string scenePath in RequiredScenePaths)
            {
                bool exists = AddFileCheck(report, $"SCENE-FILE-{Path.GetFileNameWithoutExtension(scenePath)}", scenePath);
                if (!exists)
                {
                    continue;
                }

                string yaml = ReadTextOrEmpty(scenePath);
                bool containsBootstrap = !string.IsNullOrEmpty(bootstrapGuid) && yaml.Contains($"guid: {bootstrapGuid}");
                AddCheck(
                    report,
                    $"SCENE-BOOTSTRAP-{Path.GetFileNameWithoutExtension(scenePath)}",
                    containsBootstrap,
                    "scene references SceneBootstrap",
                    containsBootstrap ? "reference found" : "reference not found");
            }

            HashSet<string> enabledBuildScenes = new HashSet<string>(
                EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => Normalize(scene.path)),
                StringComparer.Ordinal);

            foreach (string requiredPath in RequiredBuildScenePaths)
            {
                AddCheck(
                    report,
                    $"BUILD-SCENE-{Path.GetFileNameWithoutExtension(requiredPath)}",
                    enabledBuildScenes.Contains(requiredPath),
                    $"enabled in Build Settings: {requiredPath}",
                    enabledBuildScenes.Contains(requiredPath) ? "enabled" : "missing or disabled");
            }

            string firstScene = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled)?.path ?? string.Empty;
            AddCheck(
                report,
                "BUILD-FIRST-SCENE",
                Normalize(firstScene) == "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/MainMenu.unity",
                string.IsNullOrEmpty(firstScene) ? "no enabled build scene" : Normalize(firstScene));
        }

        private static void ValidatePrefabs(ValidationReport report)
        {
            ValidatePrefab(
                report,
                "PREFAB-PLAYER",
                "Assets/Resources/Prefabs/Player.prefab",
                prefab =>
                    prefab.GetComponent<PlayerController>() != null &&
                    prefab.GetComponent<Rigidbody2D>() != null &&
                    prefab.GetComponent<Collider2D>() != null &&
                    prefab.GetComponent<SpriteRenderer>() != null &&
                    prefab.tag == "Player",
                "PlayerController + Rigidbody2D + Collider2D + SpriteRenderer + Player tag");

            ValidatePrefab(
                report,
                "PREFAB-COLLECTIBLE",
                "Assets/Resources/Prefabs/Collectible.prefab",
                prefab =>
                {
                    Collider2D collider = prefab.GetComponent<Collider2D>();
                    return prefab.GetComponent<Collectible>() != null &&
                           prefab.GetComponent<SpriteRenderer>() != null &&
                           collider != null && collider.isTrigger;
                },
                "Collectible + SpriteRenderer + trigger Collider2D");

            ValidatePrefab(
                report,
                "PREFAB-HAZARD",
                "Assets/Resources/Prefabs/Hazard.prefab",
                prefab =>
                {
                    Collider2D collider = prefab.GetComponent<Collider2D>();
                    return prefab.GetComponent<Hazard>() != null &&
                           prefab.GetComponent<SpriteRenderer>() != null &&
                           collider != null && collider.isTrigger;
                },
                "Hazard + SpriteRenderer + trigger Collider2D");

            ValidatePrefab(
                report,
                "PREFAB-UI-BUTTON",
                "Assets/Resources/Prefabs/UIButton.prefab",
                prefab => prefab.GetComponent<Button>() != null && prefab.GetComponent<Image>() != null,
                "Button + Image");
        }

        private static void ValidateArt(ValidationReport report)
        {
            foreach (ImageRequirement requirement in ArtRequirements)
            {
                string path = $"Assets/Resources/Art/Final/{requirement.name}.png";
                bool exists = AddFileCheck(report, $"ART-FILE-{requirement.name}", path);
                if (!exists)
                {
                    continue;
                }

                (int width, int height, bool decoded) = ReadPngSize(path);
                AddCheck(
                    report,
                    $"ART-SIZE-{requirement.name}",
                    decoded && width == requirement.width && height == requirement.height,
                    $"{requirement.width}x{requirement.height}",
                    decoded ? $"{width}x{height}" : "PNG decode failed");

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                bool spriteReady = importer != null && importer.textureType == TextureImporterType.Sprite && sprite != null;
                AddCheck(
                    report,
                    $"ART-IMPORT-{requirement.name}",
                    spriteReady,
                    "TextureImporterType.Sprite and Sprite asset load succeeds",
                    importer == null
                        ? "TextureImporter missing"
                        : $"type={importer.textureType}; spriteLoaded={sprite != null}");
            }
        }

        private static void ValidateAudio(ValidationReport report)
        {
            foreach (string audioName in RequiredAudioNames)
            {
                string path = $"Assets/Resources/Audio/{audioName}.wav";
                bool exists = AddFileCheck(report, $"AUDIO-FILE-{audioName}", path);
                if (!exists)
                {
                    continue;
                }

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                bool valid = clip != null && clip.channels == 1 && clip.frequency == 44100 && clip.length > 0f;
                string actual = clip == null
                    ? "AudioClip import failed"
                    : $"channels={clip.channels}; frequency={clip.frequency}; length={clip.length:F3}s";
                AddCheck(
                    report,
                    $"AUDIO-IMPORT-{audioName}",
                    valid,
                    "mono, 44100 Hz, non-empty AudioClip",
                    actual);
            }
        }

        private static void ValidateRelease(ValidationReport report)
        {
            ValidateExternalImage(report, "RELEASE-ICON", "Release/icon_512.png", 512, 512);
            ValidateExternalImage(report, "RELEASE-PROMO", "Release/promo_1280x720.png", 1280, 720);
            ValidateExternalImage(report, "RELEASE-PRODUCT-MOCKUP", "Release/product_page_mockup.png", 1440, 1800);

            foreach (string path in RequiredDocumentationPaths)
            {
                AddFileCheck(report, $"DOC-{Path.GetFileNameWithoutExtension(path)}", path);
            }

            string description = ReadTextOrEmpty("Release/Docs/DESCRIPTION.md");
            string shortDescription = ParagraphAfterHeading(description, "## Краткое описание");
            string fullDescription = ParagraphAfterHeading(description, "## Полное описание");
            AddCheck(
                report,
                "DOC-SHORT-DESCRIPTION-LENGTH",
                shortDescription.Length > 0 && shortDescription.Length <= 80,
                "1..80 characters",
                $"{shortDescription.Length} characters");
            AddCheck(
                report,
                "DOC-FULL-DESCRIPTION-LENGTH",
                fullDescription.Length >= 600 && fullDescription.Length <= 1200,
                "600..1200 characters",
                $"{fullDescription.Length} characters");

            string storyboard = ReadTextOrEmpty("Release/storyboard.md");
            int storyboardRows = Regex.Matches(storyboard, @"(?m)^\|\s*[1-7]\s*\|").Count;
            AddCheck(
                report,
                "DOC-STORYBOARD-ROWS",
                storyboardRows == 7 && storyboard.Contains("80 секунд"),
                "7 rows and documented 80-second duration",
                $"rows={storyboardRows}; duration80={storyboard.Contains("80 секунд")}");
        }

        private static void ValidateTestInfrastructure(ValidationReport report)
        {
            string[] testFiles =
            {
                "Assets/Scripts/EnergoKey.Runtime.asmdef",
                "Assets/Tests/EditMode/EnergoKey.EditModeTests.asmdef",
                "Assets/Tests/EditMode/GameRulesTests.cs",
                "Assets/Tests/EditMode/SaveServiceTests.cs",
                "Assets/Editor/Validation/EnergoKeyValidation.cs"
            };

            foreach (string path in testFiles)
            {
                AddFileCheck(report, $"TEST-FILE-{Path.GetFileNameWithoutExtension(path)}", path);
            }

            string saveService = ReadTextOrEmpty("Assets/Scripts/Save/SaveService.cs");
            AddCheck(
                report,
                "TEST-PLAYERPREFS-ISOLATION",
                saveService.Contains("UseTemporaryKeysForTests") && saveService.Contains("energokey.tests."),
                "SaveService exposes Editor-only isolated test keys with cleanup",
                saveService.Contains("UseTemporaryKeysForTests") ? "test scope found" : "test scope missing");
        }

        private static void PopulateCounts(ValidationReport report)
        {
            report.counts.sceneFiles = CountFiles("Assets/Scenes", "*.unity");
            report.counts.prefabFiles = CountFiles("Assets/Resources/Prefabs", "*.prefab");
            report.counts.finalArtPngFiles = CountFiles("Assets/Resources/Art/Final", "*.png");
            report.counts.audioWavFiles = CountFiles("Assets/Resources/Audio", "*.wav");
            report.counts.releasePngFiles = CountFiles("Release", "*.png");
            report.counts.editModeTestSourceFiles = CountFiles("Assets/Tests/EditMode", "*.cs");

            AddCheck(report, "COUNT-SCENES", report.counts.sceneFiles >= 6, ">= 6", report.counts.sceneFiles.ToString());
            AddCheck(report, "COUNT-PREFABS", report.counts.prefabFiles >= 4, ">= 4", report.counts.prefabFiles.ToString());
            AddCheck(report, "COUNT-FINAL-ART", report.counts.finalArtPngFiles >= ArtRequirements.Length, $">= {ArtRequirements.Length}", report.counts.finalArtPngFiles.ToString());
            AddCheck(report, "COUNT-AUDIO", report.counts.audioWavFiles >= RequiredAudioNames.Length, $">= {RequiredAudioNames.Length}", report.counts.audioWavFiles.ToString());
            AddCheck(report, "COUNT-RELEASE-PNG", report.counts.releasePngFiles >= 3, ">= 3", report.counts.releasePngFiles.ToString());
            AddCheck(report, "COUNT-EDITMODE-TESTS", report.counts.editModeTestSourceFiles >= 2, ">= 2", report.counts.editModeTestSourceFiles.ToString());
        }

        private static bool AddFileCheck(ValidationReport report, string id, string relativePath)
        {
            string absolutePath = Absolute(relativePath);
            bool exists = File.Exists(absolutePath);
            AddCheck(report, id, exists, $"file exists: {relativePath}", exists ? "found" : "missing");

            if (!exists)
            {
                return false;
            }

            AddManifestIfPresent(report, relativePath);
            if (relativePath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                string metaPath = absolutePath + ".meta";
                AddCheck(
                    report,
                    $"{id}-META",
                    File.Exists(metaPath),
                    $"meta exists: {relativePath}.meta",
                    File.Exists(metaPath) ? "found" : "missing");
                AddManifestIfPresent(report, relativePath + ".meta");
            }

            return true;
        }

        private static void ValidatePrefab(
            ValidationReport report,
            string id,
            string path,
            Func<GameObject, bool> predicate,
            string expected)
        {
            bool exists = AddFileCheck(report, $"{id}-FILE", path);
            if (!exists)
            {
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            bool valid = prefab != null && predicate(prefab);
            AddCheck(
                report,
                id,
                valid,
                expected,
                prefab == null ? "prefab load failed" : valid ? "required components found" : "component or setting missing");
        }

        private static void ValidateExternalImage(
            ValidationReport report,
            string id,
            string path,
            int expectedWidth,
            int expectedHeight)
        {
            bool exists = AddFileCheck(report, $"{id}-FILE", path);
            if (!exists)
            {
                return;
            }

            (int width, int height, bool decoded) = ReadPngSize(path);
            AddCheck(
                report,
                id,
                decoded && width == expectedWidth && height == expectedHeight,
                $"{expectedWidth}x{expectedHeight}",
                decoded ? $"{width}x{height}" : "PNG decode failed");
        }

        private static (int width, int height, bool decoded) ReadPngSize(string relativePath)
        {
            string path = Absolute(relativePath);
            if (!File.Exists(path))
            {
                return (0, 0, false);
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                bool decoded = ImageConversion.LoadImage(texture, File.ReadAllBytes(path), true);
                return decoded ? (texture.width, texture.height, true) : (0, 0, false);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static string ParagraphAfterHeading(string text, string heading)
        {
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                if (!string.Equals(lines[index].Trim(), heading, StringComparison.Ordinal))
                {
                    continue;
                }

                for (int next = index + 1; next < lines.Length; next++)
                {
                    string candidate = lines[next].Trim();
                    if (!string.IsNullOrEmpty(candidate))
                    {
                        return candidate;
                    }
                }
            }

            return string.Empty;
        }

        private static int CountFiles(string relativeDirectory, string pattern)
        {
            string directory = Absolute(relativeDirectory);
            return Directory.Exists(directory)
                ? Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly).Length
                : 0;
        }

        private static string ReadTextOrEmpty(string relativePath)
        {
            string path = Absolute(relativePath);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        }

        private static string Absolute(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(ProjectRoot, Normalize(relativePath)));
        }

        private static string Normalize(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        private static void AddCheck(
            ValidationReport report,
            string id,
            bool passed,
            string expected,
            string actual)
        {
            report.checks.Add(new ValidationCheck
            {
                id = id,
                status = passed ? "PASS" : "FAIL",
                expected = expected,
                actual = actual
            });
        }

        private static void AddManifestIfPresent(ValidationReport report, string relativePath)
        {
            string normalized = Normalize(relativePath);
            if (report.manifest.Any(entry => entry.path == normalized))
            {
                return;
            }

            string absolutePath = Absolute(normalized);
            if (!File.Exists(absolutePath))
            {
                return;
            }

            FileInfo info = new FileInfo(absolutePath);
            report.manifest.Add(new ManifestEntry
            {
                path = normalized,
                bytes = info.Length,
                sha256 = Sha256(absolutePath)
            });
        }

        private static string Sha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static void FinalizeReport(ValidationReport report)
        {
            report.manifest = report.manifest.OrderBy(entry => entry.path, StringComparer.Ordinal).ToList();
            report.totalChecks = report.checks.Count;
            report.passedChecks = report.checks.Count(check => check.status == "PASS");
            report.failedChecks = report.checks.Count(check => check.status == "FAIL");
            report.overallStatus = report.failedChecks == 0 ? "PASS" : "FAIL";
        }

        private static void WriteReports(ValidationReport report)
        {
            string jsonPath = Absolute(EvidenceJsonPath);
            string markdownPath = Absolute(EvidenceMarkdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath) ?? Absolute("EVIDENCE"));
            File.WriteAllText(jsonPath, JsonUtility.ToJson(report, true) + Environment.NewLine, Encoding.UTF8);
            File.WriteAllText(markdownPath, BuildMarkdown(report), Encoding.UTF8);
        }

        private static string BuildMarkdown(ValidationReport report)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("# Unity project structural validation");
            output.AppendLine();
            output.AppendLine($"- Generated by an actual Unity Editor run: `{Escape(report.generatedAtUtc)}`");
            output.AppendLine($"- Unity: `{Escape(report.unityVersion)}`");
            output.AppendLine($"- Overall: **{Escape(report.overallStatus)}**");
            output.AppendLine($"- Checks: {report.passedChecks} PASS, {report.failedChecks} FAIL, {report.totalChecks} total");
            output.AppendLine();
            output.AppendLine("## Checks");
            output.AppendLine();
            output.AppendLine("| ID | Status | Expected | Actual |");
            output.AppendLine("|---|---|---|---|");
            foreach (ValidationCheck check in report.checks)
            {
                output.AppendLine($"| {Escape(check.id)} | {Escape(check.status)} | {Escape(check.expected)} | {Escape(check.actual)} |");
            }

            output.AppendLine();
            output.AppendLine("## Counts");
            output.AppendLine();
            output.AppendLine($"- Scenes: {report.counts.sceneFiles}");
            output.AppendLine($"- Prefabs: {report.counts.prefabFiles}");
            output.AppendLine($"- Final art PNG: {report.counts.finalArtPngFiles}");
            output.AppendLine($"- Audio WAV: {report.counts.audioWavFiles}");
            output.AppendLine($"- Release PNG: {report.counts.releasePngFiles}");
            output.AppendLine($"- EditMode test source files: {report.counts.editModeTestSourceFiles}");

            output.AppendLine();
            output.AppendLine("## Scope limitations");
            output.AppendLine();
            foreach (string limitation in report.limitations)
            {
                output.AppendLine($"- {limitation}");
            }

            output.AppendLine();
            output.AppendLine("## File manifest");
            output.AppendLine();
            output.AppendLine("| Path | Bytes | SHA-256 |");
            output.AppendLine("|---|---:|---|");
            foreach (ManifestEntry entry in report.manifest)
            {
                output.AppendLine($"| {Escape(entry.path)} | {entry.bytes} | `{entry.sha256}` |");
            }

            return output.ToString();
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private readonly struct ImageRequirement
        {
            public readonly string name;
            public readonly int width;
            public readonly int height;

            public ImageRequirement(string assetName, int assetWidth, int assetHeight)
            {
                name = assetName;
                width = assetWidth;
                height = assetHeight;
            }
        }

        [Serializable]
        public sealed class ValidationReport
        {
            public string schemaVersion;
            public string generatedAtUtc;
            public string unityVersion;
            public string projectRoot;
            public string overallStatus;
            public int totalChecks;
            public int passedChecks;
            public int failedChecks;
            public ProjectCounts counts;
            public List<ValidationCheck> checks;
            public List<ManifestEntry> manifest;
            public List<string> limitations;
        }

        [Serializable]
        public sealed class ProjectCounts
        {
            public int sceneFiles;
            public int prefabFiles;
            public int finalArtPngFiles;
            public int audioWavFiles;
            public int releasePngFiles;
            public int editModeTestSourceFiles;
        }

        [Serializable]
        public sealed class ValidationCheck
        {
            public string id;
            public string status;
            public string expected;
            public string actual;
        }

        [Serializable]
        public sealed class ManifestEntry
        {
            public string path;
            public long bytes;
            public string sha256;
        }
    }
}
#endif
