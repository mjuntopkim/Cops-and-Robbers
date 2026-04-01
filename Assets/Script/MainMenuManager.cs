using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject joinPanel;

    public NetworkRunner networkRunnerPrefab;
    private NetworkRunner currentRunner;

    public void OnClickExitButton()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    public async void OnClickhostButton()
    {
        currentRunner = Instantiate(networkRunnerPrefab);
        currentRunner.name = "Network Runner";

        var sceneManager = currentRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = "Game Room",
            Scene = SceneRef.FromIndex(1),
            SceneManager = sceneManager
        };

        await currentRunner.StartGame(startGameArgs);
    }

    public async void OnClickJoinMenuButton()
    {
        mainPanel.SetActive(false);
        joinPanel.SetActive(true);

        if(currentRunner == null)
        {
            currentRunner = Instantiate(networkRunnerPrefab);
            currentRunner.name = "Network Runner";
        }

        await currentRunner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public async void OnClickQuickJoinButton()
    {

    }

    public void OnClickBackButton()
    {
        joinPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public async void JoinRoom(string roomName)
    {
        var sceneManager = currentRunner.gameObject.GetComponent<NetworkSceneManagerDefault>();

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
            SceneManager = sceneManager
        };

        await currentRunner.StartGame(startGameArgs);
    }

    public void OnSessionListUpdate(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }
}
