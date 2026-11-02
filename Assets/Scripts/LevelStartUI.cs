using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LevelStartUI : MonoBehaviour
{
    [SerializeField] private GameObject levelStartRoot;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Image promptImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button pauseButton;
    public event Action OnGameStarted;

    private bool started;

    private void Awake()
    {
        Time.timeScale = 0f;
        GameInput.GameplayInputEnabled = false;
        if (levelStartRoot != null) levelStartRoot.SetActive(true);
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(BeginGame);
            confirmButton.onClick.AddListener(BeginGame);
        }
        if (pauseButton != null) pauseButton.interactable = false;
    }

    public void BeginGame()
    {
        if (started) return;
        started = true;
        GameInput.GameplayInputEnabled = true;
        Time.timeScale = 1f;
        if (levelStartRoot != null) levelStartRoot.SetActive(false);
        if (pauseButton != null) pauseButton.interactable = true;
        OnGameStarted?.Invoke();
    }

    private void OnDestroy()
    {
        if (!started)
        {
            GameInput.GameplayInputEnabled = true;
            Time.timeScale = 1f;
        }
    }
}
