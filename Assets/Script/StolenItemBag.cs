using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StolenItemBag : NetworkBehaviour
{
    [Networked] private int TotalStolenCount { get; set; }

    public void AddItem()
    {
        if (Object.HasStateAuthority)
        {
            TotalStolenCount++;
            Debug.Log(TotalStolenCount);
        }
    }
}
