using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenIdleState : CitizenBaseState
{
    private float _timer;
    public override void EnterState(CitizenAI ai)
    {
        ai.Agent.isStopped = true;
        _timer = 0f;

    }

    public override void UpdateState(CitizenAI ai, float dt)
    {
        _timer += dt;
        if(_timer >= ai.IdleWaitTime)
        {
            ai.ChangeState(CitizenAI.CitizenState.Patrol);
        }
    }

    public override void ExitState(CitizenAI ai)
    {
        return;
    }
}
