using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EnergoKeyEvidenceMenu
{
    [MenuItem("EnergoKey/Evidence/Inspect/Player")]
    public static void InspectPlayer() => SelectRuntimeComponent<PlayerController>("Player");

    [MenuItem("EnergoKey/Evidence/Inspect/Collectible")]
    public static void InspectCollectible() => SelectRuntimeComponent<Collectible>("Collectible");

    [MenuItem("EnergoKey/Evidence/Inspect/Hazard")]
    public static void InspectHazard() => SelectRuntimeComponent<Hazard>("Hazard");

    [MenuItem("EnergoKey/Evidence/Inspect/App Canvas")]
    public static void InspectAppCanvas() => SelectRuntimeObject("AppCanvas");

    [MenuItem("EnergoKey/Evidence/Inspect/Safe Area RectTransform")]
    public static void InspectSafeArea() => SelectRuntimeObject("SafeArea");

    [MenuItem("EnergoKey/Evidence/Inspect/Final Player Sprite")]
    public static void InspectFinalPlayerSprite() => SelectProjectAsset<Texture2D>("Assets/Resources/Art/Final/player.png");

    [MenuItem("EnergoKey/Evidence/Inspect/Prefabs Folder")]
    public static void InspectPrefabsFolder()
    {
        DefaultAsset folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Resources/Prefabs");
        if (folder == null)
        {
            Debug.LogWarning("ENERGOKEY_EVIDENCE: prefabs folder was not found");
            return;
        }

        System.Type projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        EditorWindow projectBrowser = projectBrowserType == null ? null : EditorWindow.GetWindow(projectBrowserType);
        MethodInfo showFolderContents = projectBrowserType?.GetMethod(
            "ShowFolderContents",
            BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo setSearch = projectBrowserType?.GetMethod(
            "SetSearch",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(string) },
            null);

        Selection.activeObject = folder;
        projectBrowser?.Focus();
        showFolderContents?.Invoke(projectBrowser, new object[] { folder.GetEntityId(), true });
        setSearch?.Invoke(projectBrowser, new object[] { "t:Prefab" });
        projectBrowser?.Repaint();
        Debug.Log("ENERGOKEY_EVIDENCE: opened Assets/Resources/Prefabs and filtered prefab assets");
    }

    [MenuItem("EnergoKey/Evidence/Inspect/Level World In Scene View")]
    public static void InspectLevelWorldInSceneView()
    {
        GameObject world = GameObject.Find($"{SceneManager.GetActiveScene().name}_World");
        if (world == null)
        {
            Debug.LogWarning("ENERGOKEY_EVIDENCE: level world was not found in the current scene");
            return;
        }

        Selection.activeGameObject = world;
        SceneView sceneView = EditorWindow.GetWindow<SceneView>();
        sceneView.Focus();
        sceneView.FrameSelected();
        sceneView.Repaint();
    }

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
        foreach (Collectible collectible in Object.FindObjectsByType<Collectible>(FindObjectsInactive.Exclude))
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

    [MenuItem("EnergoKey/Evidence/Open/Main Menu")]
    public static void OpenMainMenu() => LoadScene("MainMenu");

    [MenuItem("EnergoKey/Evidence/Open/Level Select")]
    public static void OpenLevelSelect() => LoadScene("LevelSelect");

    [MenuItem("EnergoKey/Evidence/Open/Level 1")]
    public static void OpenLevel1() => LoadScene("Level1");

    [MenuItem("EnergoKey/Evidence/Open/Level 2")]
    public static void OpenLevel2() => LoadScene("Level2");

    [MenuItem("EnergoKey/Evidence/Open/Level 3")]
    public static void OpenLevel3() => LoadScene("Level3");

    [MenuItem("EnergoKey/Evidence/Enter Play Mode In Level 1")]
    public static void EnterPlayModeInLevel1()
    {
        if (EditorApplication.isPlaying) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
        EditorApplication.EnterPlaymode();
    }

    [MenuItem("EnergoKey/Evidence/Open/Tutorial Card 1")]
    public static void OpenTutorial()
    {
        if (TryGetRuntimeComponent(out TutorialController tutorial)) tutorial.OpenFromMenu();
    }

    [MenuItem("EnergoKey/Evidence/Open/Tutorial Next Card")]
    public static void NextTutorialCard()
    {
        if (TryGetRuntimeComponent(out TutorialController tutorial)) tutorial.NextCard();
    }

    [MenuItem("EnergoKey/Evidence/Close Tutorial And Mark Seen")]
    public static void CloseTutorialAndMarkSeen()
    {
        if (!TryGetRuntimeComponent(out TutorialController tutorial)) return;
        SaveService.MarkTutorialSeen();
        tutorial.CloseTutorial();
        Debug.Log("ENERGOKEY_EVIDENCE: tutorial closed and marked as seen");
    }

    [MenuItem("EnergoKey/Evidence/Open/Settings")]
    public static void OpenSettings()
    {
        if (TryGetRuntimeComponent(out SettingsPanelController settings)) settings.OpenPanel();
    }

    [MenuItem("EnergoKey/Evidence/Open/About")]
    public static void OpenAbout()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("ENERGOKEY_EVIDENCE: enter Play Mode before opening About");
            return;
        }

        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include))
        {
            if (button != null && button.name == "AboutButton")
            {
                button.onClick.Invoke();
                return;
            }
        }

        Debug.LogWarning("ENERGOKEY_EVIDENCE: AboutButton was not found in the current scene");
    }

    private static void LoadScene(string sceneName)
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("ENERGOKEY_EVIDENCE: enter Play Mode before loading runtime scenes");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private static bool TryGetRuntimeComponent<T>(out T component) where T : Object
    {
        component = Object.FindAnyObjectByType<T>();
        if (EditorApplication.isPlaying && component != null) return true;

        Debug.LogWarning($"ENERGOKEY_EVIDENCE: enter Play Mode before using {typeof(T).Name}");
        return false;
    }

    private static void SelectRuntimeComponent<T>(string label) where T : Component
    {
        if (!TryGetRuntimeComponent(out T component)) return;
        Selection.activeGameObject = component.gameObject;
        EditorGUIUtility.PingObject(component.gameObject);
        Debug.Log($"ENERGOKEY_EVIDENCE: selected {label}");
    }

    private static void SelectRuntimeObject(string objectName)
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning($"ENERGOKEY_EVIDENCE: enter Play Mode before selecting {objectName}");
            return;
        }

        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (transform.name != objectName) continue;
            Selection.activeGameObject = transform.gameObject;
            EditorGUIUtility.PingObject(transform.gameObject);
            Debug.Log($"ENERGOKEY_EVIDENCE: selected {objectName}");
            return;
        }

        Debug.LogWarning($"ENERGOKEY_EVIDENCE: {objectName} was not found in the current scene");
    }

    private static void SelectProjectAsset<T>(string assetPath) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset == null)
        {
            Debug.LogWarning($"ENERGOKEY_EVIDENCE: asset was not found at {assetPath}");
            return;
        }

        Selection.activeObject = asset;
        EditorUtility.FocusProjectWindow();
        EditorGUIUtility.PingObject(asset);
        Debug.Log($"ENERGOKEY_EVIDENCE: selected {assetPath}");
    }
}
