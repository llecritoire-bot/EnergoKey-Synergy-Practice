#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EnergoKey.Editor.Testing
{
    /// <summary>
    /// Two-process PlayerPrefs probe. Run Write in one Unity invocation and Read
    /// in a second invocation. An isolated key prefix protects production saves.
    /// </summary>
    public static class PersistenceRestartProbe
    {
        private const string ProbePrefix = "energokey.restart_probe.";
        private const string JsonPath = "EVIDENCE/PERSISTENCE_RESTART_PROBE.json";
        private const string MarkdownPath = "EVIDENCE/PERSISTENCE_RESTART_PROBE.md";
        private const int ExpectedUnlockedLevel = 3;
        private const int ExpectedBestScore = 650;
        private const float ExpectedVolume = 0.37f;

        [Serializable]
        private sealed class ProbeResult
        {
            public string status;
            public string unityVersion;
            public string phaseOneUtc;
            public string phaseTwoUtc;
            public int processCount;
            public int unlockedLevel;
            public int bestScore;
            public float masterVolume;
            public bool tutorialSeen;
            public bool valuesMatched;
            public bool isolatedKeysCleaned;
        }

        public static void Write()
        {
            try
            {
                UseProbePrefix();
                DeleteProbeKeys();

                SaveService.RegisterVictory(1, 450);
                SaveService.RegisterVictory(2, ExpectedBestScore);
                SaveService.SetMasterVolume(ExpectedVolume);
                SaveService.MarkTutorialSeen();
                PlayerPrefs.SetString(ProbePrefix + "phaseOneUtc", DateTime.UtcNow.ToString("O"));
                PlayerPrefs.Save();

                ProbeResult result = Capture("PHASE_ONE_WRITTEN", false, false);
                result.processCount = 1;
                result.phaseOneUtc = PlayerPrefs.GetString(ProbePrefix + "phaseOneUtc", string.Empty);
                WriteJson(result);
                Debug.Log("ENERGOKEY_PERSISTENCE_PROBE: phase 1 written; close/reopen boundary ready");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(2);
            }
        }

        public static void Read()
        {
            bool passed = false;
            try
            {
                UseProbePrefix();
                string phaseOneUtc = PlayerPrefs.GetString(ProbePrefix + "phaseOneUtc", string.Empty);
                passed =
                    SaveService.UnlockedLevel == ExpectedUnlockedLevel &&
                    SaveService.BestScore == ExpectedBestScore &&
                    Mathf.Abs(SaveService.MasterVolume - ExpectedVolume) < 0.0001f &&
                    SaveService.TutorialSeen &&
                    !string.IsNullOrEmpty(phaseOneUtc);

                ProbeResult result = Capture(passed ? "PASS" : "FAIL", passed, false);
                result.processCount = 2;
                result.phaseOneUtc = phaseOneUtc;
                result.phaseTwoUtc = DateTime.UtcNow.ToString("O");

                DeleteProbeKeys();
                result.isolatedKeysCleaned = ProbeKeysAreAbsent();
                WriteJson(result);
                WriteMarkdown(result);

                Debug.Log($"ENERGOKEY_PERSISTENCE_PROBE: {result.status}; two independent Unity processes; isolated keys cleaned={result.isolatedKeysCleaned}");
                EditorApplication.Exit(passed && result.isolatedKeysCleaned ? 0 : 3);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(2);
            }
        }

        private static ProbeResult Capture(string status, bool valuesMatched, bool isolatedKeysCleaned)
        {
            return new ProbeResult
            {
                status = status,
                unityVersion = Application.unityVersion,
                unlockedLevel = SaveService.UnlockedLevel,
                bestScore = SaveService.BestScore,
                masterVolume = SaveService.MasterVolume,
                tutorialSeen = SaveService.TutorialSeen,
                valuesMatched = valuesMatched,
                isolatedKeysCleaned = isolatedKeysCleaned
            };
        }

        private static void UseProbePrefix()
        {
            FieldInfo prefixField = typeof(SaveService).GetField("keyPrefix", BindingFlags.NonPublic | BindingFlags.Static);
            if (prefixField == null)
            {
                throw new MissingFieldException(typeof(SaveService).FullName, "keyPrefix");
            }

            prefixField.SetValue(null, ProbePrefix);
        }

        private static void DeleteProbeKeys()
        {
            PlayerPrefs.DeleteKey(ProbePrefix + SaveService.UnlockedLevelKey);
            PlayerPrefs.DeleteKey(ProbePrefix + SaveService.BestScoreKey);
            PlayerPrefs.DeleteKey(ProbePrefix + SaveService.MasterVolumeKey);
            PlayerPrefs.DeleteKey(ProbePrefix + SaveService.TutorialSeenKey);
            PlayerPrefs.DeleteKey(ProbePrefix + "phaseOneUtc");
            PlayerPrefs.Save();
        }

        private static bool ProbeKeysAreAbsent()
        {
            return
                !PlayerPrefs.HasKey(ProbePrefix + SaveService.UnlockedLevelKey) &&
                !PlayerPrefs.HasKey(ProbePrefix + SaveService.BestScoreKey) &&
                !PlayerPrefs.HasKey(ProbePrefix + SaveService.MasterVolumeKey) &&
                !PlayerPrefs.HasKey(ProbePrefix + SaveService.TutorialSeenKey) &&
                !PlayerPrefs.HasKey(ProbePrefix + "phaseOneUtc");
        }

        private static void WriteJson(ProbeResult result)
        {
            string absolutePath = Path.GetFullPath(JsonPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, JsonUtility.ToJson(result, true) + Environment.NewLine, new UTF8Encoding(false));
        }

        private static void WriteMarkdown(ProbeResult result)
        {
            StringBuilder markdown = new StringBuilder();
            markdown.AppendLine("# Проверка сохранений между независимыми запусками Unity");
            markdown.AppendLine();
            markdown.AppendLine($"- Статус: `{result.status}`");
            markdown.AppendLine($"- Unity: `{result.unityVersion}`");
            markdown.AppendLine("- Процессов Unity: `2` (WRITE завершился до запуска READ)");
            markdown.AppendLine($"- Фаза WRITE, UTC: `{result.phaseOneUtc}`");
            markdown.AppendLine($"- Фаза READ, UTC: `{result.phaseTwoUtc}`");
            markdown.AppendLine($"- Максимальный открытый уровень: `{result.unlockedLevel}` (ожидалось `{ExpectedUnlockedLevel}`)");
            markdown.AppendLine($"- Лучший результат: `{result.bestScore}` (ожидалось `{ExpectedBestScore}`)");
            markdown.AppendLine($"- Громкость: `{result.masterVolume:0.00}` (ожидалось `{ExpectedVolume:0.00}`)");
            markdown.AppendLine($"- Обучение просмотрено: `{result.tutorialSeen}`");
            markdown.AppendLine($"- Все значения совпали: `{result.valuesMatched}`");
            markdown.AppendLine($"- Изолированные тестовые ключи удалены: `{result.isolatedKeysCleaned}`");
            markdown.AppendLine();
            markdown.AppendLine("Проверка использует отдельный префикс PlayerPrefs и не читает, не изменяет и не удаляет пользовательское сохранение игры.");

            string absolutePath = Path.GetFullPath(MarkdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, markdown.ToString(), new UTF8Encoding(false));
        }
    }
}
#endif
