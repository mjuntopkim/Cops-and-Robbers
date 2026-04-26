using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [SerializeField] private MonoBehaviour timingGame;
    [SerializeField] private MonoBehaviour sequenceGame;

    private IMiniGame[] _miniGames;
    private IMiniGame _currentMiniGame;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        _miniGames = new IMiniGame[]
        {
            timingGame as IMiniGame,
            sequenceGame as IMiniGame
        };
    }

    public void PlayRandomMiniGame(Action onSuccess, Action onFail)
    {
        if(_currentMiniGame != null)
        {
            return;
        }

        int randomIndex = Random.Range(0, _miniGames.Length);
        _currentMiniGame = _miniGames[randomIndex];

        _currentMiniGame.StartGame(
            onSuccess: () =>
            {
                _currentMiniGame = null;
                onSuccess?.Invoke();
            },
            onFail: () =>
            {
                _currentMiniGame = null;
                onFail?.Invoke();
            }
        );
    }

    public void CancelCurrentMiniGame()
    {
        if(_currentMiniGame != null)
        {
            _currentMiniGame.EndGame();
            _currentMiniGame = null;
        }
    }
}
