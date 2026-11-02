using UnityEngine;

public class SubmitCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO tomatoSoupSO;
    [SerializeField] private KitchenObjectSO burgerSO;
    [SerializeField] private KitchenObjectSO cabbageSlicesSO;
    [SerializeField] private KitchenObjectSO tomatoSlicesSO;

    public override void Interact(Player player)
    {
        if (player == null || !player.HasKitchenObject()) return;

        KitchenObject heldObject = player.GetKitchenObject();
        if (heldObject == null) return;
        KitchenObjectSO submittedSO = heldObject.GetKitchenObjectSO();
        // A completed burger is carried by the plate object; the burger itself
        // is the plate's nested KitchenObject. Identify it through that real SO.
        PlateKitchenObject plate = heldObject.GetComponent<PlateKitchenObject>();
        if (plate != null && IsSalad(plate))
        {
            // Capture the recipe result before destroying the plate hierarchy.
            MoneyManager.Instance?.AddMoney(8, this, "沙拉");
            AudioManager.PlayDeliverySuccess();
            heldObject.DestroySelf();
            return;
        }

        bool isBurgerOnPlate = plate != null && plate.HasBurger() &&
            plate.GetBurgerKitchenObject() != null &&
            plate.GetBurgerKitchenObject().GetKitchenObjectSO() == burgerSO;
        int amount;
        string label;
        if (submittedSO == tomatoSoupSO) { amount = 5; label = "番茄汤"; }
        else if (submittedSO == burgerSO || isBurgerOnPlate) { amount = 25; label = "汉堡"; }
        else return;
        MoneyManager.Instance?.AddMoney(amount, this, label);
        AudioManager.PlayDeliverySuccess();
        heldObject.DestroySelf();
    }

    private bool IsSalad(PlateKitchenObject plate)
    {
        if (plate == null || cabbageSlicesSO == null || tomatoSlicesSO == null)
            return false;

        var ingredients = plate.GetKitchenObjectList();
        if (ingredients == null || ingredients.Count != 2)
            return false;

        bool hasCabbage = false;
        bool hasTomato = false;
        foreach (KitchenObject ingredient in ingredients)
        {
            if (ingredient == null) return false;
            KitchenObjectSO ingredientSO = ingredient.GetKitchenObjectSO();
            if (ingredientSO == cabbageSlicesSO)
            {
                if (hasCabbage) return false;
                hasCabbage = true;
            }
            else if (ingredientSO == tomatoSlicesSO)
            {
                if (hasTomato) return false;
                hasTomato = true;
            }
            else
            {
                return false;
            }
        }

        return hasCabbage && hasTomato;
    }
}
