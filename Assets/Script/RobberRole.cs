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

    public override void Spawned()
    {
        if(!HasInputAuthority)
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

        Vector3 rayPosition = transform.position + (Vector3.up * 1f);
        Vector3 rayDirection = Camera.main.transform.forward;

        Ray ray = new Ray(rayPosition, rayDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, itemLayer))
        {
            StealItem item = hit.collider.GetComponent<StealItem>();
            StolenItemBag bag = hit.collider.GetComponent<StolenItemBag>();

            if(item != null && !item.IsStolen && !IsCarry)
            {
                _currentItem = item;
                _currentBag = null;
                UIManager.Instance.SetInteractUIActive("[E] Take", true);
            }
            else if(bag != null && IsCarry)
            {
                _currentItem = null;
                _currentBag = bag;
                UIManager.Instance.SetInteractUIActive("[E] Put", true);
            }
            else
            {
                _currentItem = null;
                UIManager.Instance.SetInteractUIActive("",false);
            }
        }
        else
        {
            _currentItem = null;
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
            }
        }
    }

    private void TryStealItem()
    {
        if (IsCarry || _currentItem == null || _currentItem.IsStolen)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, _currentItem.transform.position);
        if(distance > interactDistance)
        {
            return;
        }

        //StratMiniGame(_currentItem)
        StealSuccess(_currentItem);
    }

    private void StartMiniGame(StealItem targetItem)
    {
        //미니게임 성공시 StealSuccess();
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
