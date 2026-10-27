using UnityEngine;

public static class GameRules
{
    public const int MaxLevel = 3;
    public const int StartingLives = 3;

    public static int CalculateScore(int collectedCrystals, int remainingLives)
    {
        return Mathf.Max(0, collectedCrystals) * 100 + Mathf.Max(0, remainingLives) * 50;
    }

    public static int CrystalCountForLevel(int level)
    {
        return Mathf.Clamp(level, 1, MaxLevel) + 2;
    }

    public static int HazardCountForLevel(int level)
    {
        return Mathf.Clamp(level, 1, MaxLevel) * 2;
    }
}

