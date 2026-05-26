using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CitizenMoveState : CitizenBaseState
{
    public CitizenMoveState(CitizenBaseState initialSubState)
    {
        _currentSubState = initialSubState;
    }

    public override void EnterState(CitizenAI ai)
    {
        ai.Agent.isStopped = false;
        base.EnterState(ai);
    }

    public override void UpdateState(CitizenAI ai, float dt)
    {
        if (!ai.Agent.pathPending)
        {
            if(ai.Agent.pathStatus == NavMeshPathStatus.PathInvalid || ai.Agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                ai.ChangeState(CitizenAI.CitizenState.Idle);
                return;
            }
        }

        base.UpdateState(ai, dt);
    }
}
