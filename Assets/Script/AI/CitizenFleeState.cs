using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenFleeState : CitizenBaseState
{
    private float _fleeTotalTimer;
    private float _waitTimer;

    public override void EnterState(CitizenAI ai)
    {
        ai.Agent.speed = ai.RunSpeed;
        ai.Agent.isStopped = false;
        ai.SetRandomDestination();
        _fleeTotalTimer = 0f;
        _waitTimer = 0f;
    }

    public override void UpdateState(CitizenAI ai, float dt)
    {
        _fleeTotalTimer += dt;

        if(_fleeTotalTimer >= ai.FleeDuration)
        {
            ai.ChangeState(CitizenAI.CitizenState.Idle);
            return;
        }

        if(!ai.Agent.pathPending && ai.Agent.remainingDistance <= ai.Agent.stoppingDistance)
        {
            _waitTimer += dt;

            if(_waitTimer >= ai.FleeWaitTime)
            {
                ai.SetRandomDestination();
                _waitTimer = 0f;
            }
        }
    }

    public override void ExitState(CitizenAI ai)
    {
        return;
    }
}
