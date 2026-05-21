using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CitizenBaseState
{
    public abstract void EnterState(CitizenAI ai);
    public abstract void UpdateState(CitizenAI ai, float dt);
    public abstract void ExitState(CitizenAI ai);
}
