#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EnergoKey.Editor.Testing
{
    public static class MacStandaloneSmokeBuild
    {
        private const string DefaultBuildPath = "/tmp/EnergoKey-MacSmoke/EnergoKey.app";
        private const string FocusProbeBuildPath = "/tmp/EnergoKey-FocusProbe/EnergoKey.app";
        private const string EvidencePath = "EVIDENCE/MAC_STANDALONE_TEST.md";

        public static void Build()
        {
            BuildInternal(false);
        }

        public static void BuildFocusProbe()
        {
            BuildInternal(true);
        }

        private static void BuildInternal(bool startInLevelOne)
        {
            int exitCode = 2;
            try
            {
                string requestedPath = Environment.GetEnvironmentVariable("ENERGOKEY_MAC_BUILD_PATH");
                string defaultPath = startInLevelOne ? FocusProbeBuildPath : DefaultBuildPath;
                string buildPath = string.IsNullOrWhiteSpace(requestedPath) ? defaultPath : requestedPath;
                string parent = Path.GetDirectoryName(buildPath);
                if (string.IsNullOrWhiteSpace(parent))
                {
                    throw new InvalidOperationException("Standalone build path must have a parent directory.");
                }

                Directory.CreateDirectory(parent);
                string[] scenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray();
                if (startInLevelOne)
                {
                    scenes = scenes
                        .OrderBy(path => path == "Assets/Scenes/Level1.unity" ? 0 : 1)
                        .ToArray();
                }

                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = buildPath,
                    target = BuildTarget.StandaloneOSX,
                    options = BuildOptions.Development | BuildOptions.StrictMode
                });

                bool passed = report.summary.result == BuildResult.Succeeded;
                if (!startInLevelOne)
                {
                    WriteEvidence(report, passed);
                }

                string marker = startInLevelOne ? "ENERGOKEY_MAC_FOCUS_BUILD" : "ENERGOKEY_MAC_BUILD";
                Debug.Log($"{marker}: {(passed ? "PASS" : "FAIL")}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}; size={report.summary.totalSize}");
                exitCode = passed ? 0 : 3;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            EditorApplication.Exit(exitCode);
        }

        private static void WriteEvidence(BuildReport report, bool passed)
        {
            StringBuilder markdown = new StringBuilder();
            markdown.AppendLine("# Проверка macOS Standalone на физическом MacBook");
            markdown.AppendLine();
            markdown.AppendLine($"- Сборка: `{(passed ? "PASS" : "FAIL")}`");
            markdown.AppendLine("- Цель: `macOS Standalone (Apple Silicon compatible)`");
            markdown.AppendLine("- Тип: `Development Build`, `StrictMode`");
            markdown.AppendLine($"- Unity: `{Application.unityVersion}`");
            markdown.AppendLine($"- Версия игры: `{Application.version}`");
            markdown.AppendLine($"- Сцен в сборке: `{report.summary.platformGroup}` / `{EditorBuildSettings.scenes.Count(scene => scene.enabled)}`");
            markdown.AppendLine($"- Ошибок сборки: `{report.summary.totalErrors}`");
            markdown.AppendLine($"- Предупреждений сборки: `{report.summary.totalWarnings}`");
            markdown.AppendLine($"- Размер сборки, байт: `{report.summary.totalSize}`");
            markdown.AppendLine($"- Длительность сборки: `{report.summary.totalTime}`");
            markdown.AppendLine();
            markdown.AppendLine("Сама сборка создана во временной папке вне проекта и не включается в Git/ZIP. Фактический запуск подтверждается отдельным runtime-логом `logs/mac-standalone-smoke.log` со строкой `ENERGOKEY_RUNTIME_READY`.");
            markdown.AppendLine();
            markdown.AppendLine("MacBook с Apple M3 является физическим компьютером macOS, но не мобильным Android/iOS-устройством.");

            string absolutePath = Path.GetFullPath(EvidencePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, markdown.ToString(), new UTF8Encoding(false));
        }
    }
}
#endif
