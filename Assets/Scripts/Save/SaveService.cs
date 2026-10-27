using System;
using UnityEngine;

public static class SaveService
{
    public const string UnlockedLevelKey = "energokey.unlockedLevel";
    public const string BestScoreKey = "energokey.bestScore";
    public const string MasterVolumeKey = "energokey.masterVolume";
    public const string TutorialSeenKey = "energokey.tutorialSeen";

    private static string keyPrefix = string.Empty;

    public static int UnlockedLevel => Mathf.Clamp(PlayerPrefs.GetInt(Key(UnlockedLevelKey), 1), 1, GameRules.MaxLevel);
    public static int BestScore => Mathf.Max(0, PlayerPrefs.GetInt(Key(BestScoreKey), 0));
    public static float MasterVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(Key(MasterVolumeKey), 0.8f));
    public static bool TutorialSeen => PlayerPrefs.GetInt(Key(TutorialSeenKey), 0) == 1;

    public static void RegisterVictory(int completedLevel, int score)
    {
        if (completedLevel < GameRules.MaxLevel)
        {
            PlayerPrefs.SetInt(Key(UnlockedLevelKey), Mathf.Max(UnlockedLevel, completedLevel + 1));
        }

        if (score > BestScore)
        {
            PlayerPrefs.SetInt(Key(BestScoreKey), score);
        }

        PlayerPrefs.Save();
    }

    public static void SetMasterVolume(float value)
    {
        float safeValue = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(Key(MasterVolumeKey), safeValue);
        PlayerPrefs.Save();
        AudioListener.volume = safeValue;
    }

    public static void MarkTutorialSeen()
    {
        PlayerPrefs.SetInt(Key(TutorialSeenKey), 1);
        PlayerPrefs.Save();
    }

    public static void ResetProgressForDevelopment()
    {
        PlayerPrefs.DeleteKey(Key(UnlockedLevelKey));
        PlayerPrefs.DeleteKey(Key(BestScoreKey));
        PlayerPrefs.DeleteKey(Key(TutorialSeenKey));
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    public static void UnlockAllLevelsForEvidence()
    {
        PlayerPrefs.SetInt(Key(UnlockedLevelKey), GameRules.MaxLevel);
        PlayerPrefs.Save();
    }
#endif

    private static string Key(string productionKey)
    {
        return keyPrefix + productionKey;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Redirects SaveService to isolated PlayerPrefs keys for an Editor test.
    /// Disposing the scope deletes those keys and restores the previous prefix.
    /// Production keys are never read, written or deleted by the scope.
    /// </summary>
    public static IDisposable UseTemporaryKeysForTests(string scopeName)
    {
        if (string.IsNullOrWhiteSpace(scopeName))
        {
            throw new ArgumentException("A non-empty test scope name is required.", nameof(scopeName));
        }

        return new TemporaryKeyScope(scopeName);
    }

    private static void DeleteAllKeysForCurrentPrefix()
    {
        PlayerPrefs.DeleteKey(Key(UnlockedLevelKey));
        PlayerPrefs.DeleteKey(Key(BestScoreKey));
        PlayerPrefs.DeleteKey(Key(MasterVolumeKey));
        PlayerPrefs.DeleteKey(Key(TutorialSeenKey));
        PlayerPrefs.Save();
    }

    private sealed class TemporaryKeyScope : IDisposable
    {
        private readonly string previousPrefix;
        private bool disposed;

        public TemporaryKeyScope(string scopeName)
        {
            previousPrefix = keyPrefix;
            keyPrefix = $"energokey.tests.{scopeName}.";
            DeleteAllKeysForCurrentPrefix();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            DeleteAllKeysForCurrentPrefix();
            keyPrefix = previousPrefix;
            disposed = true;
        }
    }
#endif
}

