using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private static bool pendingTwoPlayers;

    public void StartGame()
    {
        StartSinglePlayer();
    }

    public void StartSinglePlayer()
    {
        LoadGame(false);
    }

    public void StartTwoPlayers()
    {
        LoadGame(true);
    }

    private static void LoadGame(bool twoPlayers)
    {
        pendingTwoPlayers = twoPlayers;
        SceneManager.sceneLoaded -= OnGameSceneLoaded;
        SceneManager.sceneLoaded += OnGameSceneLoaded;
        SceneManager.LoadScene("GameScene");
    }

    private static void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "GameScene")
        {
            return;
        }

        SceneManager.sceneLoaded -= OnGameSceneLoaded;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Player2")
            {
                root.SetActive(pendingTwoPlayers);
                break;
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
