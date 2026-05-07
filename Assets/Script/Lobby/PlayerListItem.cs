using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI readyStateText;

    private LobbyPlayer _linkedPlayer;

    public void Setup(LobbyPlayer player)
    {
        _linkedPlayer = player;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(_linkedPlayer == null)
        {
            return;
        }

        nameText.text = $"Player { _linkedPlayer.Object.InputAuthority.PlayerId}";

        if (_linkedPlayer.IsHost)
        {
            readyStateText.text = "Host";
        }
        else if (_linkedPlayer.IsReady)
        {
            readyStateText.text = "Ready";
        }
        else
        {
            readyStateText.text = "Waiting";
        }
    }
}
