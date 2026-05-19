using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CopRole : NetworkBehaviour
{
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask targetLayer;

    [SerializeField] private float interactDistance = 5.0f;
    [SerializeField] private LayerMask itemLayer;

    [SerializeField] private GameObject flashlightObject;

    [Networked] public int CopIndex { get; set; }
    [Networked] public NetworkBool IsFlashlightOn { get; set; }

    public static CopRole LocalCop { get; private set; }
    public bool IsPlayingMiniGame { get; private set; }

    private IInteractable _currentInteractable;
    private ChangeDetector _changeDetector;
    private Camera _mainCamera;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasInputAuthority)
        {
            LocalCop = this;
        }
        else
        {
            UIManager.Instance.SetInteractUIActive("", false);
        }

        if (flashlightObject != null)
        {
            flashlightObject.SetActive(IsFlashlightOn);
        }

        _mainCamera = Camera.main;
    }

    public override void Render()
    {
        if (!HasInputAuthority)
        {
            return;
        }

        if (IsPlayingMiniGame)
        {
            UIManager.Instance.SetInteractUIActive("", false);
            return;
        }

        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(IsFlashlightOn))
            {
                if (flashlightObject != null)
                {
                    flashlightObject.SetActive(IsFlashlightOn);
                }
            }
        }

        Vector3 rayPosition = transform.position + (Vector3.up * 1f);
        Vector3 rayDirection = _mainCamera.transform.forward; 

        Ray ray = new Ray(rayPosition, rayDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, itemLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                _currentInteractable = interactable;
                string prompt = _currentInteractable.GetInteractPrompt(this);

                UIManager.Instance.SetInteractUIActive(prompt, true);
            }
            else
            {
                _currentInteractable = null;
                UIManager.Instance.SetInteractUIActive("", false);
            }
        }
        else
        {
            _currentInteractable = null;
            UIManager.Instance.SetInteractUIActive("", false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
            if (data.button.IsSet(2))
            {
                TryAttack();
            }

            if (data.button.IsSet(1))
            {
                if (_currentInteractable != null)
                {
                    _currentInteractable.Interact(this);
                    _currentInteractable = null;
                }
            }

            if (data.button.IsSet(4))
            {
                RPC_ToggleFlashlight();
            }
        }
    }

    private void TryAttack()
    {
        Vector3 position = transform.position + (Vector3.up * 1f);
        Vector3 direction = transform.forward;

        if(Physics.SphereCast(position, attackRadius, direction, out RaycastHit hit, attackDistance, targetLayer))
        {
            RobberRole robber = hit.collider.GetComponentInParent<RobberRole>();
            
            if(robber != null)
            {
                if (Object.HasStateAuthority)
                {
                    ExecuteArrest(robber);
                }
                else
                {
                    RPC_RequestArrest(robber);
                }
            }
        }
    }

    private void ExecuteArrest(RobberRole robber)
    {
        Vector3 prisonPos = PrisonManager.Instance.GetPrisonPosition(CopIndex);

        robber.Arrest(prisonPos);
    }

    public void StartRestorePowerMiniGame(FuseBox fuse)
    {
        IsPlayingMiniGame = true;

        MiniGameManager.Instance.PlayRandomMiniGame(
            onSuccess: () =>
            {
                IsPlayingMiniGame = false;
                RPC_SetPower(fuse, true);
            },
            onFail: () =>
            {
                IsPlayingMiniGame = false;
            });
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestArrest(RobberRole robber)
    {
        if(robber != null)
        {
            float distance = Vector3.Distance(transform.position, robber.transform.position);   //transform.position은 클라에서 계산, 메모리 변조 시 알 방법이 없다. 
            if(distance <= attackDistance)                                                      //서버에서 계산하도록 하는방법이 핵 방지엔 좋지만 서버 비용이 많이 나올 수 있다. 
            {
                ExecuteArrest(robber);
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ToggleDoor(Door targetDoor)
    {
        if(targetDoor != null)
        {
            targetDoor.IsOpen = !targetDoor.IsOpen;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPower(FuseBox fuse, NetworkBool isOn)
    {
        if(fuse != null)
        {
            fuse.IsPowerOn = isOn;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ToggleFlashlight()
    {
        IsFlashlightOn = !IsFlashlightOn;
    }
}
