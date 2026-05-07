using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class FuseBox : NetworkBehaviour, IInteractable
{
    [SerializeField] private List<GameObject> lights = new List<GameObject>();

    [Networked] public NetworkBool IsPowerOn { get; set; } = true;

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        UpdateLightsVisual(IsPowerOn);
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(IsPowerOn))
            {
                UpdateLightsVisual(IsPowerOn);
            }
        }
    }

    private void UpdateLightsVisual(bool isOn)
    {
        foreach (GameObject lightObj in lights)
        {
            if (lightObj != null)
            {
                lightObj.SetActive(isOn);
            }
        }
    }
    
    public string GetInteractPrompt(NetworkBehaviour interactor)
    {
        if(interactor is RobberRole robber && IsPowerOn)
        {
            return "[E] Contect";
        }
        if(interactor is CopRole cop && !IsPowerOn)
        {
            return "[E] Contect";
        }
        return "";
    }

    public void Interact(NetworkBehaviour interactor)
    {
        if(interactor is RobberRole robber && IsPowerOn)
        {
            robber.StartFuseMiniGame(this);
        }
        else if(interactor is CopRole cop && !IsPowerOn)
        {
            cop.StartRestorePowerMiniGame(this);
        }
    }
}