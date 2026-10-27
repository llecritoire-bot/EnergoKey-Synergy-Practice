using System.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public sealed class HUDController : MonoBehaviour
{
    private static readonly ProfilerMarker EventRefreshMarker = new ProfilerMarker("EnergoKey.HUD.EventRefresh");

    private GameController gameController;
    private Text crystalsText;
    private Text livesText;
    private Text scoreText;
    private Text timerText;
    private Coroutine timerRoutine;
    private int displayedSecond = -1;

    public void Configure(GameController controller, Text crystals, Text lives, Text score, Text timer)
    {
        Unsubscribe();
        gameController = controller;
        crystalsText = crystals;
        livesText = lives;
        scoreText = score;
        timerText = timer;
        Subscribe();
        RefreshState();
        StartTimerRoutine();
    }

    private void OnEnable()
    {
        Subscribe();
        StartTimerRoutine();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopTimerRoutine();
    }

    private void RefreshState()
    {
        if (gameController == null) return;

        using (EventRefreshMarker.Auto())
        {
            crystalsText.text = $"КРИСТАЛЛЫ {gameController.CollectedCrystals}/{gameController.TotalCrystals}";
            livesText.text = $"ЖИЗНИ {gameController.Lives}";
            scoreText.text = $"СЧЁТ {gameController.Score}";
            RefreshTimer(force: true);
        }
    }

    private IEnumerator RefreshTimerRoutine()
    {
        while (true)
        {
            RefreshTimer(force: false);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void RefreshTimer(bool force)
    {
        if (gameController == null || timerText == null) return;

        int seconds = Mathf.FloorToInt(gameController.ElapsedTime);
        if (!force && seconds == displayedSecond) return;

        displayedSecond = seconds;
        timerText.text = $"ВРЕМЯ {seconds / 60:00}:{seconds % 60:00}";
    }

    private void Subscribe()
    {
        if (gameController == null) return;
        gameController.StateChanged -= RefreshState;
        gameController.StateChanged += RefreshState;
    }

    private void Unsubscribe()
    {
        if (gameController != null)
        {
            gameController.StateChanged -= RefreshState;
        }
    }

    private void StartTimerRoutine()
    {
        if (!isActiveAndEnabled || timerRoutine != null) return;
        timerRoutine = StartCoroutine(RefreshTimerRoutine());
    }

    private void StopTimerRoutine()
    {
        if (timerRoutine == null) return;
        StopCoroutine(timerRoutine);
        timerRoutine = null;
    }
}

