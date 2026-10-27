using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameRunState
{
    Playing,
    Victory,
    Defeat
}

public sealed class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public event Action StateChanged;

    public int LevelIndex { get; private set; } = 1;
    public int CollectedCrystals { get; private set; }
    public int TotalCrystals { get; private set; }
    public int Lives { get; private set; } = GameRules.StartingLives;
    public int Score => GameRules.CalculateScore(CollectedCrystals, Lives);
    public float ElapsedTime { get; private set; }
    public GameRunState State { get; private set; } = GameRunState.Playing;

    private PlayerController player;
    private ExitGate exitGate;
    private Text feedbackText;
    private GameObject victoryPanel;
    private GameObject defeatPanel;
    private Text victorySummaryText;
    private Text defeatSummaryText;
    private float damageBlockedUntil;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        TotalCrystals = FindObjectsByType<Collectible>().Length;
        Lives = GameRules.StartingLives;
        CollectedCrystals = 0;
        State = GameRunState.Playing;
        ElapsedTime = 0f;
        exitGate?.SetOpen(false);
        SetPanel(victoryPanel, false);
        SetPanel(defeatPanel, false);
        NotifyChanged();
    }

    private void Update()
    {
        if (State == GameRunState.Playing)
        {
            ElapsedTime += Time.deltaTime;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F3)) CollectAllForEvidence();
        if (Input.GetKeyDown(KeyCode.F4)) WinForEvidence();
        if (Input.GetKeyDown(KeyCode.F5)) DefeatForEvidence();
        if (Input.GetKeyDown(KeyCode.F6)) TryUseExit();
#endif
    }

    public void Configure(
        int level,
        PlayerController playerController,
        ExitGate gate,
        Text feedback,
        GameObject victory,
        GameObject defeat,
        Text victorySummary = null,
        Text defeatSummary = null)
    {
        LevelIndex = Mathf.Clamp(level, 1, GameRules.MaxLevel);
        player = playerController;
        exitGate = gate;
        feedbackText = feedback;
        victoryPanel = victory;
        defeatPanel = defeat;
        victorySummaryText = victorySummary;
        defeatSummaryText = defeatSummary;
    }

    public void Collect(Collectible collectible)
    {
        if (State != GameRunState.Playing || collectible == null) return;
        collectible.Consume();
        CollectedCrystals++;
        AudioService.Ensure().Play("collect");
        ShowFeedback("Кристалл получен", ThemeAssets.Yellow);
        if (CollectedCrystals >= TotalCrystals)
        {
            exitGate?.SetOpen(true);
            ShowFeedback("Все кристаллы собраны. Выход открыт", ThemeAssets.Green);
        }
        NotifyChanged();
    }

    public void DamagePlayer()
    {
        if (State != GameRunState.Playing || Time.unscaledTime < damageBlockedUntil) return;
        damageBlockedUntil = Time.unscaledTime + 0.8f;
        Lives--;
        AudioService.Ensure().Play("damage");
        if (Lives <= 0)
        {
            Lose();
        }
        else
        {
            player?.Respawn();
            ShowFeedback("Опасность: потеряна жизнь", ThemeAssets.Red);
            NotifyChanged();
        }
    }

    public void TryUseExit()
    {
        if (State != GameRunState.Playing) return;
        if (CollectedCrystals < TotalCrystals)
        {
            int remaining = TotalCrystals - CollectedCrystals;
            exitGate?.LockedFeedback();
            AudioService.Ensure().Play("error");
            ShowFeedback($"Выход заблокирован. Осталось кристаллов: {remaining}", ThemeAssets.Red);
            return;
        }
        Win();
    }

    public void Restart()
    {
        SceneNavigator.RestartCurrent();
    }

    public void NextOrMenu()
    {
        if (LevelIndex >= GameRules.MaxLevel)
        {
            SceneNavigator.MainMenu();
        }
        else
        {
            SceneNavigator.Load($"Level{LevelIndex + 1}");
        }
    }

#if UNITY_EDITOR
    private void CollectAllForEvidence()
    {
        if (State != GameRunState.Playing) return;
        foreach (Collectible collectible in FindObjectsByType<Collectible>())
        {
            if (collectible != null && collectible.gameObject.activeInHierarchy)
            {
                Collect(collectible);
            }
        }
    }

    private void WinForEvidence()
    {
        if (State != GameRunState.Playing) return;
        CollectAllForEvidence();
        Win();
    }

    private void DefeatForEvidence()
    {
        if (State != GameRunState.Playing) return;
        Lives = 0;
        Lose();
    }
#endif

    private void Win()
    {
        State = GameRunState.Victory;
        player?.SetInputEnabled(false);
        SaveService.RegisterVictory(LevelIndex, Score);
        AudioService.Ensure().Play("victory");
        if (victorySummaryText != null)
        {
            string heading = LevelIndex >= GameRules.MaxLevel
                ? "Энергоключ восстановлен! Все три уровня пройдены."
                : $"Уровень {LevelIndex} пройден. Открыт следующий уровень.";
            victorySummaryText.text = $"{heading}\nКристаллы: {CollectedCrystals}/{TotalCrystals}   Жизни: {Lives}   Счёт: {Score}";
        }
        SetPanel(victoryPanel, true);
        Time.timeScale = 0f;
        NotifyChanged();
    }

    private void Lose()
    {
        Lives = 0;
        State = GameRunState.Defeat;
        player?.SetInputEnabled(false);
        AudioService.Ensure().Play("defeat");
        if (defeatSummaryText != null)
        {
            defeatSummaryText.text = $"Энергия исчерпана на уровне {LevelIndex}.\nКристаллы: {CollectedCrystals}/{TotalCrystals}   Счёт: {Score}";
        }
        SetPanel(defeatPanel, true);
        Time.timeScale = 0f;
        NotifyChanged();
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackRoutine(message, color));
    }

    private IEnumerator FeedbackRoutine(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        feedbackText.gameObject.SetActive(false);
        feedbackRoutine = null;
    }

    private static void SetPanel(GameObject panel, bool value)
    {
        if (panel == null) return;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            panel.SetActive(value);
            return;
        }

        panel.SetActive(true);
        group.alpha = value ? 1f : 0f;
        group.interactable = value;
        group.blocksRaycasts = value;
    }

    private void NotifyChanged()
    {
        StateChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

