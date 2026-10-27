using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        StartSinglePlayer();
    }

    public void StartSinglePlayer()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void StartTwoPlayers()
    {
        SceneManager.LoadScene("GameScene_Double");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
