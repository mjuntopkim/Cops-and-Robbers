using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class FuseBox : NetworkBehaviour
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

        Debug.Log(isOn ? "전력 복구." : "정전");
    }

    public void TurnOffPower()
    {
        if (Object.HasStateAuthority)
        {
            IsPowerOn = false;
        }
    }

    public void TurnOnPower()
    {
        if (Object.HasStateAuthority)
        {
            IsPowerOn = true;
        }
    }
}