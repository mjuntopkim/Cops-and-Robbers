using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CitizenPatrolState : CitizenBaseState
{
    private float _patrolTimer;
    public override void EnterState(CitizenAI ai)
    {
        ai.Agent.speed = ai.WalkSpeed;
        ai.Agent.isStopped = false;
        ai.SetRandomDestination();
        _patrolTimer = 0f;
    }

    public override void UpdateState(CitizenAI ai, float dt)
    {
        _patrolTimer += dt;

        if(_patrolTimer >= 10f)
        {
            ai.ChangeState(CitizenAI.CitizenState.Idle);
            return;
        }

        if (!ai.Agent.pathPending)
        {
            if(ai.Agent.pathStatus == NavMeshPathStatus.PathInvalid || ai.Agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                ai.ChangeState(CitizenAI.CitizenState.Idle);
                return;
            }
            if(ai.Agent.remainingDistance <= ai.Agent.stoppingDistance)
            {
                ai.ChangeState(CitizenAI.CitizenState.Idle);
            }
        }
    }

    public override void ExitState(CitizenAI ai)
    {
        return;
    }
}
