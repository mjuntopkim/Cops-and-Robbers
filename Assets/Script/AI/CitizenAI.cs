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
    
    private int _searchCount = 5;

    private Vector3 _lastVisualPos;

    private CitizenBaseState _currentStateObj; 
    private readonly CitizenIdleState _idleState = new CitizenIdleState(); 
    private readonly CitizenPatrolState _patrolState = new CitizenPatrolState(); 
    private readonly CitizenFleeState _fleeState = new CitizenFleeState(); 

    public NavMeshAgent Agent => _agent; 
    public float WalkSpeed => walkSpeed; 
    public float RunSpeed => runSpeed; 
    public float IdleWaitTime => idleWaitTime; 
    public float FleeWaitTime => fleeWaitTime; 
    public float FleeDuration => fleeDuration;

    public override void Spawned()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _animator.SetFloat("MotionSpeed", 4.0f);

        _lastVisualPos = transform.position;

        if(AIManager.Instance != null)
        {
            AIManager.Instance.ActiveCitizens.Add(this);
        }

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

        if(_currentStateObj != null)
        {
            _currentStateObj.UpdateState(this, dt);
        }
    }

    public override void Render()
    {
        float moveDist = Vector3.Distance(transform.position, _lastVisualPos);
        _lastVisualPos = transform.position;

        if (moveDist > 0.001f)
        {
            float speed = moveDist / Time.deltaTime;
            _animator.SetFloat("Speed", speed);
        }
        else
        {
            _animator.SetFloat("Speed", 0.0f);
        }
    }

    public void ChangeState(CitizenState state)
    {
        CurrentState = state;

        if(_currentStateObj != null)
        {
            _currentStateObj.ExitState(this);
        }

        switch (CurrentState)
        {
            case CitizenState.Idle:
                _currentStateObj = _idleState;
                break;
            case CitizenState.Patrol:
                _currentStateObj = _patrolState;
                break;
            case CitizenState.Flee:
                _currentStateObj = _fleeState;
                break;
        }

        if(_currentStateObj != null)
        {
            _currentStateObj.EnterState(this);
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

    public void SetRandomDestination()
    {
        NavMeshHit hit;

        for(int i = 0; i < _searchCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))   
            {                                                                                       
                _agent.SetDestination(hit.position);                                               
                return;
            }
        }

        ChangeState(CitizenState.Idle);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if(AIManager.Instance != null)
        {
            AIManager.Instance.ActiveCitizens.Remove(this);
        }
    }
}
