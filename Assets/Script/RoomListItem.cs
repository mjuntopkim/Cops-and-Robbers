using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button roomButton;

    private string _sessionId;
    private MainMenuManager _menuManager;

    public void Setup(SessionInfo sessionInfo, MainMenuManager manager)
    {
        _menuManager = manager;

        _sessionId = sessionInfo.Name;

        string _roomName = sessionInfo.Name;

        if (sessionInfo.Properties != null && sessionInfo.Properties.TryGetValue("RoomTitle", out var titleprop))
        {
            if (titleprop.IsString)
            {
                _roomName = (string)titleprop;
            }
        }

        roomNameText.text = _roomName;
        playerCountText.text = $"{sessionInfo.PlayerCount} / {sessionInfo.MaxPlayers}";

        roomButton.onClick.AddListener(OnClickRoom);
    }

    private void OnClickRoom()
    {
        _menuManager.OnRoomSelected(_sessionId);
    }
}
