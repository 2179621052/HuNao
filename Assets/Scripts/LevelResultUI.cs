using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class LevelResultUI : MonoBehaviour
{
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button winConfirmButton;
    [SerializeField] private Button loseConfirmButton;

    private bool resultShown;
    private bool returnRequested;
    private bool twoPlayerMode;
    private bool subscribed;

    private void Awake()
    {
        if (resultRoot != null) resultRoot.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        if (winConfirmButton != null)
        {
            winConfirmButton.onClick.RemoveListener(ReturnToLevelSelect);
            winConfirmButton.onClick.AddListener(ReturnToLevelSelect);
        }
        if (loseConfirmButton != null)
        {
            loseConfirmButton.onClick.RemoveListener(ReturnToLevelSelect);
            loseConfirmButton.onClick.AddListener(ReturnToLevelSelect);
        }

        var sceneName = SceneManager.GetActiveScene().name;
        twoPlayerMode = sceneName.EndsWith("_Double", StringComparison.OrdinalIgnoreCase);

        SubscribeToLevelManager();
    }

    private void Start()
    {
        SubscribeToLevelManager();
        if (LevelManager.Instance == null) return;
        if (LevelManager.Instance.CurrentState == LevelManager.State.Won) ShowWin();
        else if (LevelManager.Instance.CurrentState == LevelManager.State.Failed) ShowLose();
    }

    private void SubscribeToLevelManager()
    {
        if (subscribed || LevelManager.Instance == null) return;
        subscribed = true;
        LevelManager.Instance.OnLevelWon += ShowWin;
        LevelManager.Instance.OnLevelFailed += ShowLose;
    }

    private void ShowWin()
    {
        if (resultShown) return;
        resultShown = true;
        GameInput.GameplayInputEnabled = false;
        Time.timeScale = 0f;

        var manager = LevelManager.Instance;
        if (manager != null)
        {
            MainMenuUI.UnlockNextLevelForMode(manager.LevelNumber, twoPlayerMode);
        }

        if (resultRoot != null) resultRoot.SetActive(true);
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);
    }

    private void ShowLose()
    {
        if (resultShown) return;
        resultShown = true;
        GameInput.GameplayInputEnabled = false;
        Time.timeScale = 0f;

        if (resultRoot != null) resultRoot.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(true);
    }

    public void ReturnToLevelSelect()
    {
        if (!resultShown || returnRequested) return;
        returnRequested = true;
        Time.timeScale = 1f;
        GameInput.GameplayInputEnabled = true;
        MainMenuUI.PrepareReturnToLevelSelect(twoPlayerMode);
        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnDestroy()
    {
        if (subscribed && LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelWon -= ShowWin;
            LevelManager.Instance.OnLevelFailed -= ShowLose;
        }
    }
}