using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private const string SelectedModeKey = "HuNao.SelectedMode.Double";
    private const string ReturnToLevelSelectKey = "HuNao.ReturnToLevelSelect";
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject[] mainMenuButtonsToHide;
    [SerializeField] private Button[] levelButtons;
    private bool twoPlayerMode;

    private void Awake()
    {
        twoPlayerMode = PlayerPrefs.GetInt(SelectedModeKey, 0) == 1;
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        WireLevelButtons();
        if (PlayerPrefs.GetInt(ReturnToLevelSelectKey, 0) == 1)
        {
            PlayerPrefs.SetInt(ReturnToLevelSelectKey, 0);
            PlayerPrefs.Save();
            OpenLevelSelect(twoPlayerMode);
        }
    }

    public void StartGame() => StartSinglePlayer();
    public void StartSinglePlayer() => OpenLevelSelect(false);
    public void StartTwoPlayers() => OpenLevelSelect(true);

    private void OpenLevelSelect(bool doubleMode)
    {
        twoPlayerMode = doubleMode;
        PlayerPrefs.SetInt(SelectedModeKey, twoPlayerMode ? 1 : 0);
        PlayerPrefs.Save();
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        if (mainMenuButtonsToHide != null)
            foreach (var button in mainMenuButtonsToHide)
                if (button != null) button.SetActive(false);
        RefreshLevelButtons();
    }

    public void CloseLevelSelect()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (mainMenuButtonsToHide != null)
            foreach (var button in mainMenuButtonsToHide)
                if (button != null) button.SetActive(true);
    }

    public void LoadLevel(int level)
    {
        twoPlayerMode = PlayerPrefs.GetInt(SelectedModeKey, twoPlayerMode ? 1 : 0) == 1;
        if (level < 1 || level > 4 || level > GetUnlockedLevel()) return;
        string sceneName = "GameScene " + level + (twoPlayerMode ? "_Double" : string.Empty);
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Level scene is missing or not in Build Settings: " + sceneName);
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void UnlockNextLevel(int completedLevel)
    {
        UnlockNextLevelForMode(completedLevel, twoPlayerMode);
    }

    public static void UnlockNextLevelForMode(int completedLevel, bool doubleMode)
    {
        if (completedLevel < 1 || completedLevel >= 4) return;
        int next = completedLevel + 1;
        string key = doubleMode ? "HuNao.UnlockedLevel.Double" : "HuNao.UnlockedLevel.Single";
        int current = Mathf.Clamp(PlayerPrefs.GetInt(key, 1), 1, 4);
        PlayerPrefs.SetInt(key, Mathf.Max(current, next));
        PlayerPrefs.Save();
    }

    public static void PrepareReturnToLevelSelect(bool doubleMode)
    {
        PlayerPrefs.SetInt(SelectedModeKey, doubleMode ? 1 : 0);
        PlayerPrefs.SetInt(ReturnToLevelSelectKey, 1);
        PlayerPrefs.Save();
    }

    public void QuitGame() => Application.Quit();

    private int GetUnlockedLevel()
    {
        string key = twoPlayerMode ? "HuNao.UnlockedLevel.Double" : "HuNao.UnlockedLevel.Single";
        return Mathf.Clamp(PlayerPrefs.GetInt(key, 1), 1, 4);
    }

    private void WireLevelButtons()
    {
        if (levelButtons == null) return;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i + 1;
            if (levelButtons[i] == null) continue;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => LoadLevel(level));
        }
    }

    private void RefreshLevelButtons()
    {
        int unlocked = GetUnlockedLevel();
        if (levelButtons == null) return;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;
            bool isUnlocked = i + 1 <= unlocked;
            levelButtons[i].interactable = isUnlocked;
            ApplyLevelButtonVisual(levelButtons[i], isUnlocked);
        }
    }

    private void ApplyLevelButtonVisual(Button button, bool isUnlocked)
    {
        var colors = button.colors;
        colors.normalColor = isUnlocked ? new Color(0.18f, 0.65f, 0.30f, 1f) : new Color(0.28f, 0.30f, 0.33f, 1f);
        colors.highlightedColor = isUnlocked ? new Color(0.28f, 0.78f, 0.38f, 1f) : new Color(0.34f, 0.36f, 0.39f, 1f);
        colors.pressedColor = isUnlocked ? new Color(0.12f, 0.48f, 0.22f, 1f) : new Color(0.22f, 0.24f, 0.27f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.24f, 0.25f, 0.27f, 0.9f);
        button.colors = colors;

        var status = button.transform.Find("Status")?.GetComponent<Text>();
        if (status != null)
        {
            status.text = isUnlocked ? "已解锁" : "尚未解锁";
            status.color = isUnlocked ? new Color(0.86f, 1f, 0.88f, 1f) : new Color(0.82f, 0.84f, 0.86f, 1f);
        }

        var icon = button.transform.Find("StateIcon")?.GetComponent<Text>();
        if (icon != null)
        {
            icon.text = isUnlocked ? "开" : "锁";
            icon.color = isUnlocked ? new Color(0.9f, 1f, 0.75f, 1f) : new Color(0.75f, 0.77f, 0.8f, 1f);
        }
    }
}
