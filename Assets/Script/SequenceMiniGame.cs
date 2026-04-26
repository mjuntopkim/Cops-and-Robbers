using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SequenceMiniGame : MonoBehaviour, IMiniGame
{
    public enum CommandKey { W, A, S, D}

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private RectTransform textContainer;
    [SerializeField] private GameObject keyTextPrefab;

    [SerializeField] private int sequenceLength = 5;

    private Color defaultColor = new Color(1f, 1f, 1f, 0.5f);
    private Color activeColor = Color.yellow;
    private Color completeColor = Color.green;
    private Color failColor = Color.red;

    private Action _onSuccess;
    private Action _onFail;
    private bool _isPlaying = false;

    private List<CommandKey> _targetSequence = new List<CommandKey>();
    private List<TextMeshProUGUI> _keyTexts = new List<TextMeshProUGUI>();
    private int _currentIndex = 0;

    public void StartGame(Action onSuccess, Action onFail)
    {
        _onSuccess = onSuccess;
        _onFail = onFail;

        GenerateSequence();
        UpdateUI();

        gamePanel.SetActive(true);
        _isPlaying = true;
    }

    public void EndGame()
    {
        _isPlaying = false;
        gamePanel.SetActive(false);
    }

    private void GenerateSequence()
    {
        _targetSequence.Clear();
        _currentIndex = 0;

        foreach(Transform child in textContainer)
        {
            Destroy(child.gameObject);
        }
        _keyTexts.Clear();

        for(int i = 0; i < sequenceLength; i++)
        {
            CommandKey randomKey = (CommandKey)UnityEngine.Random.Range(0, 4);
            _targetSequence.Add(randomKey);

            GameObject textObj = Instantiate(keyTextPrefab, textContainer);
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = randomKey.ToString();
            _keyTexts.Add(tmp);
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;

        if (Input.GetKeyDown(KeyCode.W)) CheckInput(CommandKey.W);
        else if (Input.GetKeyDown(KeyCode.A)) CheckInput(CommandKey.A);
        else if (Input.GetKeyDown(KeyCode.S)) CheckInput(CommandKey.S);
        else if (Input.GetKeyDown(KeyCode.D)) CheckInput(CommandKey.D);
    }

    private void CheckInput(CommandKey inputKey)
    {
        if (_targetSequence[_currentIndex] == inputKey)
        {
            _currentIndex++;
            UpdateUI();

            if (_currentIndex >= sequenceLength)
            {
                _isPlaying = false;
                Debug.Log("성공");
                _onSuccess?.Invoke();
                EndGame();
            }
        }
        else
        {
            Debug.Log("실패");
            _currentIndex = 0;
            FlashFailUI();

            _onFail?.Invoke();
            Invoke(nameof(EndGame), 0.2f);
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < _keyTexts.Count; i++)
        {
            if (i < _currentIndex) _keyTexts[i].color = completeColor;
            else if (i == _currentIndex) _keyTexts[i].color = activeColor;
            else _keyTexts[i].color = defaultColor;
        }
    }

    private void FlashFailUI()
    {
        foreach (var tmp in _keyTexts)
        {
            tmp.color = failColor;
        }
        CancelInvoke(nameof(UpdateUI));
        Invoke(nameof(UpdateUI), 0.2f);
    }
}
