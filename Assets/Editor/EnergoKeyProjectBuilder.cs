#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EnergoKeyProjectBuilder
{
    private const string PrototypeArt = "Assets/Resources/Art/Prototype";
    private const string FinalArt = "Assets/Resources/Art/Final";
    private const string Prefabs = "Assets/Resources/Prefabs";

    [MenuItem("EnergoKey/Build UP PM12 Prototype")]
    public static void BuildPrototype()
    {
        EnsureFolders();
        CreatePrototypeArt();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureArtImporters();
        CreatePrefabs();
        CreateBootstrapScene("Prototype");
        ConfigurePlayerSettings();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Prototype.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("ENERGOKEY_BUILD_OK: UP.PM.12 prototype created");
    }

    public static void BuildPrototypeBatch()
    {
        BuildPrototype();
    }

    [MenuItem("EnergoKey/Build Complete Three-Level Game")]
    public static void BuildFullGame()
    {
        EnsureFolders();
        CreatePrototypeArt();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureArtImporters(PrototypeArt, false);
        CreatePrefabs(PrototypeArt);

        string[] sceneNames = { "MainMenu", "LevelSelect", "Level1", "Level2", "Level3", "Prototype" };
        foreach (string sceneName in sceneNames)
        {
            CreateBootstrapScene(sceneName);
        }

        ConfigurePlayerSettings();
        EditorBuildSettings.scenes = System.Array.ConvertAll(
            sceneNames,
            sceneName => new EditorBuildSettingsScene($"Assets/Scenes/{sceneName}.unity", true));
        AssetDatabase.SaveAssets();
        Debug.Log("ENERGOKEY_FULL_BUILD_OK: menu, level select and three gameplay scenes created");
    }

    public static void BuildFullGameBatch()
    {
        BuildFullGame();
    }

    [MenuItem("EnergoKey/Prepare Final Visual Style")]
    public static void PrepareFinalStyle()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureArtImporters(FinalArt, true);
        CreatePrefabs(FinalArt);
        AssetDatabase.SaveAssets();
        Debug.Log("ENERGOKEY_FINAL_STYLE_OK: original final sprites imported and prefabs refreshed");
    }

    public static void PrepareFinalStyleBatch()
    {
        PrepareFinalStyle();
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(PrototypeArt);
        Directory.CreateDirectory(Prefabs);
        Directory.CreateDirectory("Assets/Scenes");
    }

    private static void CreatePrototypeArt()
    {
        WriteSolid("background", new Color32(25, 31, 39, 255));
        WriteSolid("player", new Color32(60, 205, 255, 255));
        WriteSolid("crystal", new Color32(255, 209, 40, 255));
        WriteSolid("hazard", new Color32(255, 61, 72, 255));
        WriteSolid("exit_closed", new Color32(132, 145, 166, 255));
        WriteSolid("exit_open", new Color32(61, 224, 122, 255));
        WriteSolid("ui_button", new Color32(54, 92, 120, 255));
        WriteSolid("ui_panel", new Color32(45, 52, 65, 255));
        WriteSolid("life", new Color32(60, 205, 255, 255));
        WriteSolid("lock", new Color32(205, 214, 224, 255));
        WriteSolid("pause", new Color32(255, 255, 255, 255));
        WriteSolid("arrow", new Color32(255, 255, 255, 255));
    }

    private static void WriteSolid(string name, Color32 color)
    {
        string path = $"{PrototypeArt}/{name}.png";
        if (File.Exists(path)) return;
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    private static void ConfigureArtImporters()
    {
        ConfigureArtImporters(PrototypeArt, false);
    }

    private static void ConfigureArtImporters(string artFolder, bool highResolution)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { artFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                bool isBackground = Path.GetFileNameWithoutExtension(path) == "background";
                importer.spritePixelsPerUnit = highResolution ? (isBackground ? 80f : 256f) : 32f;
                importer.filterMode = highResolution ? FilterMode.Bilinear : FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }
    }

    private static Sprite Sprite(string artFolder, string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{artFolder}/{name}.png");
    }

    private static void CreatePrefabs()
    {
        CreatePrefabs(PrototypeArt);
    }

    private static void CreatePrefabs(string artFolder)
    {
        GameObject player = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(PlayerController));
        player.tag = "Player";
        player.GetComponent<SpriteRenderer>().sprite = Sprite(artFolder, "player");
        player.GetComponent<Rigidbody2D>().gravityScale = 0;
        player.GetComponent<Rigidbody2D>().freezeRotation = true;
        player.GetComponent<CircleCollider2D>().radius = 0.42f;
        PrefabUtility.SaveAsPrefabAsset(player, $"{Prefabs}/Player.prefab");
        Object.DestroyImmediate(player);

        GameObject collectible = new GameObject("Collectible", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Collectible));
        collectible.GetComponent<SpriteRenderer>().sprite = Sprite(artFolder, "crystal");
        collectible.GetComponent<CircleCollider2D>().isTrigger = true;
        collectible.GetComponent<CircleCollider2D>().radius = 0.4f;
        PrefabUtility.SaveAsPrefabAsset(collectible, $"{Prefabs}/Collectible.prefab");
        Object.DestroyImmediate(collectible);

        GameObject hazard = new GameObject("Hazard", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Hazard));
        hazard.GetComponent<SpriteRenderer>().sprite = Sprite(artFolder, "hazard");
        hazard.GetComponent<BoxCollider2D>().isTrigger = true;
        hazard.GetComponent<BoxCollider2D>().size = new Vector2(0.82f, 0.82f);
        PrefabUtility.SaveAsPrefabAsset(hazard, $"{Prefabs}/Hazard.prefab");
        Object.DestroyImmediate(hazard);

        GameObject button = new GameObject("UIButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 68);
        button.GetComponent<Image>().sprite = Sprite(artFolder, "ui_button");
        GameObject label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        label.transform.SetParent(button.transform, false);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "КНОПКА";
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        PrefabUtility.SaveAsPrefabAsset(button, $"{Prefabs}/UIButton.prefab");
        Object.DestroyImmediate(button);
    }

    private static void CreateBootstrapScene(string sceneName)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("SceneBootstrap", typeof(SceneBootstrap));
        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{sceneName}.unity");
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.productName = "Энергоключ";
        PlayerSettings.companyName = "Учебный проект";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.runInBackground = false;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
    }
}
#endif
