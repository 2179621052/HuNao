using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MoneyPopupUI : MonoBehaviour
{
    private sealed class Entry
    {
        public TextMeshProUGUI text;
        public CanvasGroup group;
        public BaseCounter source;
        public float age;
        public int slot;
    }

    private readonly List<Entry> entries = new List<Entry>();
    private RectTransform canvasRect;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
    }

    public void Show(SubmitCounter source, int amount, string label)
    {
        Show((BaseCounter)source, amount, label);
    }

    public void Show(BaseCounter source, int amount, string label)
    {
        if (canvas == null) return;
        if (entries.Count >= 3) RemoveEntry(entries[0]);
        var go = new GameObject("IncomePopup");
        go.transform.SetParent(canvas.transform, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.richText = true;
        text.fontSize = 28f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.text = "<color=#FFD34E>+$" + amount + "</color>";
        var group = go.AddComponent<CanvasGroup>();
        var rect = text.rectTransform;
        rect.sizeDelta = new Vector2(300f, 55f);
        var entry = new Entry { text = text, group = group, source = source, age = 0f, slot = entries.Count };
        entries.Add(entry);
        UpdatePosition(entry);
    }

    private void Update()
    {
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var entry = entries[i];
            entry.age += Time.unscaledDeltaTime;
            UpdatePosition(entry);
            float progress = Mathf.Clamp01(entry.age / 1.8f);
            entry.group.alpha = 1f - progress;
            if (entry.age >= 1.8f) RemoveEntry(entry);
        }
    }

    private void UpdatePosition(Entry entry)
    {
        if (entry.source == null || Camera.main == null || canvasRect == null) return;
        Transform anchor = entry.source.GetKitchenObjectFollowTransfrom();
        Vector3 world = (anchor != null ? anchor.position : entry.source.transform.position) + Vector3.up * 1.25f;
        Vector3 screen = Camera.main.WorldToScreenPoint(world);
        if (screen.z <= 0f) { entry.group.alpha = 0f; return; }
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local))
        {
            local.y += 42f * entry.slot + Mathf.Lerp(0f, 50f, Mathf.Clamp01(entry.age / 1.8f));
            entry.text.rectTransform.anchoredPosition = local;
        }
    }

    private void RemoveEntry(Entry entry)
    {
        entries.Remove(entry);
        if (entry.text != null) Destroy(entry.text.gameObject);
    }
}
