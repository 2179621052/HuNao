using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GuestCounter : BaseCounter, IHasProgress
{
    public enum OrderType { TomatoSoup, Burger, Salad }

    private const int GuestRewardMultiplier = 10;

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    [Header("Order References")]
    [SerializeField] private KitchenObjectSO tomatoSoupSO;
    [SerializeField] private KitchenObjectSO burgerSO;
    [SerializeField] private KitchenObjectSO cabbageSlicesSO;
    [SerializeField] private KitchenObjectSO tomatoSlicesSO;
    [SerializeField] private GuestOrderUI orderUI;
    [SerializeField, Min(0.1f)] private float orderDuration = 30f;

    private OrderType currentOrder;
    private float orderTimeRemaining;
    private bool orderActive;
    private readonly List<OrderType> availableOrders = new List<OrderType>(3);

    private void Awake()
    {
        if (orderUI != null)
        {
            orderUI.SpriteConfigurationChanged += OrderUI_SpriteConfigurationChanged;
        }
        RollNextOrder();
        StartCoroutine(NotifyInitialProgressNextFrame());
    }

    public OrderType CurrentOrder => currentOrder;

    private void RollNextOrder()
    {
        BuildAvailableOrders();
        if (availableOrders.Count == 0)
        {
            orderActive = false;
            orderUI?.ClearOrder();
            TriggerProgressChanged(0f);
            return;
        }

        currentOrder = availableOrders[UnityEngine.Random.Range(0, availableOrders.Count)];
        orderTimeRemaining = Mathf.Max(0.1f, orderDuration);
        orderActive = true;
        orderUI?.SetOrder(currentOrder);
        TriggerProgressChanged(1f);
    }

    private void BuildAvailableOrders()
    {
        availableOrders.Clear();
        if (orderUI == null) return;
        foreach (OrderType order in Enum.GetValues(typeof(OrderType)))
        {
            if (orderUI.IsOrderEnabled(order)) availableOrders.Add(order);
        }
    }

    private void OrderUI_SpriteConfigurationChanged()
    {
        BuildAvailableOrders();
        if (availableOrders.Count == 0)
        {
            orderActive = false;
            orderUI.ClearOrder();
            TriggerProgressChanged(0f);
        }
        else if (!orderActive || !availableOrders.Contains(currentOrder))
        {
            RollNextOrder();
        }
    }

    private IEnumerator NotifyInitialProgressNextFrame()
    {
        yield return null;
        if (isActiveAndEnabled && orderActive)
        {
            TriggerProgressChanged(1f);
        }
    }

    private void Update()
    {
        if (!orderActive) return;

        orderTimeRemaining -= Time.deltaTime;
        float progress = Mathf.Clamp01(orderTimeRemaining / Mathf.Max(0.1f, orderDuration));
        TriggerProgressChanged(progress);

        if (orderTimeRemaining <= 0f)
        {
            orderActive = false;
            TriggerProgressChanged(0f);
            AudioManager.PlayDeliveryFail();
            RollNextOrder();
        }
    }

    public override void Interact(Player player)
    {
        if (player == null || !player.HasKitchenObject()) return;
        var held = player.GetKitchenObject();
        if (held == null || !Matches(held)) return;
        if (!orderActive || orderTimeRemaining <= 0f) return;

        float remainingProgress = Mathf.Clamp01(orderTimeRemaining / Mathf.Max(0.1f, orderDuration));
        int basePrice = currentOrder == OrderType.TomatoSoup ? 5 : currentOrder == OrderType.Burger ? 25 : 8;
        int amount = Mathf.Max(1, Mathf.RoundToInt(basePrice * GuestRewardMultiplier * remainingProgress));
        held.DestroySelf();
        MoneyManager.Instance?.AddMoney(amount, this, string.Empty);
        AudioManager.PlayDeliverySuccess();
        RollNextOrder();
    }

    private void TriggerProgressChanged(float progressNormalized)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = Mathf.Clamp01(progressNormalized)
        });
    }

    private void OnDisable()
    {
        if (orderUI != null)
        {
            orderUI.SpriteConfigurationChanged -= OrderUI_SpriteConfigurationChanged;
        }
        orderActive = false;
        TriggerProgressChanged(0f);
        StopAllCoroutines();
    }

    private bool Matches(KitchenObject held)
    {
        if (currentOrder == OrderType.TomatoSoup)
            return held.GetKitchenObjectSO() == tomatoSoupSO;

        var plate = held.GetComponent<PlateKitchenObject>();
        if (plate == null) return false;

        if (currentOrder == OrderType.Burger)
            return plate.HasBurger() && plate.GetBurgerKitchenObject() != null &&
                   plate.GetBurgerKitchenObject().GetKitchenObjectSO() == burgerSO;

        var list = plate.GetKitchenObjectList();
        if (list == null || list.Count != 2 || cabbageSlicesSO == null || tomatoSlicesSO == null) return false;
        bool cabbage = false, tomato = false;
        foreach (var item in list)
        {
            if (item == null) return false;
            var so = item.GetKitchenObjectSO();
            if (so == cabbageSlicesSO) { if (cabbage) return false; cabbage = true; }
            else if (so == tomatoSlicesSO) { if (tomato) return false; tomato = true; }
            else return false;
        }
        return cabbage && tomato;
    }
}
