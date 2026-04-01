using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [SerializeField] private NetworkPrefabRef _playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private Transform[] _copSpawnPoint;
    [SerializeField] private Transform[] _robberSpawnPoint;

    private int _copSpawnCount = 0;
    private int _robberSpawnCount = 0;

    public void Start()
    {
        var _runner = FindObjectOfType<NetworkRunner>();
        if(_runner != null)
        {
            _runner.AddCallbacks(this);
        }
    }

    public void OnDestroy()
    {
        var _runner = FindObjectOfType<NetworkRunner>();
        if(_runner != null)
        {
            _runner.RemoveCallbacks(this);
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        data.direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        data.direction.Normalize();

        if (Camera.main != null)
        {
            data.cameraYaw = Camera.main.transform.eulerAngles.y;
        }

        data.button.Set(0, Input.GetKey(KeyCode.Space));

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            PlayerRole assignedRole;
            if (player == runner.LocalPlayer)
            {
                assignedRole = PlayerRole.Cop;
            }
            else
            {
                assignedRole = PlayerRole.Robber;
            }

            Vector3 spawnPosition = GetSpawnPosition(assignedRole);

            NetworkObject networkPlayerObject = runner.Spawn(
                _playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player,
                (runner, obj) =>
                {
                    Player playerComponent = obj.GetComponent<Player>();
                    if (playerComponent != null)
                    {
                        playerComponent.Role = assignedRole;
                    }
                }
            );

            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if(_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    private Vector3 GetSpawnPosition(PlayerRole role)
    {
        if(role == PlayerRole.Cop)
        {
            Vector3 pos = _copSpawnPoint[_copSpawnCount % _copSpawnPoint.Length].position;
            _copSpawnCount++;
            return pos;
        }
        else
        {
            Vector3 pos = _robberSpawnPoint[_robberSpawnCount % _robberSpawnPoint.Length].position;
            _robberSpawnCount++;
            return pos;
        }
    }
}
