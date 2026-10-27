using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public sealed class HUDController : MonoBehaviour
{
    private static readonly ProfilerMarker PerFrameMarker = new ProfilerMarker("EnergoKey.HUD.PerFrameRefresh");

    private GameController gameController;
    private Text crystalsText;
    private Text livesText;
    private Text scoreText;
    private Text timerText;

    public void Configure(GameController controller, Text crystals, Text lives, Text score, Text timer)
    {
        gameController = controller;
        crystalsText = crystals;
        livesText = lives;
        scoreText = score;
        timerText = timer;
    }

    private void Update()
    {
        using (PerFrameMarker.Auto())
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (gameController == null) return;
        crystalsText.text = $"КРИСТАЛЛЫ {gameController.CollectedCrystals}/{gameController.TotalCrystals}";
        livesText.text = $"ЖИЗНИ {gameController.Lives}";
        scoreText.text = $"СЧЁТ {gameController.Score}";
        int seconds = Mathf.FloorToInt(gameController.ElapsedTime);
        timerText.text = $"ВРЕМЯ {seconds / 60:00}:{seconds % 60:00}";
    }
}

