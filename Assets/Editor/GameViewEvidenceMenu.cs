using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only helpers used to capture the three acceptance resolutions in the
/// real Game View. The commands add (or reuse) fixed-resolution entries and
/// select them; they are not included in a player build.
/// </summary>
public static class GameViewEvidenceMenu
{
    [MenuItem("EnergoKey/Evidence/Game View/1280 x 720 (16-9)")]
    public static void Set1280x720() => SelectFixedResolution(1280, 720, "Evidence 1280x720 (16:9)");

    [MenuItem("EnergoKey/Evidence/Game View/1440 x 720 (18-9)")]
    public static void Set1440x720() => SelectFixedResolution(1440, 720, "Evidence 1440x720 (18:9)");

    [MenuItem("EnergoKey/Evidence/Game View/1024 x 768 (4-3)")]
    public static void Set1024x768() => SelectFixedResolution(1024, 768, "Evidence 1024x768 (4:3)");

    private static void SelectFixedResolution(int width, int height, string label)
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type gameViewType = editorAssembly.GetType("UnityEditor.GameView");
        Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
        Type groupType = editorAssembly.GetType("UnityEditor.GameViewSizeGroupType");
        Type sizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
        Type sizeKindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");

        if (gameViewType == null || sizesType == null || groupType == null || sizeType == null || sizeKindType == null)
        {
            Debug.LogError("ENERGOKEY_EVIDENCE: Unity Game View reflection API is unavailable");
            return;
        }

        Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        object standalone = Enum.Parse(groupType, "Standalone");
        object group = sizesType.GetMethod("GetGroup")?.Invoke(sizes, new[] { standalone });

        if (group == null)
        {
            Debug.LogError("ENERGOKEY_EVIDENCE: Standalone Game View size group was not found");
            return;
        }

        Type concreteGroupType = group.GetType();
        int builtinCount = (int)concreteGroupType.GetMethod("GetBuiltinCount")!.Invoke(group, null);
        int customCount = (int)concreteGroupType.GetMethod("GetCustomCount")!.Invoke(group, null);
        MethodInfo getSize = concreteGroupType.GetMethod("GetGameViewSize");
        PropertyInfo widthProperty = sizeType.GetProperty("width");
        PropertyInfo heightProperty = sizeType.GetProperty("height");
        int selectedIndex = -1;

        for (int index = 0; index < builtinCount + customCount; index++)
        {
            object candidate = getSize!.Invoke(group, new object[] { index });
            int candidateWidth = (int)widthProperty!.GetValue(candidate);
            int candidateHeight = (int)heightProperty!.GetValue(candidate);
            if (candidateWidth == width && candidateHeight == height)
            {
                selectedIndex = index;
                break;
            }
        }

        if (selectedIndex < 0)
        {
            object fixedResolution = Enum.Parse(sizeKindType, "FixedResolution");
            ConstructorInfo constructor = sizeType.GetConstructor(new[] { sizeKindType, typeof(int), typeof(int), typeof(string) });
            object newSize = constructor!.Invoke(new[] { fixedResolution, (object)width, height, label });
            concreteGroupType.GetMethod("AddCustomSize")!.Invoke(group, new[] { newSize });
            selectedIndex = builtinCount + customCount;
        }

        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        PropertyInfo selectedProperty = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        selectedProperty!.SetValue(gameView, selectedIndex);
        MethodInfo snapZoom = gameViewType.GetMethod("SnapZoom", BindingFlags.Instance | BindingFlags.NonPublic);
        snapZoom?.Invoke(gameView, new object[] { 1f });
        gameView.Repaint();
        Debug.Log($"ENERGOKEY_EVIDENCE: Game View set to {width}x{height}");
    }
}
