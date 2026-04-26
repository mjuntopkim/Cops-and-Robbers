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

    [Networked] public int CopIndex { get; set; }

    public static CopRole LocalCop { get; private set; }
    public bool IsPlayingMiniGame { get; private set; }

    private FuseBox _currentFuseBox;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            LocalCop = this;
        }
        else
        {
            UIManager.Instance.SetInteractUIActive("", false);
        }
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

        Vector3 rayPosition = transform.position + (Vector3.up * 1f);
        Vector3 rayDirection = Camera.main.transform.forward;

        Ray ray = new Ray(rayPosition, rayDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, itemLayer))
        {
            FuseBox breaker = hit.collider.GetComponentInParent<FuseBox>();

            // 차단기가 존재하고, 꺼져있을 때만 UI 표시
            if (breaker != null && !breaker.IsPowerOn)
            {
                _currentFuseBox = breaker;
                UIManager.Instance.SetInteractUIActive("[E] Contect", true);
            }
            else
            {
                _currentFuseBox = null;
                UIManager.Instance.SetInteractUIActive("", false);
            }
        }
        else
        {
            _currentFuseBox = null;
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
                if (_currentFuseBox != null)
                {
                    TryRestorePower();
                }
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

    private void TryRestorePower()
    {
        if (IsPlayingMiniGame || _currentFuseBox == null || _currentFuseBox.IsPowerOn) return;

        float distance = Vector3.Distance(transform.position, _currentFuseBox.transform.position);
        if (distance > interactDistance) return;

        IsPlayingMiniGame = true; 

        MiniGameManager.Instance.PlayRandomMiniGame(
            onSuccess: () =>
            {
                IsPlayingMiniGame = false; 
                PowerRestoreSuccess(_currentFuseBox);
                Debug.Log("전력 복구 성공");
            },
            onFail: () =>
            {
                IsPlayingMiniGame = false;
                Debug.LogWarning("전력 복구 실패");
            }
        );
    }

    private void PowerRestoreSuccess(FuseBox fuse)
    {
        if (Object.HasStateAuthority)
        {
            fuse.TurnOnPower();
        }
        else
        {
            RPC_RequestTurnOn(fuse);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestTurnOn(FuseBox fuse)
    {
        if (!fuse.IsPowerOn)
        {
            fuse.TurnOnPower();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestArrest(RobberRole robber)
    {
        if(robber != null)
        {
            float distance = Vector3.Distance(transform.position, robber.transform.position);
            if(distance <= attackDistance)
            {
                ExecuteArrest(robber);
            }
        }
    }
}
