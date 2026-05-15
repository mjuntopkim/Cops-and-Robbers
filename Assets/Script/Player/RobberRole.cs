using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class RobberRole : NetworkBehaviour
{
    [SerializeField] private float interactDistance = 5.0f;
    [SerializeField] private LayerMask itemLayer;

    [Networked] public NetworkBool IsCarry { get; private set; }
    [Networked] private StealItem CarriedItem { get; set; }

    private IInteractable _currentInteractable;

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
        if (GetInput(out NetworkInputData data))
        {
            if (data.button.IsSet(1))
            {
                if(_currentInteractable != null)
                {
                    _currentInteractable.Interact(this);
                    _currentInteractable = null;
                }
            }
        }
    }

    public void StartStealMiniGame(StealItem targetItem)
    {
        IsPlayingMiniGame = true;
        MiniGameManager.Instance.PlayRandomMiniGame(
            onSuccess: () =>
            {
                IsPlayingMiniGame = false;
                RPC_StealItem(targetItem);
            },
            onFail: () =>
            {
                IsPlayingMiniGame = false;
                RPC_BroadcastFailAlarm();
            });
    }

    public void PutItemToBag(StolenItemBag bag)
    {
        RPC_PutItem(bag);
    }

    public void StartDoorMiniGame(Door targetDoor)
    {
        IsPlayingMiniGame = true;
        MiniGameManager.Instance.PlayRandomMiniGame(
            onSuccess: () =>
            {
                IsPlayingMiniGame = false;
                RPC_ToggleDoor(targetDoor);
            },
            onFail: () =>
            {
                IsPlayingMiniGame = false;
                RPC_BroadcastFailAlarm();
            });
    }

    public void StartFuseMiniGame(FuseBox fuse)
    {
        IsPlayingMiniGame = true;
        MiniGameManager.Instance.PlayRandomMiniGame(
            onSuccess: () =>
            {
                IsPlayingMiniGame = false;
                RPC_SetPower(fuse, false);
            },
            onFail: () =>
            {
                IsPlayingMiniGame = false;
                RPC_BroadcastFailAlarm();
            });
    }

    public void Arrest(Vector3 prisonPoint)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (IsCarry)
        {
            if(CarriedItem != null)
            {
                CarriedItem.IsStolen = false;
                CarriedItem = null;
            }
            IsCarry = false;
        }

        RPC_CancelMiniGame();

        var ncc = GetComponent<NetworkCharacterController>();
        if(ncc != null)
        {
            ncc.Teleport(prisonPoint);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_StealItem(StealItem targetItem)
    {
        if(targetItem != null && !targetItem.IsStolen)
        {
            targetItem.IsStolen = true;
            IsCarry = true;

            CarriedItem = targetItem;
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
    public void RPC_PutItem(StolenItemBag bag)
    {
        if(IsCarry && bag != null)
        {
            IsCarry = false;
            CarriedItem = null;
            bag.TotalStolenCount++;
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_BroadcastFailAlarm()
    {
        if (CopRole.LocalCop != null)
        {
            CitizenAI[] allCitizens = FindObjectsOfType<CitizenAI>();
            UIManager.Instance.ShowGlobalAlarm("<color=red>Someone triggered an alarm !!</color>", 3.0f);
            foreach (var citizen in allCitizens)
            {
                citizen.TriggerAlarm();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_CancelMiniGame()
    {
        if (IsPlayingMiniGame)
        {
            IsPlayingMiniGame = false;

            MiniGameManager.Instance.CancelCurrentMiniGame();
        }
    }
}
