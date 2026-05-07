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
        //퓨전은 네트워크 오브젝트를 씬 전환이 일어나고 현재 씬에 소속된 오브젝트 목록을 정리하고 이전 오브젝트 디스폰, 새 씬에 네트워크 오브젝트들 등록, runner.spawn으로 생성된것은 현재 씬 오브젝트로 설정됨
        //runner.spawn으로 생성한것을 DontDestroyOnLoad 하면 유니티는 씬에 상관없다 생각하고 퓨전은 로비 씬의 오브젝트라고 생각

        //DontDestroyOnLoad(this.gameObject); //이것을 유니티에서 관리하는게 아닌 퓨전에서 관리하도록 해야한다. 
            
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
