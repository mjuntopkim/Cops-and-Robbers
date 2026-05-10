using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class CitizenAI : NetworkBehaviour
{
    private NavMeshAgent _agent;

    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 10f;
    private float _timer;

    public override void Spawned()
    {
        _agent = GetComponent<NavMeshAgent>();

        _timer = wanderTimer;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        _timer += Runner.DeltaTime;

        if(_timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
            _agent.SetDestination(newPos);
            _timer = 0;
        }
    }

    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}
