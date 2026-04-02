using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum PlayerRole { Cop, Robber}

public class LobbyPlayer : NetworkBehaviour
{
    [Networked] public PlayerRole Role { get; set; }
    [Networked] public NetworkBool IsReady { get; set; }
    [Networked] public NetworkBool IsHost { get; set; }

    private ChangeDetector _changeDetector;
    public override void Spawned()
    {
        DontDestroyOnLoad(this.gameObject);
            
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if(LobbyManager.Instance != null)
        {
            LobbyManager.Instance.AddPlayerUI(this);
        }
    }

    public override void Render()
    {
        foreach(var p in _changeDetector.DetectChanges(this))
        {
            switch (p)
            {
                case nameof(Role):
                    if(LobbyManager.Instance != null)
                    {
                        LobbyManager.Instance.UpdatePlayerUIPosition(this);
                    }
                    break;

                case nameof(IsReady):
                    if(LobbyManager.Instance != null)
                    {
                        LobbyManager.Instance.UpdatePlayerUIInfo(this);
                    }
                    break;
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if(LobbyManager.Instance != null)
        {
            LobbyManager.Instance.RemovePlayerUI(this);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SetRole(PlayerRole role)
    {
        if (IsReady)
        {
            return;
        }
        Role = role;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_Ready(NetworkBool ready)
    {
        IsReady = ready;
    }
}
