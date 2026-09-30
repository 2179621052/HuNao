using System.Collections.Generic;

public interface IKitchenObjectContainer
{
    bool TryAddKitchenObject(KitchenObject kitchenObject);

    List<KitchenObject> GetKitchenObjectList();
}