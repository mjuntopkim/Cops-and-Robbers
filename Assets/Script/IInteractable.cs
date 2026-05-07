using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public interface IInteractable
{
    string GetInteractPrompt(NetworkBehaviour interactor);

    void Interact(NetworkBehaviour interactor);
}
