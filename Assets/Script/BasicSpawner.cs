using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;

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
        runner = FindObjectOfType<NetworkRunner>();
        if(runner != null)
        {
            runner.AddCallbacks(this);
        }
    }

    public void OnDestroy()
    {
        runner = FindObjectOfType<NetworkRunner>();
        if(runner != null)
        {
            runner.RemoveCallbacks(this);
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

        data.button.Set(1, Input.GetKey(KeyCode.E));

        data.button.Set(2, Input.GetKey(KeyCode.Mouse0));

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
            //LobbyPlayer[] allLobbyPlayers = FindObjectsOfType<LobbyPlayer>();   //잘 되지만 / 퓨전 버전마다 다르다 - 퓨전이 자체적으로 관리하는 네트워크 오브젝트와 DontDestroyOnLoad오브젝트가 서로 충돌이 날 수 있다.
                                                                                //그냥 퓨전 API를 사용 runner.ActivePlayers 사용 여기서 로비 플레이어 정보를 가져와서 사용
                                                                                //FindObjectsOfType 유니티월드에서 사용 퓨전이랑 충돌날 수 있다.

            foreach (PlayerRef playerRef in runner.ActivePlayers)
            {
                var playerRole = PlayerRole.Cop;
                var prefabToSpawn = _copPrefab;

                if (LobbyManager.LobbyRole.TryGetValue(playerRef, out PlayerRole savedRole)){
                    playerRole = savedRole;
                }

                var spawnPosition = GetSpawnPosition(playerRole);

                if(playerRole == PlayerRole.Cop)
                {
                    prefabToSpawn = _copPrefab;
                }
                else if(playerRole == PlayerRole.Robber)
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

                //로비 플레이 해제하는 코드가 없다. 메모리를 계속 잡아먹는다. 메모리 낭비 및 네트워크도 잡아먹는다. 그리고 체인지 디텍터로 계속 돌아간다.
                //아무것도 안하는데 계속 돌아가서 메모리, 네트워크 낭비
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
