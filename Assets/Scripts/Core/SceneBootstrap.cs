using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SceneBootstrap : MonoBehaviour
{
    private enum RuntimeView
    {
        MainMenu,
        LevelSelect,
        Gameplay
    }

    private Canvas appCanvas;
    private Image canvasBackdrop;
    private RectTransform safeAreaRoot;
    private Camera mainCamera;
    private GameObject currentScreenRoot;
    private GameObject currentWorldRoot;
    private GameController activeGame;
    private RuntimeView currentView;
    private int currentLevel;

    private void Awake()
    {
        AudioService.Ensure();
        Time.timeScale = 1f;
        BuildApplicationShell();

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "LevelSelect")
        {
            ShowLevelSelectInternal();
        }
        else if (sceneName == "Level1")
        {
            BuildLevelInternal(1);
        }
        else if (sceneName == "Level2")
        {
            BuildLevelInternal(2);
        }
        else if (sceneName == "Level3")
        {
            BuildLevelInternal(3);
        }
        else if (sceneName == "Prototype")
        {
            BuildLevelInternal(1);
        }
        else
        {
            ShowMainMenuInternal();
        }
    }

    private void Update()
    {
        UpdateCameraForAspect();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SaveService.UnlockAllLevelsForEvidence();
            Debug.Log("ENERGOKEY_EVIDENCE: F2 — все уровни открыты");
            if (currentView == RuntimeView.LevelSelect)
            {
                ShowLevelSelectInternal();
            }
        }

#endif
    }

    public void ShowMainMenu()
    {
        NavigateToMainMenu();
    }

    public void ShowLevelSelect()
    {
        NavigateToLevelSelect();
    }

    public void LoadLevel(int level)
    {
        NavigateToLevel(level);
    }

    private void BuildApplicationShell()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.GetComponent<Camera>();
        }

        mainCamera.orthographic = true;
        mainCamera.backgroundColor = ThemeAssets.Background;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        UpdateCameraForAspect();

        appCanvas = UIFactory.CreateCanvas("AppCanvas");
        canvasBackdrop = UIFactory.CreateImage(appCanvas.transform, "ScreenBackdrop", ThemeAssets.Background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        canvasBackdrop.sprite = ThemeAssets.Sprite("background");
        canvasBackdrop.raycastTarget = false;

        GameObject safeObject = new GameObject("SafeArea", typeof(RectTransform));
        safeObject.transform.SetParent(appCanvas.transform, false);
        safeAreaRoot = safeObject.GetComponent<RectTransform>();
        safeAreaRoot.anchorMin = Vector2.zero;
        safeAreaRoot.anchorMax = Vector2.one;
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
        SafeAreaAdapter safeArea = safeObject.AddComponent<SafeAreaAdapter>();
        safeArea.Configure(safeAreaRoot, false, new Rect(0.06f, 0.06f, 0.88f, 0.88f), false, true);
    }

    private void UpdateCameraForAspect()
    {
        if (mainCamera == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        float aspect = (float)Screen.width / Screen.height;
        mainCamera.orthographicSize = Mathf.Max(4.5f, 8f / Mathf.Max(0.1f, aspect));
    }

    private void NavigateToMainMenu()
    {
        Time.timeScale = 1f;
        if (!TryLoadNamedScene("MainMenu")) ShowMainMenuInternal();
    }

    private void NavigateToLevelSelect()
    {
        Time.timeScale = 1f;
        if (!TryLoadNamedScene("LevelSelect")) ShowLevelSelectInternal();
    }

    private void NavigateToLevel(int level)
    {
        int safeLevel = Mathf.Clamp(level, 1, GameRules.MaxLevel);
        if (safeLevel > SaveService.UnlockedLevel)
        {
            ShowLevelSelectInternal();
            return;
        }

        Time.timeScale = 1f;
        if (!TryLoadNamedScene($"Level{safeLevel}")) BuildLevelInternal(safeLevel);
    }

    private bool TryLoadNamedScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            return false;
        }

        SceneNavigator.Load(sceneName);
        return true;
    }

    private void ShowMainMenuInternal()
    {
        Transform root = PrepareScreen("MainMenuScreen", RuntimeView.MainMenu, true);
        Text title = UIFactory.CreateText(root, "GameTitle", "ЭНЕРГОКЛЮЧ", 64, TextAnchor.MiddleCenter, new Vector2(0.15f, 0.78f), new Vector2(0.85f, 0.94f), Vector2.zero, Vector2.zero);
        title.fontStyle = FontStyle.Bold;
        title.color = ThemeAssets.Cyan;

        Text subtitle = UIFactory.CreateText(root, "Subtitle", "2D TOP-DOWN • СОБЕРИ КРИСТАЛЛЫ И ОТКРОЙ ВЫХОД", 21, TextAnchor.MiddleCenter, new Vector2(0.12f, 0.70f), new Vector2(0.88f, 0.79f), Vector2.zero, Vector2.zero);
        subtitle.color = new Color(0.76f, 0.84f, 0.91f);

        Image menuPanel = CreatePanelAt(root, "MenuPanel", new Vector2(480f, 400f), new Vector2(0f, -35f));
        CreateFittedButton(menuPanel.transform, "PlayButton", "ИГРАТЬ", NavigateToLevelSelect, new Vector2(0f, 115f), new Vector2(320f, 62f));
        Button tutorialButton = CreateFittedButton(menuPanel.transform, "TutorialButton", "КАК ИГРАТЬ", null, new Vector2(0f, 38f), new Vector2(320f, 62f));
        Button settingsButton = CreateFittedButton(menuPanel.transform, "SettingsButton", "НАСТРОЙКИ", null, new Vector2(0f, -39f), new Vector2(320f, 62f));
        Button aboutButton = CreateFittedButton(menuPanel.transform, "AboutButton", "ОБ ИГРЕ", null, new Vector2(0f, -116f), new Vector2(320f, 62f));

        Text progress = UIFactory.CreateText(root, "ProgressSummary", $"Открыто уровней: {SaveService.UnlockedLevel}/{GameRules.MaxLevel}    Лучший счёт: {SaveService.BestScore}", 22, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.13f), Vector2.zero, Vector2.zero);
        progress.color = ThemeAssets.Yellow;

        CreateSettingsOverlay(root, settingsButton);
        CreateTutorialOverlay(root, tutorialButton);
        CreateAboutOverlay(root, aboutButton);
    }

    private void ShowLevelSelectInternal()
    {
        Transform root = PrepareScreen("LevelSelectScreen", RuntimeView.LevelSelect, true);
        Text title = UIFactory.CreateText(root, "Title", "ВЫБОР УРОВНЯ", 52, TextAnchor.MiddleCenter, new Vector2(0.2f, 0.79f), new Vector2(0.8f, 0.94f), Vector2.zero, Vector2.zero);
        title.fontStyle = FontStyle.Bold;
        title.color = ThemeAssets.Cyan;

        for (int level = 1; level <= GameRules.MaxLevel; level++)
        {
            CreateLevelCard(root, level, (level - 2) * 290f);
        }

        CreateFittedButton(root, "BackButton", "НАЗАД", NavigateToMainMenu, new Vector2(0f, -275f), new Vector2(240f, 58f));
        Text hint = UIFactory.CreateText(root, "UnlockHint", "Следующий уровень открывается только после победы", 20, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.20f), Vector2.zero, Vector2.zero);
        hint.color = new Color(0.75f, 0.82f, 0.9f);
    }

    private void CreateLevelCard(Transform parent, int level, float x)
    {
        bool unlocked = level <= SaveService.UnlockedLevel;
        Image card = CreatePanelAt(parent, $"Level{level}Card", new Vector2(250f, 310f), new Vector2(x, 5f));
        Text heading = UIFactory.CreateText(card.transform, "Heading", $"УРОВЕНЬ {level}", 31, TextAnchor.MiddleCenter, new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.92f), Vector2.zero, Vector2.zero);
        heading.fontStyle = FontStyle.Bold;
        heading.color = unlocked ? ThemeAssets.Cyan : new Color(0.58f, 0.62f, 0.68f);

        Text details = UIFactory.CreateText(card.transform, "Details", $"Кристаллы: {GameRules.CrystalCountForLevel(level)}\nОпасности: {GameRules.HazardCountForLevel(level)}\nСложность: {DifficultyName(level)}", 20, TextAnchor.MiddleCenter, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.68f), Vector2.zero, Vector2.zero);
        details.horizontalOverflow = HorizontalWrapMode.Wrap;
        details.verticalOverflow = VerticalWrapMode.Overflow;

        int capturedLevel = level;
        Button button = CreateFittedButton(card.transform, "StartButton", unlocked ? "НАЧАТЬ" : "ЗАБЛОКИРОВАНО", unlocked ? (UnityAction)(() => NavigateToLevel(capturedLevel)) : null, new Vector2(0f, -105f), new Vector2(205f, 58f));
        button.interactable = unlocked;
        if (!unlocked)
        {
            Text lockText = UIFactory.CreateText(card.transform, "Lock", "ЗАМОК", 16, TextAnchor.MiddleCenter, new Vector2(0.25f, 0.22f), new Vector2(0.75f, 0.32f), Vector2.zero, Vector2.zero);
            lockText.color = new Color(0.68f, 0.72f, 0.78f);
        }
    }

    private void BuildLevelInternal(int level)
    {
        if (level > SaveService.UnlockedLevel)
        {
            ShowLevelSelectInternal();
            return;
        }

        Transform uiRoot = PrepareScreen($"Level{level}Screen", RuntimeView.Gameplay, false);
        currentLevel = level;
        currentWorldRoot = new GameObject($"Level{level}_World");
        Transform world = currentWorldRoot.transform;
        CreateWorldBackground(world);
        CreateBoundaryWalls(world);

        PlayerController player;
        ExitGate gate;
        if (level == 1) BuildLevelOne(world, out player, out gate);
        else if (level == 2) BuildLevelTwo(world, out player, out gate);
        else BuildLevelThree(world, out player, out gate);

        Text feedback = UIFactory.CreateText(uiRoot, "Feedback", string.Empty, 25, TextAnchor.MiddleCenter, new Vector2(0.18f, 0.73f), new Vector2(0.82f, 0.82f), Vector2.zero, Vector2.zero);
        feedback.fontStyle = FontStyle.Bold;
        feedback.gameObject.SetActive(false);

        GameObject victoryPanel = CreateResultPanel(uiRoot, "VictoryPanel", "ПОБЕДА", ThemeAssets.Green, out Text victorySummary);
        GameObject defeatPanel = CreateResultPanel(uiRoot, "DefeatPanel", "ПОРАЖЕНИЕ", ThemeAssets.Red, out Text defeatSummary);

        GameObject managerObject = new GameObject("GameController", typeof(GameController));
        managerObject.transform.SetParent(world, false);
        activeGame = managerObject.GetComponent<GameController>();
        activeGame.Configure(level, player, gate, feedback, victoryPanel, defeatPanel, victorySummary, defeatSummary);

        CreateHud(uiRoot, activeGame, level);
        CreateControls(uiRoot, player);
        ConfigureResultButtons(victoryPanel.transform, defeatPanel.transform, level);
        CreatePauseUi(uiRoot, activeGame);
    }

    private void CreateSettingsOverlay(Transform parent, Button openButton)
    {
        GameObject panel = UIFactory.CreateOverlayPanel(parent, "SettingsPanel", "НАСТРОЙКИ", "Громкость звуков и музыки");
        CanvasGroup group = panel.AddComponent<CanvasGroup>();
        SetCanvasGroupVisible(group, false);
        Slider slider = CreateSlider(panel.transform, "MasterVolume", new Vector2(0f, -10f), new Vector2(390f, 34f));
        Text value = UIFactory.CreateText(panel.transform, "VolumeValue", "80%", 24, TextAnchor.MiddleCenter, new Vector2(0.42f, 0.34f), new Vector2(0.58f, 0.42f), Vector2.zero, Vector2.zero);
        Button close = CreateFittedButton(panel.transform, "CloseButton", "ГОТОВО", null, new Vector2(0f, -145f), new Vector2(230f, 58f));
        SettingsPanelController controller = parent.gameObject.AddComponent<SettingsPanelController>();
        controller.Configure(slider, group, openButton, close, value, true);
    }

    private void CreateTutorialOverlay(Transform parent, Button openButton)
    {
        GameObject panel = UIFactory.CreateOverlayPanel(parent, "TutorialPanel", "Управление", string.Empty);
        CanvasGroup group = panel.AddComponent<CanvasGroup>();
        SetCanvasGroupVisible(group, false);
        Text title = FindDirectText(panel.transform, "Title");
        Text body = FindDirectText(panel.transform, "Body");
        if (body != null)
        {
            body.fontSize = 22;
            body.GetComponent<RectTransform>().anchorMin = new Vector2(0.27f, 0.40f);
            body.GetComponent<RectTransform>().anchorMax = new Vector2(0.73f, 0.62f);
        }

        Text progress = UIFactory.CreateText(panel.transform, "Progress", "1 / 3", 20, TextAnchor.MiddleCenter, new Vector2(0.42f, 0.33f), new Vector2(0.58f, 0.39f), Vector2.zero, Vector2.zero);
        Button back = CreateFittedButton(panel.transform, "BackButton", "НАЗАД", null, new Vector2(-185f, -145f), new Vector2(190f, 56f));
        Button next = CreateFittedButton(panel.transform, "NextButton", "ДАЛЕЕ", null, new Vector2(185f, -145f), new Vector2(190f, 56f));
        Button skip = CreateFittedButton(panel.transform, "SkipButton", "ПРОПУСТИТЬ", null, new Vector2(0f, -215f), new Vector2(230f, 50f));
        TutorialController controller = parent.gameObject.AddComponent<TutorialController>();
        controller.Configure(group, title, body, progress, next, back, skip, true);
        openButton.onClick.AddListener(controller.OpenFromMenu);
    }

    private void CreateAboutOverlay(Transform parent, Button openButton)
    {
        GameObject panel = UIFactory.CreateOverlayPanel(parent, "AboutPanel", "ОБ ИГРЕ", "Энергоключ\nVersion 1.0.0\nАвтор: студент\nГрафика и звук созданы для учебного проекта");
        CanvasGroup group = panel.AddComponent<CanvasGroup>();
        SetCanvasGroupVisible(group, false);
        Text body = FindDirectText(panel.transform, "Body");
        if (body != null) body.fontSize = 23;
        Button close = CreateFittedButton(panel.transform, "CloseButton", "НАЗАД", null, new Vector2(0f, -160f), new Vector2(230f, 58f));
        openButton.onClick.AddListener(() => SetCanvasGroupVisible(group, true));
        close.onClick.AddListener(() => SetCanvasGroupVisible(group, false));
    }

    private void CreatePauseUi(Transform parent, GameController controller)
    {
        Button pauseButton = CreateFittedButton(parent, "PauseButton", "II", null, new Vector2(-48f, -43f), new Vector2(66f, 58f), new Vector2(1f, 1f));
        GameObject pausePanel = UIFactory.CreateOverlayPanel(parent, "PausePanel", "ПАУЗА", string.Empty);
        CanvasGroup pauseGroup = pausePanel.AddComponent<CanvasGroup>();
        SetCanvasGroupVisible(pauseGroup, false);
        Text timer = UIFactory.CreateText(pausePanel.transform, "PausedTimer", "ТАЙМЕР ОСТАНОВЛЕН  00:00", 24, TextAnchor.MiddleCenter, new Vector2(0.28f, 0.50f), new Vector2(0.72f, 0.58f), Vector2.zero, Vector2.zero);
        timer.color = ThemeAssets.Yellow;
        Button resume = CreateFittedButton(pausePanel.transform, "ResumeButton", "ПРОДОЛЖИТЬ", null, new Vector2(0f, -65f), new Vector2(270f, 56f));
        CreateFittedButton(pausePanel.transform, "LevelSelectButton", "ВЫБОР УРОВНЯ", NavigateToLevelSelect, new Vector2(0f, -130f), new Vector2(270f, 54f));
        CreateFittedButton(pausePanel.transform, "MenuButton", "ГЛАВНОЕ МЕНЮ", NavigateToMainMenu, new Vector2(0f, -193f), new Vector2(270f, 54f));
        PauseController pauseController = parent.gameObject.AddComponent<PauseController>();
        pauseController.Configure(pauseGroup, pauseButton, resume, 0.22f, true, controller, timer);
    }

    private GameObject CreateResultPanel(Transform parent, string name, string title, Color titleColor, out Text summary)
    {
        GameObject panel = UIFactory.CreateOverlayPanel(parent, name, title, string.Empty);
        CanvasGroup group = panel.AddComponent<CanvasGroup>();
        SetCanvasGroupVisible(group, false);
        Text heading = FindDirectText(panel.transform, "Title");
        if (heading != null) heading.color = titleColor;
        summary = FindDirectText(panel.transform, "Body");
        if (summary != null)
        {
            summary.fontSize = 21;
            summary.GetComponent<RectTransform>().anchorMin = new Vector2(0.22f, 0.43f);
            summary.GetComponent<RectTransform>().anchorMax = new Vector2(0.78f, 0.61f);
        }
        return panel;
    }

    private void ConfigureResultButtons(Transform victory, Transform defeat, int level)
    {
        CreateFittedButton(victory, "RestartButton", "ПОВТОРИТЬ", RestartCurrentLevel, new Vector2(0f, -72f), new Vector2(265f, 54f));
        string nextLabel = level >= GameRules.MaxLevel ? "В ГЛАВНОЕ МЕНЮ" : "СЛЕДУЮЩИЙ УРОВЕНЬ";
        UnityAction nextAction = level >= GameRules.MaxLevel ? (UnityAction)NavigateToMainMenu : () => NavigateToLevel(level + 1);
        CreateFittedButton(victory, "NextButton", nextLabel, nextAction, new Vector2(0f, -137f), new Vector2(300f, 54f));
        CreateFittedButton(victory, "LevelSelectButton", "ВЫБОР УРОВНЯ", NavigateToLevelSelect, new Vector2(0f, -200f), new Vector2(265f, 50f));
        CreateFittedButton(defeat, "RestartButton", "ПОВТОРИТЬ", RestartCurrentLevel, new Vector2(0f, -72f), new Vector2(265f, 54f));
        CreateFittedButton(defeat, "LevelSelectButton", "ВЫБОР УРОВНЯ", NavigateToLevelSelect, new Vector2(0f, -137f), new Vector2(265f, 54f));
        CreateFittedButton(defeat, "MenuButton", "ГЛАВНОЕ МЕНЮ", NavigateToMainMenu, new Vector2(0f, -200f), new Vector2(265f, 50f));
    }

    private void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        if (SceneManager.GetActiveScene().name == "Prototype")
        {
            SceneNavigator.RestartCurrent();
            return;
        }

        string sceneName = $"Level{currentLevel}";
        if (Application.CanStreamedLevelBeLoaded(sceneName)) SceneNavigator.Load(sceneName);
        else BuildLevelInternal(currentLevel);
    }

    private Transform PrepareScreen(string name, RuntimeView view, bool showBackdrop)
    {
        Time.timeScale = 1f;
        if (currentScreenRoot != null)
        {
            currentScreenRoot.SetActive(false);
            Destroy(currentScreenRoot);
        }
        if (currentWorldRoot != null)
        {
            currentWorldRoot.SetActive(false);
            Destroy(currentWorldRoot);
        }

        activeGame = null;
        currentView = view;
        canvasBackdrop.enabled = showBackdrop;
        currentScreenRoot = new GameObject(name, typeof(RectTransform));
        currentScreenRoot.transform.SetParent(safeAreaRoot, false);
        RectTransform rect = currentScreenRoot.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return currentScreenRoot.transform;
    }

    private static string DifficultyName(int level)
    {
        if (level == 1) return "простая";
        if (level == 2) return "средняя";
        return "высокая";
    }

    private static Image CreatePanelAt(Transform parent, string name, Vector2 size, Vector2 position)
    {
        Image image = UIFactory.CreateImage(parent, name, ThemeAssets.Panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        image.rectTransform.sizeDelta = size;
        image.rectTransform.anchoredPosition = position;
        return image;
    }

    private static Button CreateFittedButton(Transform parent, string name, string label, UnityAction action, Vector2 position, Vector2 size, Vector2? anchor = null)
    {
        Button button = UIFactory.CreateButton(parent, name, label, action, anchor ?? new Vector2(0.5f, 0.5f), size, position);
        Text text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = 24;
        }
        return button;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image background = sliderObject.GetComponent<Image>();
        background.sprite = ThemeAssets.Sprite("ui_panel");
        background.color = new Color(0.2f, 0.24f, 0.31f, 1f);

        Image fill = UIFactory.CreateImage(sliderObject.transform, "Fill", ThemeAssets.Cyan, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(8f, 0f), new Vector2(-8f, 0f));
        Image handle = UIFactory.CreateImage(sliderObject.transform, "Handle", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        handle.rectTransform.sizeDelta = new Vector2(26f, 46f);
        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static Text FindDirectText(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<Text>() : null;
    }

    private static void SetCanvasGroupVisible(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.gameObject.SetActive(true);
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private static void CreateHud(Transform parent, GameController controller, int level)
    {
        Image bar = UIFactory.CreateImage(parent, "HUD", new Color(0.04f, 0.06f, 0.10f, 0.94f), new Vector2(0f, 0.87f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        Text levelText = UIFactory.CreateText(bar.transform, "Level", $"УРОВЕНЬ {level}", 20, TextAnchor.MiddleLeft, new Vector2(0.02f, 0f), new Vector2(0.15f, 1f), Vector2.zero, Vector2.zero);
        levelText.color = ThemeAssets.Cyan;
        Text crystals = UIFactory.CreateText(bar.transform, "Crystals", $"КРИСТАЛЛЫ 0/{GameRules.CrystalCountForLevel(level)}", 20, TextAnchor.MiddleLeft, new Vector2(0.15f, 0f), new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero);
        Text lives = UIFactory.CreateText(bar.transform, "Lives", "ЖИЗНИ 3", 20, TextAnchor.MiddleCenter, new Vector2(0.37f, 0f), new Vector2(0.52f, 1f), Vector2.zero, Vector2.zero);
        Text score = UIFactory.CreateText(bar.transform, "Score", "СЧЁТ 150", 20, TextAnchor.MiddleCenter, new Vector2(0.51f, 0f), new Vector2(0.68f, 1f), Vector2.zero, Vector2.zero);
        Text timer = UIFactory.CreateText(bar.transform, "Timer", "ВРЕМЯ 00:00", 20, TextAnchor.MiddleRight, new Vector2(0.67f, 0f), new Vector2(0.91f, 1f), Vector2.zero, Vector2.zero);
        HUDController hud = bar.gameObject.AddComponent<HUDController>();
        hud.Configure(controller, crystals, lives, score, timer);
    }

    private static void CreateControls(Transform parent, PlayerController player)
    {
        CreateDirectionButton(parent, player, "LeftButton", "←", new Vector2(-240f, 95f), Vector2.left);
        CreateDirectionButton(parent, player, "RightButton", "→", new Vector2(-80f, 95f), Vector2.right);
        CreateDirectionButton(parent, player, "UpButton", "↑", new Vector2(-160f, 175f), Vector2.up);
        CreateDirectionButton(parent, player, "DownButton", "↓", new Vector2(-160f, 15f), Vector2.down);
    }

    private static void CreateDirectionButton(Transform parent, PlayerController player, string name, string label, Vector2 position, Vector2 direction)
    {
        Button button = CreateFittedButton(parent, name, label, null, position, new Vector2(72f, 72f), new Vector2(1f, 0f));
        MobileInputButton input = button.gameObject.AddComponent<MobileInputButton>();
        input.Configure(player, direction);
    }

    private static void CreateWorldBackground(Transform parent)
    {
        GameObject go = new GameObject("WorldBackground", typeof(SpriteRenderer));
        go.transform.SetParent(parent, false);
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("background");
        renderer.color = Color.white;
        renderer.sortingOrder = -20;
        Vector2 spriteSize = renderer.sprite != null ? renderer.sprite.bounds.size : Vector2.one;
        float scaleX = spriteSize.x > 0.001f ? 16f / spriteSize.x : 1f;
        float scaleY = spriteSize.y > 0.001f ? 9f / spriteSize.y : 1f;
        go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private static void CreateBoundaryWalls(Transform parent)
    {
        CreateWall(parent, "Wall_Top", new Vector2(0f, 4.3f), new Vector2(16f, 0.35f));
        CreateWall(parent, "Wall_Bottom", new Vector2(0f, -4.3f), new Vector2(16f, 0.35f));
        CreateWall(parent, "Wall_Left", new Vector2(-7.8f, 0f), new Vector2(0.35f, 9f));
        CreateWall(parent, "Wall_Right", new Vector2(7.8f, 0f), new Vector2(0.35f, 9f));
    }

    private static void BuildLevelOne(Transform parent, out PlayerController player, out ExitGate gate)
    {
        CreateWall(parent, "Wall_01", new Vector2(-1.5f, 1.1f), new Vector2(0.45f, 4.2f));
        CreateWall(parent, "Wall_02", new Vector2(2.8f, -1.25f), new Vector2(0.45f, 3.6f));
        player = CreatePlayer(parent, new Vector2(-6.2f, -2.7f));
        CreateCollectible(parent, 1, new Vector2(-4.6f, 2.4f));
        CreateCollectible(parent, 2, new Vector2(0.4f, -2.5f));
        CreateCollectible(parent, 3, new Vector2(4.5f, 2.6f));
        CreateHazard(parent, 1, new Vector2(-0.1f, 2.2f));
        CreateHazard(parent, 2, new Vector2(4.0f, -1.8f));
        gate = CreateExit(parent, new Vector2(6.4f, 2.8f));
    }

    private static void BuildLevelTwo(Transform parent, out PlayerController player, out ExitGate gate)
    {
        CreateWall(parent, "Wall_01", new Vector2(-2.7f, 1.3f), new Vector2(0.45f, 4.8f));
        CreateWall(parent, "Wall_02", new Vector2(1.4f, -1.2f), new Vector2(0.45f, 4.8f));
        CreateWall(parent, "Wall_03", new Vector2(4.5f, 1.1f), new Vector2(3.8f, 0.42f));
        player = CreatePlayer(parent, new Vector2(-6.2f, 3.0f));
        CreateCollectible(parent, 1, new Vector2(-5.2f, -2.5f));
        CreateCollectible(parent, 2, new Vector2(-0.8f, 3.0f));
        CreateCollectible(parent, 3, new Vector2(2.8f, -2.6f));
        CreateCollectible(parent, 4, new Vector2(5.4f, 2.8f));
        CreateHazard(parent, 1, new Vector2(-4.1f, 0.3f));
        CreateHazard(parent, 2, new Vector2(-0.5f, -1.1f));
        CreateHazard(parent, 3, new Vector2(2.7f, 2.6f));
        CreateHazard(parent, 4, new Vector2(4.6f, -1.5f));
        gate = CreateExit(parent, new Vector2(6.3f, -3.0f));
    }

    private static void BuildLevelThree(Transform parent, out PlayerController player, out ExitGate gate)
    {
        CreateWall(parent, "Wall_01", new Vector2(-4.1f, 0f), new Vector2(0.42f, 5.5f));
        CreateWall(parent, "Wall_02", new Vector2(-0.9f, 1.4f), new Vector2(0.42f, 5.0f));
        CreateWall(parent, "Wall_03", new Vector2(2.4f, -1.3f), new Vector2(0.42f, 5.0f));
        CreateWall(parent, "Wall_04", new Vector2(5.0f, 1.5f), new Vector2(0.42f, 4.2f));
        CreateWall(parent, "Wall_05", new Vector2(0.7f, -3.2f), new Vector2(3.0f, 0.42f));
        player = CreatePlayer(parent, new Vector2(-6.3f, -3.0f));
        CreateCollectible(parent, 1, new Vector2(-6.0f, 2.7f));
        CreateCollectible(parent, 2, new Vector2(-2.5f, -2.5f));
        CreateCollectible(parent, 3, new Vector2(0.7f, 3.0f));
        CreateCollectible(parent, 4, new Vector2(3.8f, -2.8f));
        CreateCollectible(parent, 5, new Vector2(6.1f, 0.2f));
        CreateHazard(parent, 1, new Vector2(-5.2f, -0.2f));
        CreateHazard(parent, 2, new Vector2(-2.5f, 1.2f));
        CreateHazard(parent, 3, new Vector2(0.6f, -1.4f));
        CreateHazard(parent, 4, new Vector2(1.0f, 2.0f));
        CreateHazard(parent, 5, new Vector2(3.8f, 1.4f));
        CreateHazard(parent, 6, new Vector2(5.7f, -2.2f));
        gate = CreateExit(parent, new Vector2(6.3f, 3.0f));
    }

    private static void CreateWall(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        SpriteRenderer renderer = wall.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("ui_panel");
        renderer.color = new Color(0.27f, 0.32f, 0.39f);
        renderer.sortingOrder = 1;
        Vector2 spriteSize = renderer.sprite != null ? renderer.sprite.bounds.size : Vector2.one;
        wall.transform.localScale = new Vector3(
            spriteSize.x > 0.001f ? size.x / spriteSize.x : size.x,
            spriteSize.y > 0.001f ? size.y / spriteSize.y : size.y,
            1f);
        wall.GetComponent<BoxCollider2D>().size = spriteSize;
    }

    private static PlayerController CreatePlayer(Transform parent, Vector2 position)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Player");
        GameObject go = prefab != null ? Instantiate(prefab, parent) : new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(PlayerController));
        if (go.transform.parent == null) go.transform.SetParent(parent, false);
        go.name = "Player";
        go.tag = "Player";
        go.transform.position = position;
        go.transform.localScale = Vector3.one;
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("player");
        renderer.sortingOrder = 5;
        Rigidbody2D body = go.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        PlayerController player = go.GetComponent<PlayerController>();
        player.SetSpawn(position);
        return player;
    }

    private static void CreateCollectible(Transform parent, int index, Vector2 position)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Collectible");
        GameObject go = prefab != null ? Instantiate(prefab, parent) : new GameObject("Collectible", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Collectible));
        if (go.transform.parent == null) go.transform.SetParent(parent, false);
        go.name = $"Collectible_{index:00}";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.72f;
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("crystal");
        renderer.sortingOrder = 4;
        go.GetComponent<Collider2D>().isTrigger = true;
    }

    private static void CreateHazard(Transform parent, int index, Vector2 position)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Hazard");
        GameObject go = prefab != null ? Instantiate(prefab, parent) : new GameObject("Hazard", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Hazard));
        if (go.transform.parent == null) go.transform.SetParent(parent, false);
        go.name = $"Hazard_{index:00}";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.82f;
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("hazard");
        renderer.sortingOrder = 3;
        go.GetComponent<Collider2D>().isTrigger = true;
    }

    private static ExitGate CreateExit(Transform parent, Vector2 position)
    {
        GameObject go = new GameObject("ExitGate", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(ExitGate));
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.82f, 0.82f);
        return go.GetComponent<ExitGate>();
    }

}
