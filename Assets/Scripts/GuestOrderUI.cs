using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GuestOrderUI : MonoBehaviour
{
    public event Action SpriteConfigurationChanged;
    [SerializeField] private Transform orderAnchor;
    [SerializeField] private Sprite tomatoSoupSprite;
    [SerializeField] private Sprite burgerSprite;
    [SerializeField] private Sprite saladSprite;
    [SerializeField] private float worldScale = 0.01f;
    private GameObject iconObject;
    private Image icon;

    public void SetOrder(GuestCounter.OrderType order)
    {
        if (icon == null) CreateIcon();
        if (icon == null) return;
        icon.sprite = order == GuestCounter.OrderType.TomatoSoup ? tomatoSoupSprite :
            order == GuestCounter.OrderType.Burger ? burgerSprite : saladSprite;
        icon.enabled = icon.sprite != null;
    }

    public bool IsOrderEnabled(GuestCounter.OrderType order)
    {
        return GetSprite(order) != null;
    }

    public void ClearOrder()
    {
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

    public void NotifySpriteConfigurationChanged()
    {
        SpriteConfigurationChanged?.Invoke();
    }

    private Sprite GetSprite(GuestCounter.OrderType order)
    {
        return order == GuestCounter.OrderType.TomatoSoup ? tomatoSoupSprite :
            order == GuestCounter.OrderType.Burger ? burgerSprite : saladSprite;
    }

    private void CreateIcon()
    {
        if (orderAnchor == null) return;
        iconObject = new GameObject("GuestOrderIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(orderAnchor, false);
        var canvas = iconObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 120;
        iconObject.AddComponent<GraphicRaycaster>();
        icon = iconObject.GetComponent<Image>();
        icon.raycastTarget = false;
        var rect = icon.rectTransform;
        rect.sizeDelta = new Vector2(80f, 80f);
        rect.localScale = Vector3.one * worldScale;
        rect.localPosition = Vector3.zero;
        if (Camera.main != null) iconObject.transform.forward = Camera.main.transform.forward;
    }
}
