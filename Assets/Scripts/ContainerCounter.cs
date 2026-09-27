using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ContainerCounter : BaseCounter
{

    //public event EventHandler OnPlayerGrabbefObject;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    


    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

            //OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }


    
}
