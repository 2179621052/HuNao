using TMPro;
using UnityEngine;

public sealed class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountText;
    private RectTransform amountTransform;
    private Vector3 baseScale = Vector3.one;
    private float pulseTime;
    private bool subscribed;

    private void Awake()
    {
        MoneyManager.GetOrCreate();
        if (amountText == null) amountText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (amountText == null) return;
        amountText.richText = true;
        amountText.color = Color.white;
        amountText.enabled = true;
        amountTransform = amountText.rectTransform;
        baseScale = amountTransform.localScale;
        RefreshFromManager();
    }

    private void OnEnable() => RefreshFromManager();
    private void Start() => RefreshFromManager();

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnDisable() => Unsubscribe();

    private void Unsubscribe()
    {
        if (subscribed && MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= HandleMoneyChanged;
        subscribed = false;
    }

    private void RefreshFromManager()
    {
        if (amountText == null) amountText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (amountText == null) return;
        MoneyManager manager = MoneyManager.GetOrCreate();
        if (!subscribed)
        {
            manager.OnMoneyChanged += HandleMoneyChanged;
            subscribed = true;
        }
        amountText.enabled = true;
        amountText.color = Color.white;
        amountText.rectTransform.localScale = baseScale;
        amountText.ForceMeshUpdate(true, true);
        amountText.GetComponent<CanvasRenderer>()?.SetAlpha(1f);
        HandleMoneyChanged(manager.CurrentMoney);
        Canvas.ForceUpdateCanvases();
    }

    private void Update()
    {
        if (pulseTime <= 0f || amountTransform == null) return;
        pulseTime -= Time.unscaledDeltaTime;
        float t = 1f - Mathf.Clamp01(pulseTime / 0.2f);
        amountTransform.localScale = baseScale * Mathf.Lerp(1.15f, 1f, t);
        if (pulseTime <= 0f) amountTransform.localScale = baseScale;
    }

    private void HandleMoneyChanged(int amount)
    {
        amountText.text = "<color=#FFD34E>$</color><color=#FFFFFF>" + amount + "</color>";
        amountText.enabled = true;
        pulseTime = 0.2f;
        amountTransform.localScale = baseScale * 1.15f;
        amountText.ForceMeshUpdate(true, true);
    }

    public void ShowSubmission(SubmitCounter source, int amount, string label)
    {
        MoneyManager.Instance?.GetComponent<MoneyPopupUI>()?.Show(source, amount, label);
    }
}
