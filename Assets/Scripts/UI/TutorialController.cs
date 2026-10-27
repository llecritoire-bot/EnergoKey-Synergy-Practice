using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TutorialController : MonoBehaviour
{
    [Serializable]
    public struct TutorialCard
    {
        public string title;
        [TextArea(3, 6)] public string body;

        public TutorialCard(string cardTitle, string cardBody)
        {
            title = cardTitle;
            body = cardBody;
        }
    }

    private static readonly TutorialCard[] DefaultCards =
    {
        new TutorialCard(
            "Управление",
            "Используйте экранные кнопки со стрелками, чтобы перемещать игрока. В Unity Editor также доступны WASD и стрелки."),
        new TutorialCard(
            "Особое правило",
            "Сначала соберите все энергетические кристаллы. Только после этого выход откроется и позволит завершить уровень."),
        new TutorialCard(
            "Завершение и ошибка",
            "Опасности отнимают одну жизнь и возвращают игрока к старту. Открытый выход приносит победу, а потеря всех трёх жизней — поражение.")
    };

    [SerializeField] private CanvasGroup tutorialGroup;
    [SerializeField] private Text titleText;
    [SerializeField] private Text bodyText;
    [SerializeField] private Text progressText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private bool autoShowOnFirstRun = true;

    private bool hasStarted;

    public int CurrentCardIndex { get; private set; }
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        BindButtons();
        SetHiddenImmediately();
    }

    private void Start()
    {
        hasStarted = true;
        if (autoShowOnFirstRun && !SaveService.TutorialSeen)
        {
            OpenFirstRun();
        }
    }

    public void Configure(
        CanvasGroup panel,
        Text title,
        Text body,
        Text progress,
        Button next,
        Button back,
        Button skip,
        bool showAutomaticallyOnFirstRun = true)
    {
        UnbindButtons();
        tutorialGroup = panel;
        titleText = title;
        bodyText = body;
        progressText = progress;
        nextButton = next;
        backButton = back;
        skipButton = skip;
        autoShowOnFirstRun = showAutomaticallyOnFirstRun;
        BindButtons();
        SetHiddenImmediately();

        if (hasStarted && autoShowOnFirstRun && !SaveService.TutorialSeen)
        {
            OpenFirstRun();
        }
    }

    public void OpenFirstRun()
    {
        if (SaveService.TutorialSeen)
        {
            return;
        }

        OpenFromMenu();
    }

    public void OpenFromMenu()
    {
        CurrentCardIndex = 0;
        IsOpen = true;

        if (tutorialGroup != null)
        {
            tutorialGroup.gameObject.SetActive(true);
            tutorialGroup.alpha = 1f;
            tutorialGroup.interactable = true;
            tutorialGroup.blocksRaycasts = true;
        }

        RefreshCard();
    }

    public void NextCard()
    {
        if (!IsOpen)
        {
            return;
        }

        if (CurrentCardIndex < DefaultCards.Length - 1)
        {
            CurrentCardIndex++;
            RefreshCard();
            return;
        }

        CompleteTutorial();
    }

    public void PreviousCard()
    {
        if (!IsOpen || CurrentCardIndex <= 0)
        {
            return;
        }

        CurrentCardIndex--;
        RefreshCard();
    }

    public void SkipTutorial()
    {
        if (!IsOpen)
        {
            return;
        }

        SaveService.MarkTutorialSeen();
        CloseTutorial();
    }

    public void CompleteTutorial()
    {
        SaveService.MarkTutorialSeen();
        CloseTutorial();
    }

    public void CloseTutorial()
    {
        IsOpen = false;
        SetHiddenImmediately();
    }

    private void RefreshCard()
    {
        CurrentCardIndex = Mathf.Clamp(CurrentCardIndex, 0, DefaultCards.Length - 1);
        TutorialCard card = DefaultCards[CurrentCardIndex];

        if (titleText != null)
        {
            titleText.text = card.title;
        }

        if (bodyText != null)
        {
            bodyText.text = card.body;
        }

        if (progressText != null)
        {
            progressText.text = $"{CurrentCardIndex + 1} / {DefaultCards.Length}";
        }

        if (backButton != null)
        {
            backButton.interactable = CurrentCardIndex > 0;
        }

        SetButtonLabel(nextButton, CurrentCardIndex == DefaultCards.Length - 1 ? "ГОТОВО" : "ДАЛЕЕ");
    }

    private void SetHiddenImmediately()
    {
        IsOpen = false;
        if (tutorialGroup != null)
        {
            tutorialGroup.alpha = 0f;
            tutorialGroup.interactable = false;
            tutorialGroup.blocksRaycasts = false;
        }
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
        }
    }

    private void BindButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextCard);
            nextButton.onClick.AddListener(NextCard);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(PreviousCard);
            backButton.onClick.AddListener(PreviousCard);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipTutorial);
            skipButton.onClick.AddListener(SkipTutorial);
        }
    }

    private void UnbindButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextCard);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(PreviousCard);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipTutorial);
        }
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }
}
