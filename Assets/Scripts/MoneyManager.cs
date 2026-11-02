using System;
using UnityEngine;

public sealed class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }
    public event Action<int> OnMoneyChanged;
    public int CurrentMoney { get; private set; }

    public void ResetMoney()
    {
        if (CurrentMoney == 0) return;
        CurrentMoney = 0;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateAtRuntime()
    {
        GetOrCreate();
    }

    public static MoneyManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        Instance = FindFirstObjectByType<MoneyManager>();
        if (Instance != null) return Instance;
        var go = new GameObject("MoneySystem");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<MoneyManager>();
        var popupCanvas = new GameObject("IncomePopupCanvas");
        popupCanvas.transform.SetParent(go.transform, false);
        var canvas = popupCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;
        var scaler = popupCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        go.AddComponent<MoneyPopupUI>();
        return Instance;
    }

    public void AddMoney(int amount, SubmitCounter source, string label)
    {
        AddMoneyInternal(amount, source, label);
    }

    public void AddMoney(int amount, BaseCounter source, string label)
    {
        AddMoneyInternal(amount, source, label);
    }

    private void AddMoneyInternal(int amount, BaseCounter source, string label)
    {
        if (amount <= 0) return;
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        // MoneyUI lives in the scene HUD; popup UI lives on the persistent manager.
        GetComponent<MoneyPopupUI>()?.Show(source, amount, label);
    }
}
