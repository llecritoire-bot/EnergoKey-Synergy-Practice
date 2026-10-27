using UnityEngine;

public static class SaveService
{
    public const string UnlockedLevelKey = "energokey.unlockedLevel";
    public const string BestScoreKey = "energokey.bestScore";
    public const string MasterVolumeKey = "energokey.masterVolume";
    public const string TutorialSeenKey = "energokey.tutorialSeen";

    public static int UnlockedLevel => Mathf.Clamp(PlayerPrefs.GetInt(UnlockedLevelKey, 1), 1, GameRules.MaxLevel);
    public static int BestScore => Mathf.Max(0, PlayerPrefs.GetInt(BestScoreKey, 0));
    public static float MasterVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 0.8f));
    public static bool TutorialSeen => PlayerPrefs.GetInt(TutorialSeenKey, 0) == 1;

    public static void RegisterVictory(int completedLevel, int score)
    {
        if (completedLevel < GameRules.MaxLevel)
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, Mathf.Max(UnlockedLevel, completedLevel + 1));
        }

        if (score > BestScore)
        {
            PlayerPrefs.SetInt(BestScoreKey, score);
        }

        PlayerPrefs.Save();
    }

    public static void SetMasterVolume(float value)
    {
        float safeValue = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, safeValue);
        PlayerPrefs.Save();
        AudioListener.volume = safeValue;
    }

    public static void MarkTutorialSeen()
    {
        PlayerPrefs.SetInt(TutorialSeenKey, 1);
        PlayerPrefs.Save();
    }

    public static void ResetProgressForDevelopment()
    {
        PlayerPrefs.DeleteKey(UnlockedLevelKey);
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.DeleteKey(TutorialSeenKey);
        PlayerPrefs.Save();
    }
}

