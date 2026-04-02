using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private Transform copScroll;
    [SerializeField] private Transform robberScroll;

    private NetworkRunner runner;
    private LobbyPlayer _lobbyPlayer;
    private string _currentRoomTitle = "";

    private Dictionary<LobbyPlayer, PlayerListItem> _playerListItem = new Dictionary<LobbyPlayer, PlayerListItem>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        
        if(runner != null)
        {
            UpdateRoomNameDisplay();

            if (runner.IsServer)
            {
                readyButton.gameObject.SetActive(false);
                startButton.gameObject.SetActive(true);
                startButton.interactable = false;
            }
            else
            {
                readyButton.gameObject.SetActive(true);
                startButton.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        CheckPlayerReady();
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
            if(player.Object == null || !player.Object.IsValid)
            {
                allReady = false;
                continue;
            }

            if(!player.HasInputAuthority && !player.IsReady)
            {
                allReady = false;
                break;
            }
        }

        startButton.interactable = allReady;
    }

    private void UpdateRoomNameDisplay()
    {
        if(runner != null && runner.SessionInfo.IsValid)
        {
            if(runner.SessionInfo.Properties.TryGetValue("RoomTitle", out var titleProp))
            {
                if(titleProp.IsString && _currentRoomTitle != (string)titleProp)
                {
                    _currentRoomTitle = (string)titleProp;
                    roomName.text = _currentRoomTitle;
                }
            }
        }
    }

    public void AddPlayerUI(LobbyPlayer player)
    {
        GameObject obj = Instantiate(playerListItemPrefab, copScroll);
        PlayerListItem item = obj.GetComponent<PlayerListItem>();

        item.Setup(player);
        _playerListItem.Add(player, item);

        UpdatePlayerUIPosition(player);
    }

    public void UpdatePlayerUIPosition(LobbyPlayer player)
    {
        if(_playerListItem.TryGetValue(player, out PlayerListItem item))
        {
            Transform targetContent;
            if (player.Role == PlayerRole.Cop)
            {
                targetContent = copScroll;
            }
            else
            {
                targetContent = robberScroll;
            }

            item.transform.SetParent(targetContent, false);

            if (player.HasStateAuthority)
            {
                item.transform.SetAsFirstSibling();
            }
            
        }
    }

    public void UpdatePlayerUIInfo(LobbyPlayer player)
    {
        if (_playerListItem.TryGetValue(player, out PlayerListItem item))
        {
            item.UpdateUI();
        }
    }

    public void RemovePlayerUI(LobbyPlayer player)
    {
        if(_playerListItem.TryGetValue(player, out PlayerListItem item))
        {
            Destroy(item.gameObject);
            _playerListItem.Remove(player);
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
        if (runner.IsServer)
        {
            runner.LoadScene(SceneRef.FromIndex(2));
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
