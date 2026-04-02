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

    [SerializeField] private NetworkPrefabRef _copPrefab;
    [SerializeField] private NetworkPrefabRef _robberPrefab1;
    [SerializeField] private NetworkPrefabRef _robberPrefab2;
    [SerializeField] private NetworkPrefabRef _robberPrefab3;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private Transform[] _copSpawnPoint;
    [SerializeField] private Transform[] _robberSpawnPoint;

    private int _copSpawnCount = 0;
    private int _robberSpawnCount = 0;

    public void Awake()
    {
        _runner = FindObjectOfType<NetworkRunner>();
        if(_runner != null)
        {
            _runner.AddCallbacks(this);
        }
    }

    public void OnDestroy()
    {
        _runner = FindObjectOfType<NetworkRunner>();
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
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
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
    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        if (runner.IsServer)
        {
            LobbyPlayer[] allLobbyPlayers = FindObjectsOfType<LobbyPlayer>();

            Debug.Log($"<color=yellow>씬 2 로드 완료! 찾은 로비 플레이어 수: {allLobbyPlayers.Length}명</color>");
            
            foreach (var lobbyPlayer in allLobbyPlayers)
            {
                PlayerRef playerRef = lobbyPlayer.Object.InputAuthority;
                Vector3 spawnPosition = GetSpawnPosition(lobbyPlayer.Role);
                NetworkPrefabRef prefabToSpawn = _copPrefab;

                if(lobbyPlayer.Role == PlayerRole.Cop)
                {
                    prefabToSpawn = _copPrefab;
                }
                else if(lobbyPlayer.Role == PlayerRole.Robber)
                {
                    int randomRobber = UnityEngine.Random.Range(0, 3);
                    if(randomRobber == 0)
                    {
                        prefabToSpawn = _robberPrefab1;
                    }
                    else if(randomRobber == 1)
                    {
                        prefabToSpawn = _robberPrefab2;
                    }
                    else
                    {
                        prefabToSpawn = _robberPrefab3;
                    }
                }

                NetworkObject spawnObj = runner.Spawn(prefabToSpawn, spawnPosition, Quaternion.identity, playerRef);
                _spawnedCharacters.Add(playerRef, spawnObj);
            }
        }
    }
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
