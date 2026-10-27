using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaAdapter : MonoBehaviour
{
    private static readonly Rect DefaultSimulatedSafeArea = new Rect(0.06f, 0.06f, 0.88f, 0.88f);

    [SerializeField] private RectTransform safeAreaRoot;
    [SerializeField] private bool simulateInEditor;
    [SerializeField] private Rect simulatedSafeAreaNormalized = new Rect(0.06f, 0.06f, 0.88f, 0.88f);
    [SerializeField] private bool allowF8Toggle = true;
    [SerializeField] private bool showDebugBorder;
    [SerializeField] private Color debugBorderColor = new Color(0f, 1f, 0.8f, 0.9f);
    [SerializeField, Min(1f)] private float debugBorderThickness = 2f;

    private Rect lastAppliedSafeArea;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;
    private bool hasAppliedArea;

    public RectTransform SafeAreaRoot => safeAreaRoot;
    public Rect AppliedSafeArea { get; private set; }

    public bool IsSimulating
    {
        get
        {
#if UNITY_EDITOR
            return simulateInEditor;
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        EnsureTarget();
        ApplySafeArea(true);
    }

    private void OnEnable()
    {
        EnsureTarget();
        ApplySafeArea(true);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (allowF8Toggle && Input.GetKeyDown(KeyCode.F8))
        {
            simulateInEditor = !simulateInEditor;
            ApplySafeArea(true);
        }
#endif

        ApplySafeArea(false);
    }

    public void Configure(RectTransform target, bool enableEditorSimulation = false)
    {
        Configure(target, enableEditorSimulation, DefaultSimulatedSafeArea, false, true);
    }

    public void Configure(
        RectTransform target,
        bool enableEditorSimulation,
        Rect normalizedSimulation,
        bool drawDebugBorder,
        bool enableF8Toggle = true)
    {
        safeAreaRoot = target != null ? target : GetComponent<RectTransform>();
        simulateInEditor = enableEditorSimulation;
        simulatedSafeAreaNormalized = SanitizeNormalizedRect(normalizedSimulation);
        showDebugBorder = drawDebugBorder;
        allowF8Toggle = enableF8Toggle;
        ApplySafeArea(true);
    }

    public void SetEditorSimulation(bool enabled)
    {
        simulateInEditor = enabled;
        ApplySafeArea(true);
    }

    public void SetDebugBorderVisible(bool visible)
    {
        showDebugBorder = visible;
    }

    public void Refresh()
    {
        ApplySafeArea(true);
    }

    private void EnsureTarget()
    {
        if (safeAreaRoot == null)
        {
            safeAreaRoot = GetComponent<RectTransform>();
        }
    }

    private void ApplySafeArea(bool force)
    {
        if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Rect area = ResolveSafeArea();
        if (!force &&
            hasAppliedArea &&
            lastScreenWidth == Screen.width &&
            lastScreenHeight == Screen.height &&
            Approximately(area, lastAppliedSafeArea))
        {
            return;
        }

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 anchorMin = new Vector2(area.xMin / screenSize.x, area.yMin / screenSize.y);
        Vector2 anchorMax = new Vector2(area.xMax / screenSize.x, area.yMax / screenSize.y);

        safeAreaRoot.anchorMin = anchorMin;
        safeAreaRoot.anchorMax = anchorMax;
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;

        AppliedSafeArea = area;
        lastAppliedSafeArea = area;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        hasAppliedArea = true;
    }

    private Rect ResolveSafeArea()
    {
#if UNITY_EDITOR
        if (simulateInEditor)
        {
            Rect normalized = SanitizeNormalizedRect(simulatedSafeAreaNormalized);
            return new Rect(
                normalized.x * Screen.width,
                normalized.y * Screen.height,
                normalized.width * Screen.width,
                normalized.height * Screen.height);
        }
#endif

        Rect area = Screen.safeArea;
        if (area.width <= 0f || area.height <= 0f)
        {
            return new Rect(0f, 0f, Screen.width, Screen.height);
        }

        float xMin = Mathf.Clamp(area.xMin, 0f, Screen.width);
        float yMin = Mathf.Clamp(area.yMin, 0f, Screen.height);
        float xMax = Mathf.Clamp(area.xMax, xMin, Screen.width);
        float yMax = Mathf.Clamp(area.yMax, yMin, Screen.height);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Rect SanitizeNormalizedRect(Rect value)
    {
        float xMin = Mathf.Clamp01(Mathf.Min(value.xMin, value.xMax));
        float yMin = Mathf.Clamp01(Mathf.Min(value.yMin, value.yMax));
        float xMax = Mathf.Clamp01(Mathf.Max(value.xMin, value.xMax));
        float yMax = Mathf.Clamp01(Mathf.Max(value.yMin, value.yMax));

        if (xMax - xMin < 0.01f || yMax - yMin < 0.01f)
        {
            return DefaultSimulatedSafeArea;
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static bool Approximately(Rect left, Rect right)
    {
        const float tolerance = 0.01f;
        return Mathf.Abs(left.x - right.x) < tolerance &&
               Mathf.Abs(left.y - right.y) < tolerance &&
               Mathf.Abs(left.width - right.width) < tolerance &&
               Mathf.Abs(left.height - right.height) < tolerance;
    }

    private void OnGUI()
    {
        if ((!showDebugBorder && !IsSimulating) || !hasAppliedArea || Event.current.type != EventType.Repaint)
        {
            return;
        }

        float top = Screen.height - AppliedSafeArea.yMax;
        Rect guiRect = new Rect(AppliedSafeArea.xMin, top, AppliedSafeArea.width, AppliedSafeArea.height);
        float thickness = Mathf.Max(1f, debugBorderThickness);
        Color previousColor = GUI.color;
        GUI.color = debugBorderColor;
        GUI.DrawTexture(new Rect(guiRect.xMin, guiRect.yMin, guiRect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(guiRect.xMin, guiRect.yMax - thickness, guiRect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(guiRect.xMin, guiRect.yMin, thickness, guiRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(guiRect.xMax - thickness, guiRect.yMin, thickness, guiRect.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }
}
