using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class CitizenAI : NetworkBehaviour
{
    public enum CitizenState
    {
        Idle,
        Patrol,
        Flee
    }

    private NavMeshAgent _agent;
    private Animator _animator;

    [Networked] public CitizenState CurrentState { get; set; }

    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float runSpeed = 10.0f;
    [SerializeField] private float idleWaitTime = 3.0f;
    [SerializeField] private float fleeWaitTime = 1.0f;
    [SerializeField] private float fleeDuration = 10.0f;
    [SerializeField] private float wanderRadius = 10.0f;
    
    private float _stateTimer;
    private float _fleeTotalTimer;

    private Vector3 _lastVisualPos;

    public override void Spawned()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _animator.SetFloat("MotionSpeed", 4.0f);

        _lastVisualPos = transform.position;

        ChangeState(CitizenState.Idle);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        float dt = Runner.DeltaTime;

        if (_agent.desiredVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
        }

        switch (CurrentState)
        {
            case CitizenState.Idle:
                UpdateIdle(dt);
                break;
            case CitizenState.Patrol:
                UpdatePatrol(dt);
                break;
            case CitizenState.Flee:
                UpdateFlee(dt);
                break;
        }
    }

    public override void Render()
    {
        float moveDist = Vector3.Distance(transform.position, _lastVisualPos);
        _lastVisualPos = transform.position;

        if (moveDist > 0.001f)
        {
            switch (CurrentState)
            {
                case CitizenState.Idle:
                    _animator.SetFloat("Speed", 0.0f);
                    break;
                case CitizenState.Patrol:
                    _animator.SetFloat("Speed", 6.0f);
                    break;
                case CitizenState.Flee:
                    _animator.SetFloat("Speed", 10.0f);
                    break;
            }
        }
        else
        {
            _animator.SetFloat("Speed", 0.0f);
        }
    }

    private void ChangeState(CitizenState state)
    {
        CurrentState = state;
        _stateTimer = 0f;

        switch (CurrentState)
        {
            case CitizenState.Idle:
                _agent.isStopped = true;
                break;
            case CitizenState.Patrol:
                _agent.speed = walkSpeed;
                _agent.isStopped = false;
                SetRandomDestination();
                break;
            case CitizenState.Flee:
                _agent.speed = runSpeed;
                _agent.isStopped = false;
                _fleeTotalTimer = 0f;
                SetRandomDestination();
                break;
        }
    }

    private void UpdateIdle(float dt)
    {
        _stateTimer += dt;

        if(_stateTimer >= idleWaitTime)
        {
            ChangeState(CitizenState.Patrol);
        }
    }

    private void UpdatePatrol(float dt)
    {
        if(!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            ChangeState(CitizenState.Idle);
        }
    }

    private void UpdateFlee(float dt)
    {
        _fleeTotalTimer += dt;

        if(_fleeTotalTimer >= fleeDuration)
        {
            ChangeState(CitizenState.Idle);
            return;
        }

        if(!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _stateTimer += dt;
            if(_stateTimer >= fleeWaitTime)
            {
                SetRandomDestination();
                _stateTimer = 0f;
            }
        }
    }

    public void TriggerAlarm()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if(CurrentState != CitizenState.Flee)
        {
            ChangeState(CitizenState.Flee);
        }
    }

    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }
}
