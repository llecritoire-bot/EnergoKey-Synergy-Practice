using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneNavigator
{
    public static void Load(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public static void RestartCurrent()
    {
        Load(SceneManager.GetActiveScene().name);
    }

    public static void MainMenu()
    {
        Load("MainMenu");
    }
}

