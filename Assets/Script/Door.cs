using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Door : NetworkBehaviour
{
    [Networked] public NetworkBool IsOpen { get; set; }
    private ChangeDetector _changeDetector;

    [SerializeField] private Transform doorMesh; 
    [SerializeField] private Vector3 openOffset = new Vector3(1.5f, 0, 0); 
    [SerializeField] private float openSpeed = 5f;

    private Vector3 _closedPos;
    private Vector3 _targetPos;

    public override void Spawned()
    {
        _closedPos = doorMesh.localPosition;
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        UpdateVisuals(); 
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(IsOpen)) UpdateVisuals();
        }

        doorMesh.localPosition = Vector3.Lerp(doorMesh.localPosition, _targetPos, Time.deltaTime * openSpeed);
    }

    private void UpdateVisuals()
    {
        _targetPos = IsOpen ? _closedPos + openOffset : _closedPos;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleDoor()
    {
        IsOpen = !IsOpen;
    }
}
