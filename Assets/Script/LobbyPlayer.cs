using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum PlayerRole { Cop, Robber}

public class LobbyPlayer : NetworkBehaviour
{
    [Networked] public PlayerRole Role { get; set; }
    [Networked] public NetworkBool IsReady { get; set; }

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
