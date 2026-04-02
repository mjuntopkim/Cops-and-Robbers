using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject hostPanael;
    [SerializeField] private TMP_InputField roomNameInputField;

    [SerializeField] private Transform sessionListCenter;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private Button joinButton;

    [SerializeField]private NetworkRunner networkRunnerPrefab;

    private NetworkRunner _runner;
    private string _selectedRoomname = "";

    public void OnClickExitButton()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    public void OnClickhostButton()
    {
        mainPanel.gameObject.SetActive(false);
        hostPanael.gameObject.SetActive(true);
    }

    public async void OnClickCreateRoomButton()
    {
        string newName = roomNameInputField.text;

        _runner = Instantiate(networkRunnerPrefab);
        _runner.name = "Network Runner";
        _runner.AddCallbacks(this);

        var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var customProps = new Dictionary<string, SessionProperty>();
        customProps.Add("RoomTitle", newName);

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = Guid.NewGuid().ToString(),
            SessionProperties = customProps,
            PlayerCount = 6,
            IsVisible = true,
            IsOpen = true,
            Scene = SceneRef.FromIndex(1),
            SceneManager = sceneManager
        };

        await _runner.StartGame(startGameArgs);
    }

    public async void OnClickJoinMenuButton()
    {
        mainPanel.SetActive(false);
        joinPanel.SetActive(true);
        joinButton.interactable = false;

        if (_runner == null)
        {
            _runner = Instantiate(networkRunnerPrefab);
            _runner.AddCallbacks(this);
        }

        if (!_runner.LobbyInfo.IsValid)
        {
            await _runner.JoinSessionLobby(SessionLobby.ClientServer);
        }
    }

    /*public async void OnClickQuickJoinButton()
    {

    }*/

    public void OnClickBackButton()
    {
        joinPanel.SetActive(false);
        mainPanel.SetActive(true);
        hostPanael.SetActive(false);
        joinButton.interactable = false;
    }

    public async void OnClickJoinRoomButton()
    {
        var sceneManager = _runner.gameObject.GetComponent<NetworkSceneManagerDefault>();

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = _selectedRoomname,
            SceneManager = sceneManager
        };
        
        await _runner.StartGame(startGameArgs);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        foreach(Transform child in sessionListCenter)
        {
            Destroy(child.gameObject);
        }

        foreach (var session in sessionList)
        {
            if(!session.IsOpen || !session.IsVisible)
            {
                continue;
            }
            Debug.Log("잘 실행됨");

            GameObject item = Instantiate(roomListItemPrefab, sessionListCenter, false);
            RoomListItem itemScript = item.GetComponent<RoomListItem>();

            itemScript.Setup(session, this);
        }

        _selectedRoomname = "";
        joinButton.interactable = false;
    }

    public void OnRoomSelected(string roomName)
    {
        _selectedRoomname = roomName;
        joinButton.interactable = true;
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
