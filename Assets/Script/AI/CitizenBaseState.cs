using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CitizenBaseState
{
    protected CitizenBaseState _currentSubState;

    public virtual void EnterState(CitizenAI ai)
    {
        if(_currentSubState != null)
        {
            _currentSubState.EnterState(ai);
        }
    }
    public virtual void UpdateState(CitizenAI ai, float dt)
    {
        if (_currentSubState != null)
        {
            _currentSubState.UpdateState(ai, dt);
        }
    }
    public virtual void ExitState(CitizenAI ai)
    {
        return;
    }

    protected void SetSubState(CitizenAI ai, CitizenBaseState subState)
    {
        if(_currentSubState != null)
        {
            _currentSubState.ExitState(ai);
        }

        _currentSubState = subState;

        if (_currentSubState != null)
        {
            _currentSubState.EnterState(ai);
        }
    }
}
