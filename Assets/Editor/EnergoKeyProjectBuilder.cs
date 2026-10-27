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
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PrototypeArt }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }
    }

    private static Sprite Sprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{PrototypeArt}/{name}.png");
    }

    private static void CreatePrefabs()
    {
        GameObject player = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(PlayerController));
        player.tag = "Player";
        player.GetComponent<SpriteRenderer>().sprite = Sprite("player");
        player.GetComponent<Rigidbody2D>().gravityScale = 0;
        player.GetComponent<Rigidbody2D>().freezeRotation = true;
        PrefabUtility.SaveAsPrefabAsset(player, $"{Prefabs}/Player.prefab");
        Object.DestroyImmediate(player);

        GameObject collectible = new GameObject("Collectible", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Collectible));
        collectible.GetComponent<SpriteRenderer>().sprite = Sprite("crystal");
        collectible.GetComponent<CircleCollider2D>().isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(collectible, $"{Prefabs}/Collectible.prefab");
        Object.DestroyImmediate(collectible);

        GameObject hazard = new GameObject("Hazard", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Hazard));
        hazard.GetComponent<SpriteRenderer>().sprite = Sprite("hazard");
        hazard.GetComponent<BoxCollider2D>().isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(hazard, $"{Prefabs}/Hazard.prefab");
        Object.DestroyImmediate(hazard);

        GameObject button = new GameObject("UIButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 68);
        button.GetComponent<Image>().sprite = Sprite("ui_button");
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
