using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
   
    public override void Interact(Player player)
    {
       if (!HasKitchenObject())
        {
            //There is no KitchenObject here
            if (player.HasKitchenObject())
            {
                //Player is carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else
            {
                //Player not carrying anything
            }
        }
        else
        {
            //There is a KitchenObject here

            if (player.HasKitchenObject())
            {
                //Player is carrying something

                PlateKitchenObject plateKitchenObject =
                    GetKitchenObject().GetComponent<PlateKitchenObject>();

                if (plateKitchenObject != null)
                {
                    plateKitchenObject.TryAddKitchenObject(
                        player.GetKitchenObject()
                    );
                }
            }
            else
            {
                //Player is not carrying anything

                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

   
}
