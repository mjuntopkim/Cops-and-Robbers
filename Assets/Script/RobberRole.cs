using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class RobberRole : NetworkBehaviour
{
    [SerializeField] private float interactDistance = 5.0f;
    [SerializeField] private LayerMask itemLayer;

    [Networked] private NetworkBool IsCarry { get; set; }

    private StealItem _currentItem;
    private StolenItemBag _currentBag;
    private FuseBox _currentFuseBox;

    public bool IsPlayingMiniGame { get; private set; }
    public static RobberRole LocalRobber { get; private set; }

    public override void Spawned()
    {
        if(!HasInputAuthority)
        {
            UIManager.Instance.SetInteractUIActive("", false);
        }
        else
        {
            LocalRobber = this;
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
            StealItem item = hit.collider.GetComponent<StealItem>();
            StolenItemBag bag = hit.collider.GetComponent<StolenItemBag>();
            FuseBox fuse = hit.collider.GetComponent<FuseBox>();

            if(item != null && !item.IsStolen && !IsCarry)
            {
                _currentItem = item;
                _currentBag = null;
                _currentFuseBox = null;
                UIManager.Instance.SetInteractUIActive("[E] Take", true);
            }
            else if(bag != null && IsCarry)
            {
                _currentItem = null;
                _currentBag = bag;
                _currentFuseBox = null;
                UIManager.Instance.SetInteractUIActive("[E] Put", true);
            }
            else if(fuse != null && fuse.IsPowerOn)
            {
                _currentItem = null;
                _currentBag = null;
                _currentFuseBox = fuse;
                UIManager.Instance.SetInteractUIActive("[E] Context", true);
            }
            else
            {
                _currentItem = null;
                _currentBag = null;
                _currentFuseBox = null;
                UIManager.Instance.SetInteractUIActive("",false);
            }
        }
        else
        {
            _currentItem = null;
            _currentBag = null;
            _currentFuseBox = null;
            UIManager.Instance.SetInteractUIActive("",false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (data.button.IsSet(1))
            {
                if(_currentItem != null)
                {
                    TryStealItem();
                }
                else if(_currentBag != null)
                {
                    TryPutItem();
                }
                else if (_currentFuseBox != null)
                {
                    TryTurnOffBreaker();
                }
            }
        }
    }

    private void TryStealItem()
    {
        if (IsPlayingMiniGame || IsCarry || _currentItem == null || _currentItem.IsStolen)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, _currentItem.transform.position);
        if(distance > interactDistance)
        {
            return;
        }

        StartMiniGame(_currentItem);
    }

    private void StartMiniGame(StealItem targetItem)
    {
        IsPlayingMiniGame = true;

        MiniGameManager.Instance.PlayRandomMiniGame(
            onSuccess: () =>
            {
                IsPlayingMiniGame = false;
                StealSuccess(_currentItem);
                Debug.Log("미니게임 성공");
            },
            onFail: () =>
            {
                IsPlayingMiniGame = false;
                Debug.LogWarning("미니게임 실패");
            }
        );
    }

    private void StealSuccess(StealItem targetItem)
    {
        if (Object.HasStateAuthority)
        {
            targetItem.IsStolen = true;
            IsCarry = true;
        }
        else
        {
            RPC_Steal(targetItem);
        }
    }

    private void TryPutItem()
    {
        if(!IsCarry || _currentBag == null)
        {
            return;
        }

        if (Object.HasStateAuthority)
        {
            IsCarry = false;
            _currentBag.AddItem();
        }
        else
        {
            RPC_PutItem(_currentBag);
        }
    }

    public void Arrest(Vector3 prisonPoint)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (IsCarry)
        {
            IsCarry = false;
        }

        var ncc = GetComponent<NetworkCharacterController>();
        if(ncc != null)
        {
            ncc.Teleport(prisonPoint);
        }
    }

    private void TryTurnOffBreaker()
    {
        if (IsPlayingMiniGame || _currentFuseBox == null || !_currentFuseBox.IsPowerOn) return;

        float distance = Vector3.Distance(transform.position, _currentFuseBox.transform.position);
        if (distance > interactDistance) return;

        IsPlayingMiniGame = true;

        MiniGameManager.Instance.PlayRandomMiniGame(
            onSuccess: () =>
            {
                IsPlayingMiniGame = false;
                BreakerSuccess(_currentFuseBox);
            },
            onFail: () =>
            {
                IsPlayingMiniGame = false;
            }
        );
    }

    private void BreakerSuccess(FuseBox fuse)
    {
        if (Object.HasStateAuthority)
        {
            fuse.TurnOffPower();
        }
        else
        {
            RPC_TurnOffBreaker(fuse);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_TurnOffBreaker(FuseBox fuse)
    {
        if (fuse.IsPowerOn)
        {
            fuse.TurnOffPower();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Steal(StealItem targetItem)
    {
        if(!targetItem.IsStolen)
        {
            targetItem.IsStolen = true;
            IsCarry = true;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PutItem(StolenItemBag bag)
    {
        if (IsCarry)
        {
            IsCarry = false;
            bag.AddItem();
        }
    }
}
