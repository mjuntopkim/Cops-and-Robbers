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

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        data.direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
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

    async void StartGame(GameMode mode)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();

        // _runner가 로컬 사용자의 입력을 받을 것 인지 설정, true로 해야 키보드로 입력한 정보가 서버로 전송됨
        _runner.ProvideInput = true;

        // 현재 열려있는 씬의 번호(index)를 가져옴
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        // 가져온 씬이 유효하면 네트워크 씬 정보에 등록 (나중에 접속하는 사람들에게 알려주기 위함)
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // 이 설정으로 게임 접속 시도, 될 때까지 기다림
        await _runner.StartGame(new StartGameArgs()
        {
            // 내가 호스트인지 클라이언트인지
            GameMode = mode,

            //방 이름
            SessionName = "TestRoom",

            // 위에서 저장한 씬 정보
            Scene = scene,

            // Fusion이 씬을 로드할때 이 컴포넌트에 씬을 불러오라고 시킴
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

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

    private void OnGUI()
    {
        if(_runner == null)
        {
            if (GUI.Button(new Rect(10, 10, 200, 40), "방 생성"))
            {
                StartGame(GameMode.Host);
            }

            if (GUI.Button(new Rect(10, 60, 200, 40), "방 참가"))
            {
                StartGame(GameMode.Client);
            }
        }
    }
}
