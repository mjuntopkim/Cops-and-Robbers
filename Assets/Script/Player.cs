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

        foreach (var player in allLobbyPlayers)
        {
            if (player.Object.InputAuthority == Object.InputAuthority)
            {
                if (player.Role == PlayerRole.Robber)
                {
                    _cc.maxSpeed = 6.0f;
                }
                else if (player.Role == PlayerRole.Cop)
                {
                    _cc.maxSpeed = 10.0f;
                }
                break;
            }
        }

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
            if (HasStateAuthority)
            {
                AnimSpeed = data.direction.magnitude;
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
