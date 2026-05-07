using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StolenItemBag : NetworkBehaviour, IInteractable
{
    [Networked] public int TotalStolenCount { get; set; }

    public string GetInteractPrompt(NetworkBehaviour interactor)
    {
        if(interactor is RobberRole robber && robber.IsCarry)
        {
            return "[E] Put";
        }

        return "";
    }

    public void Interact(NetworkBehaviour interactor)
    {
        if(interactor is RobberRole robber && robber.IsCarry)
        {
            robber.PutItemToBag(this);
        }
    }
}
