using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : NetworkBehaviour      
{
    [SerializeField] private CinemachineFreeLook _freeLookCamera;
    [SerializeField] private Transform _cameraRoot;

    [Networked] private float AnimSpeed { get; set; }               

    private ChangeDetector _change;
    private Renderer _renderer;
    private Animator _animator;
    private PlayerRoles _role;

    private Vector3 _spawnPoint;
    private Color PlayerColor;

    private NetworkCharacterController _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>(); 
        _renderer = GetComponentInChildren<Renderer>();
        _animator = GetComponentInChildren<Animator>();
        _role = GetComponent<PlayerRoles>();
        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Spawned()
    {
        if(_role.Role == PlayerRole.Robber)
        {
            _cc.maxSpeed = 6.0f;
        }
        else if(_role.Role == PlayerRole.Cop)
        {
            _cc.maxSpeed = 4.0f;
        }
    }

    /*public override void Render()   //눈에 보이는 것은 Render()나 Update()에서
    {
        foreach(var change in _change.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(CurrentHp):
                    healthBar.UpdateHealth(CurrentHp, MaxHp);
                    break;

                case nameof(IsDead):
                    UpdateColor();
                    break;

                case nameof(Score):
                    PlayerScoreEffect();
                    break;

                case nameof(CanInput):
                    UpdateColor();
                    break;
            }
        }

        if (_animator != null)
        {
            _animator.SetFloat("Speed", AnimSpeed);
            _animator.SetFloat("MotionSpeed", 4.0f);

            bool isGrounded = _cc.Grounded;
            _animator.SetBool("Grounded", isGrounded);

            _animator.SetBool("FreeFall", !isGrounded && _cc.Velocity.y < 0);

            if(!isGrounded && _cc.Velocity.y > 2)   //_cc.Velocity.y > 0 으로인해 0.001 같은 작은 수로도 점프 애니메이션이 작동함(평지에서도)
            {                                       //_cc.Velocity.y > 0 유니티 기본엔진은 지터값 무시, 퓨전은 무시 안함 매우 예민
                _animator.SetBool("Jump", true);
            }
            else if (isGrounded)
            {
                _animator.SetBool("Jump", false);
            }
        }
    }*/

    public override void FixedUpdateNetwork()   //로직 계산은 여기서
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector3 moveDirection = Quaternion.Euler(0, data.cameraYaw, 0) * new Vector3(data.direction.x, 0, data.direction.y);
            moveDirection.Normalize();

            if(moveDirection.sqrMagnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * 10f);
            }

            _cc.Move(moveDirection);

            if(data.button.Equals(0))
            {
                _cc.Jump();
            }
        }
    }

    private void UpdateColor()
    {
        if(_renderer != null)
        {

        }
    }
}
