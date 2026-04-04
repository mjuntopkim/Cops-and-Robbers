using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StealItem : NetworkBehaviour
{
    [Networked] public NetworkBool IsStolen { get; set; }

    private ChangeDetector _changeDetector;
    private Renderer[] _renderer;
    private Collider _collider;
    
    private void Awake()
    {
        _renderer = GetComponentsInChildren<Renderer>();
        _collider = GetComponent<Collider>();
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        CheckItem();
    }

    public override void Render()
    {
        foreach(var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(IsStolen):
                    CheckItem();
                    break;
            }
        }
    }

    private void CheckItem()
    {
        bool isVisible = !IsStolen;
        if(_renderer != null)
        {
            foreach(Renderer r in _renderer)
            {
                if(r != null)
                {
                    r.enabled = isVisible;
                }
            }
        }
        if(_collider != null)
        {
            _collider.enabled = isVisible;
        }
    }
}
