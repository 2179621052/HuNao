using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : MonoBehaviour, IKitchenObjectContainer
{
    private List<KitchenObject> kitchenObjectList;

    private KitchenObject burgerKitchenObject;

    [SerializeField] private KitchenObjectSO burgerSO;
    [SerializeField] private Transform burgerPoint;

    [Header("Ingredient SO")]
    [SerializeField] private KitchenObjectSO breadSO;
    [SerializeField] private KitchenObjectSO meatPattyCookedSO;
    [SerializeField] private KitchenObjectSO cheeseSlicesSO;
    [SerializeField] private KitchenObjectSO tomatoSlicesSO;
    [SerializeField] private KitchenObjectSO cabbageSlicesSO;

    [Header("Ingredient Points")]
    [SerializeField] private Transform breadPoint;
    [SerializeField] private Transform pattyPoint;
    [SerializeField] private Transform cheesePoint;
    [SerializeField] private Transform tomatoPoint;
    [SerializeField] private Transform lettucePoint;


    private void Awake()
    {
        kitchenObjectList = new List<KitchenObject>();
    }


    public bool TryAddKitchenObject(KitchenObject kitchenObject)
    {
        // 汉堡已经完成后，不允许继续加食材
        if (HasBurger())
        {
            return false;
        }

        KitchenObjectSO kitchenObjectSO =
            kitchenObject.GetKitchenObjectSO();

        Transform targetPoint =
            GetPointForKitchenObjectSO(kitchenObjectSO);

        // 不是当前汉堡配方里的食材
        if (targetPoint == null)
        {
            return false;
        }

        // 已经有相同食材
        if (HasKitchenObjectSO(kitchenObjectSO))
        {
            return false;
        }

        kitchenObjectList.Add(kitchenObject);

        kitchenObject.SetKitchenObjectContainer(targetPoint);

        Debug.Log("Plate added: " + kitchenObjectSO.name);

        CheckBurgerComplete();

        return true;
    }


    private void CheckBurgerComplete()
    {
        if (kitchenObjectList.Count != 5)
        {
            return;
        }

        bool hasBread = HasKitchenObjectSO(breadSO);
        bool hasPatty = HasKitchenObjectSO(meatPattyCookedSO);
        bool hasCheese = HasKitchenObjectSO(cheeseSlicesSO);
        bool hasTomato = HasKitchenObjectSO(tomatoSlicesSO);
        bool hasCabbage = HasKitchenObjectSO(cabbageSlicesSO);

        if (hasBread &&
            hasPatty &&
            hasCheese &&
            hasTomato &&
            hasCabbage)
        {
            CompleteBurger();
        }
    }


    private void CompleteBurger()
    {
        foreach (KitchenObject kitchenObject in kitchenObjectList)
        {
            kitchenObject.DestroySelf();
        }

        kitchenObjectList.Clear();

        Transform burgerTransform =
            Instantiate(burgerSO.prefab);

        burgerTransform.parent = burgerPoint;
        burgerTransform.localPosition = Vector3.zero;

        burgerKitchenObject =
            burgerTransform.GetComponent<KitchenObject>();

        Debug.Log("Burger completed!");
    }


    private Transform GetPointForKitchenObjectSO(
        KitchenObjectSO kitchenObjectSO)
    {
        if (kitchenObjectSO == breadSO)
        {
            return breadPoint;
        }

        if (kitchenObjectSO == meatPattyCookedSO)
        {
            return pattyPoint;
        }

        if (kitchenObjectSO == cheeseSlicesSO)
        {
            return cheesePoint;
        }

        if (kitchenObjectSO == tomatoSlicesSO)
        {
            return tomatoPoint;
        }

        if (kitchenObjectSO == cabbageSlicesSO)
        {
            return lettucePoint;
        }

        return null;
    }


    private bool HasKitchenObjectSO(
        KitchenObjectSO kitchenObjectSO)
    {
        foreach (KitchenObject kitchenObject in kitchenObjectList)
        {
            if (kitchenObject.GetKitchenObjectSO() == kitchenObjectSO)
            {
                return true;
            }
        }

        return false;
    }


    public bool HasBurger()
    {
        return burgerKitchenObject != null;
    }


    public KitchenObject GetBurgerKitchenObject()
    {
        return burgerKitchenObject;
    }


    public List<KitchenObject> GetKitchenObjectList()
    {
        return kitchenObjectList;
    }
}