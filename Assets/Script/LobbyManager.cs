using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;
    private TextMeshProUGUI roomName;

    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private Button changeRoomNameButton;

    private NetworkRunner _runner;
    private LobbyPlayer _lobbyPlayer;
    private string _currentRoomTitle = "";

    private void Start()
    {
        _runner = FindObjectOfType<NetworkRunner>();
        
        if(_runner != null)
        {
            if (_runner.IsServer)
            {
                readyButton.gameObject.SetActive(false);
                startButton.gameObject.SetActive(true);
                startButton.interactable = false;

                roomNameInputField.gameObject.SetActive(true);
                changeRoomNameButton.gameObject.SetActive(true);
            }
            else
            {
                readyButton.gameObject.SetActive(true);
                startButton.gameObject.SetActive(false);

                roomNameInputField.gameObject.SetActive(false);
                changeRoomNameButton.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        CheckPlayerReady();
        UpdateRoomNameDisplay();
    }

    private void CheckPlayerReady()
    {
        var allPlayer = FindObjectsOfType<LobbyPlayer>();

        if(allPlayer.Length <= 1)
        {
            startButton.interactable = false;
            return;
        }

        bool allReady = true;
        foreach(var player in allPlayer)
        {
            if(!player.HasStateAuthority && !player.IsReady)
            {
                allReady = false;
                break;
            }
        }

        startButton.interactable = allReady;
    }

    private void UpdateRoomNameDisplay()
    {
        if(_runner != null && _runner.SessionInfo.IsValid)
        {
            if(_runner.SessionInfo.Properties.TryGetValue("RoomTitle", out var titleProp))
            {
                if(titleProp.IsString && _currentRoomTitle != (string)titleProp)
                {
                    _currentRoomTitle = (string)titleProp;
                    roomName.text = _currentRoomTitle;
                }
            }
        }
    }

    public void OnClickChangeRoomNameButton()
    {
        if(_runner != null && _runner.IsServer)
        {
            string newName = roomNameInputField.text;

            if (!string.IsNullOrEmpty(newName))
            {
                var newProps = new Dictionary<string, SessionProperty>();
                newProps.Add("RoomTitle", newName);

                _runner.SessionInfo.UpdateCustomProperties(newProps);
            }
        }
    }

    public void OnClickReadyButton()
    {
        if(_lobbyPlayer == null)
        {
            _lobbyPlayer = GetLobbyPlayer();
        }

        if (_lobbyPlayer != null)
        {
            bool newState = !_lobbyPlayer.IsReady;
            _lobbyPlayer.Rpc_Ready(newState);
        }
    }

    public void OnClickRoleCopButton()
    {
        if(_lobbyPlayer == null)
        {
            _lobbyPlayer = GetLobbyPlayer();
        }
        if(_lobbyPlayer != null && !_lobbyPlayer.IsReady)
        {
            _lobbyPlayer.Rpc_SetRole(PlayerRole.Cop);
        }
    }

    public void OnClickRobberRoleButton()
    {
        if(_lobbyPlayer == null)
        {
            _lobbyPlayer = GetLobbyPlayer();
        }
        if(_lobbyPlayer != null && !_lobbyPlayer.IsReady)
        {
            _lobbyPlayer.Rpc_SetRole(PlayerRole.Robber);
        }
    }

    public void OnClickStartButton()
    {
        if (_runner.IsServer)
        {
            _runner.LoadScene(SceneRef.FromIndex(2));
        }
    }

    public LobbyPlayer GetLobbyPlayer()
    {
        var player = FindObjectsOfType<LobbyPlayer>();
        foreach(var p in player)
        {
            if (p.HasInputAuthority)
            {
                return p;
            }
        }
        return null;
    }
}
