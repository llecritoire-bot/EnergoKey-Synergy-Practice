using UnityEditor;
using UnityEngine;

public static class EnergoKeyEvidenceMenu
{
    [MenuItem("EnergoKey/Evidence/Toggle Pause _F7")]
    public static void TogglePause()
    {
        if (!TryGetRuntimeComponent(out PauseController pauseController)) return;
        pauseController.TogglePause();
        Debug.Log("ENERGOKEY_EVIDENCE: pause toggled from the Editor menu");
    }

    [MenuItem("EnergoKey/Evidence/Collect All Crystals")]
    public static void CollectAllCrystals()
    {
        if (!TryGetRuntimeComponent(out GameController controller)) return;
        foreach (Collectible collectible in Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None))
        {
            if (collectible != null && collectible.gameObject.activeInHierarchy)
            {
                controller.Collect(collectible);
            }
        }
    }

    [MenuItem("EnergoKey/Evidence/Show Locked Exit Feedback")]
    public static void ShowLockedExitFeedback()
    {
        if (TryGetRuntimeComponent(out GameController controller)) controller.TryUseExit();
    }

    [MenuItem("EnergoKey/Evidence/Show Victory")]
    public static void ShowVictory()
    {
        if (!TryGetRuntimeComponent(out GameController controller)) return;
        CollectAllCrystals();
        controller.TryUseExit();
    }

    [MenuItem("EnergoKey/Evidence/Show Defeat")]
    public static void ShowDefeat()
    {
        if (TryGetRuntimeComponent(out GameController controller))
        {
            controller.SendMessage("DefeatForEvidence", SendMessageOptions.DontRequireReceiver);
        }
    }

    [MenuItem("EnergoKey/Evidence/Toggle Safe Area Simulation")]
    public static void ToggleSafeAreaSimulation()
    {
        if (!TryGetRuntimeComponent(out SafeAreaAdapter adapter)) return;
        adapter.SetEditorSimulation(!adapter.IsSimulating);
        adapter.SetDebugBorderVisible(adapter.IsSimulating);
        Debug.Log($"ENERGOKEY_EVIDENCE: Safe Area simulation = {adapter.IsSimulating}");
    }

    [MenuItem("EnergoKey/Evidence/Unlock All Levels")]
    public static void UnlockAllLevels()
    {
        SaveService.UnlockAllLevelsForEvidence();
        Debug.Log("ENERGOKEY_EVIDENCE: all levels unlocked");
    }

    private static bool TryGetRuntimeComponent<T>(out T component) where T : Object
    {
        component = Object.FindFirstObjectByType<T>();
        if (EditorApplication.isPlaying && component != null) return true;

        Debug.LogWarning($"ENERGOKEY_EVIDENCE: enter Play Mode before using {typeof(T).Name}");
        return false;
    }
}
