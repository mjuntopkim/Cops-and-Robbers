using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;
    public static Dictionary<PlayerRef, PlayerRole> LobbyRole = new Dictionary<PlayerRef, PlayerRole>();

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
        if(Instance == null)
        {
            Instance = this;    //중복체크를 해야함(지금은 로비씬이 한번만 생겨서 상관은 없지만 나중에 문제가 생길 수 있다.(중복 참조))
        }
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
        CheckPlayerReady(); //이벤트 기반으로 변경 할 것, 업데이트는 비워놓아야한다.
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
            LobbyRole.Clear();

            LobbyPlayer[] allLobbyPlayers = FindObjectsOfType<LobbyPlayer>();
            foreach(var lp in allLobbyPlayers)
            {
                LobbyRole.Add(lp.Object.InputAuthority, lp.Role);
            }

            runner.LoadScene(SceneRef.FromIndex(2));
        }
    }

    public LobbyPlayer GetLobbyPlayer() //로비 플레이어가 스폰 될때 자기가 로컬이면 로비매니저에 등록을 해버리기
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
