using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;
    private float previousTimeScale = 1f;
    private bool paused;

    private void Awake()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(ExitToMainMenu);
    }

    public void TogglePause() { if (paused) ResumeGame(); else PauseGame(); }
    public void PauseGame()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentState != LevelManager.State.Running) return;
        if (paused) return;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        paused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
    }
    public void ResumeGame()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentState != LevelManager.State.Running) return;
        if (!paused) return;
        Time.timeScale = previousTimeScale;
        paused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
    }
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        paused = false;
        SceneManager.LoadScene("MainMenuScene");
    }
    private void OnDisable() { if (paused) Time.timeScale = previousTimeScale; }
}
