using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : NetworkBehaviour      
{
    [SerializeField] private CinemachineFreeLook _freeLookCamera;
    [Networked] private float AnimSpeed { get; set; }
    private TickTimer _catchCooldown { get; set; }

    private ChangeDetector _change;
    private Renderer _renderer;
    private Animator _animator;

    private NetworkCharacterController _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>(); 
        _renderer = GetComponentInChildren<Renderer>();
        _animator = GetComponentInChildren<Animator>();
    }

    public override void Spawned()
    {
        LobbyPlayer[] allLobbyPlayers = FindObjectsOfType<LobbyPlayer>();

        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (_freeLookCamera != null)
        {
            _freeLookCamera.gameObject.SetActive(HasInputAuthority);
        }
    }

    public override void Render()
    {
        if (_animator != null)
        {
            _animator.SetFloat("Speed", AnimSpeed);
            _animator.SetFloat("MotionSpeed", 4.0f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            float targetSpeed = 6.0f;

            if (data.button.IsSet(3))
            {
                targetSpeed = 10.0f;
            }

            _cc.maxSpeed = targetSpeed;

            if (HasStateAuthority)
            {
                if(data.direction.magnitude > 0)
                {
                    AnimSpeed = targetSpeed;
                }
                else
                {
                    AnimSpeed = 0.0f;
                }
            }

            Vector3 moveDirection = Quaternion.Euler(0, data.cameraYaw, 0) * data.direction;
            moveDirection.Normalize();

            if (moveDirection.sqrMagnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * 20f);
            }

            _cc.Move(moveDirection);
        }
    }
}
