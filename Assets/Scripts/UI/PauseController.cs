using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PauseController : MonoBehaviour
{
    [SerializeField] private CanvasGroup pauseGroup;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Text pausedTimerText;
    [SerializeField, Min(0f)] private float fadeDuration = 0.2f;
    [SerializeField] private bool pauseOnFocusLoss = true;

    private GameController timerSource;
    private Coroutine transitionRoutine;
    private float timeScaleBeforePause = 1f;
    private bool transitionLocked;
    private bool applicationHasFocus = true;

    public bool IsPaused { get; private set; }
    public bool IsTransitioning => transitionLocked;

    private void Awake()
    {
        BindButtons();
        SetHiddenImmediately();
    }

    public void Configure(
        CanvasGroup panel,
        Button pauseTrigger = null,
        Button resumeTrigger = null,
        float transitionSeconds = 0.2f,
        bool autoPauseOnFocusLoss = true,
        GameController pausedTimerSource = null,
        Text timerText = null)
    {
        UnbindButtons();
        pauseGroup = panel;
        pauseButton = pauseTrigger;
        resumeButton = resumeTrigger;
        timerSource = pausedTimerSource;
        pausedTimerText = timerText;
        fadeDuration = Mathf.Max(0f, transitionSeconds);
        pauseOnFocusLoss = autoPauseOnFocusLoss;
        BindButtons();

        if (IsPaused && pauseGroup != null)
        {
            pauseGroup.gameObject.SetActive(true);
            pauseGroup.alpha = 1f;
            pauseGroup.interactable = true;
            pauseGroup.blocksRaycasts = true;
        }
        else
        {
            SetHiddenImmediately();
        }
    }

    public void PauseGame()
    {
        if (IsPaused || transitionLocked || (timerSource != null && timerSource.State != GameRunState.Playing))
        {
            return;
        }

        timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        IsPaused = true;
        transitionLocked = true;
        RefreshPausedTimer();

        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }

        if (pauseGroup == null)
        {
            transitionLocked = false;
            return;
        }

        pauseGroup.gameObject.SetActive(true);
        pauseGroup.alpha = 0f;
        pauseGroup.interactable = false;
        pauseGroup.blocksRaycasts = true;

        if (!isActiveAndEnabled || fadeDuration <= 0f)
        {
            pauseGroup.alpha = 1f;
            pauseGroup.interactable = true;
            transitionLocked = false;
            return;
        }

        BeginTransition(FadeIn());
    }

    public void ResumeGame()
    {
        if (!IsPaused || transitionLocked)
        {
            return;
        }

        transitionLocked = true;
        if (pauseGroup == null)
        {
            CompleteResume();
            return;
        }

        pauseGroup.interactable = false;
        pauseGroup.blocksRaycasts = true;

        if (!isActiveAndEnabled || fadeDuration <= 0f)
        {
            CompleteResume();
            return;
        }

        BeginTransition(FadeOut());
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void LateUpdate()
    {
        if (IsPaused)
        {
            RefreshPausedTimer();
        }
    }

    private void RefreshPausedTimer()
    {
        if (pausedTimerText == null || timerSource == null)
        {
            return;
        }

        int seconds = Mathf.Max(0, Mathf.FloorToInt(timerSource.ElapsedTime));
        pausedTimerText.text = $"ТАЙМЕР ОСТАНОВЛЕН  {seconds / 60:00}:{seconds % 60:00}";
    }

    private void BeginTransition(IEnumerator transition)
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(transition);
    }

    private IEnumerator FadeIn()
    {
        yield return Fade(0f, 1f);
        pauseGroup.alpha = 1f;
        pauseGroup.interactable = true;
        pauseGroup.blocksRaycasts = true;
        transitionLocked = false;
        transitionRoutine = null;
    }

    private IEnumerator FadeOut()
    {
        yield return Fade(pauseGroup.alpha, 0f);
        CompleteResume();
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeDuration <= 0f)
        {
            if (pauseGroup != null)
            {
                pauseGroup.alpha = to;
            }
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = progress * progress * (3f - 2f * progress);
            if (pauseGroup != null)
            {
                pauseGroup.alpha = Mathf.LerpUnclamped(from, to, eased);
            }
            yield return null;
        }
    }

    private void CompleteResume()
    {
        if (pauseGroup != null)
        {
            pauseGroup.alpha = 0f;
            pauseGroup.interactable = false;
            pauseGroup.blocksRaycasts = false;
        }

        Time.timeScale = timeScaleBeforePause;
        IsPaused = false;
        transitionLocked = false;
        transitionRoutine = null;

        if (pauseButton != null)
        {
            pauseButton.interactable = true;
        }

        if (!applicationHasFocus && pauseOnFocusLoss)
        {
            PauseGame();
        }
    }

    private void SetHiddenImmediately()
    {
        if (pauseGroup != null)
        {
            pauseGroup.alpha = 0f;
            pauseGroup.interactable = false;
            pauseGroup.blocksRaycasts = false;
        }
    }

    private void BindButtons()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseGame);
            pauseButton.onClick.AddListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }
    }

    private void UnbindButtons()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        applicationHasFocus = hasFocus;
        if (!hasFocus && pauseOnFocusLoss)
        {
            PauseGame();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        applicationHasFocus = !pauseStatus;
        if (pauseStatus && pauseOnFocusLoss)
        {
            PauseGame();
        }
    }

    private void OnDestroy()
    {
        UnbindButtons();
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (IsPaused)
        {
            Time.timeScale = timeScaleBeforePause;
            IsPaused = false;
        }
    }
}
