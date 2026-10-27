using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIFactory
{
    private static Font cachedFont;

    public static Canvas CreateCanvas(string name = "Canvas")
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        return canvas;
    }

    public static Image CreateImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.sprite = ThemeAssets.Sprite("ui_panel");
        return image;
    }

    public static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        Text text = go.GetComponent<Text>();
        text.font = Font();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = content;
        text.resizeTextForBestFit = false;
        return text;
    }

    public static Button CreateButton(Transform parent, string name, string label, UnityAction action, Vector2 anchor, Vector2 size, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = go.GetComponent<Image>();
        image.sprite = ThemeAssets.Sprite("ui_button");
        image.color = new Color(0.18f, 0.36f, 0.48f, 0.96f);
        Button button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f);
        colors.pressedColor = new Color(0.75f, 0.86f, 0.92f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.65f);
        button.colors = colors;
        if (action != null) button.onClick.AddListener(action);
        button.onClick.AddListener(() => AudioService.Ensure().Play("button"));
        Text text = CreateText(go.transform, "Label", label, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    public static GameObject CreateOverlayPanel(Transform parent, string name, string title, string body)
    {
        Image blocker = CreateImage(parent, name, new Color(0.015f, 0.02f, 0.035f, 0.88f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CreateImage(blocker.transform, "Panel", ThemeAssets.Panel, new Vector2(0.25f, 0.22f), new Vector2(0.75f, 0.78f), Vector2.zero, Vector2.zero);
        Text heading = CreateText(blocker.transform, "Title", title, 46, TextAnchor.MiddleCenter, new Vector2(0.25f, 0.60f), new Vector2(0.75f, 0.75f), Vector2.zero, Vector2.zero);
        heading.fontStyle = FontStyle.Bold;
        heading.color = ThemeAssets.Cyan;
        Text bodyText = CreateText(blocker.transform, "Body", body, 24, TextAnchor.MiddleCenter, new Vector2(0.28f, 0.42f), new Vector2(0.72f, 0.61f), Vector2.zero, Vector2.zero);
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        return blocker.gameObject;
    }

    public static Font Font()
    {
        if (cachedFont != null) return cachedFont;
        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return cachedFont;
    }
}

