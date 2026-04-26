using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimingMiniGame : MonoBehaviour, IMiniGame
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private RectTransform needle;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private Image successZoneImage;

    [SerializeField] private float rotationSpeed = 250f;
    [SerializeField] private float successAngleSize = 45f;

    private Action _onSuccess;
    private Action _onFail;
    private bool _isPlaying = false;

    private float _currentAngle = 0f;
    private float _targetZoneStartAngle = 0f;

    public void StartGame(Action onSuccess, Action onFail)
    {
        _onSuccess = onSuccess;
        _onFail = onFail;

        SetupRandomZone();

        _currentAngle = 0f;
        needle.localEulerAngles = new Vector3(0, 0, -_currentAngle);

        gamePanel.SetActive(true);
        _isPlaying = true;
    }

    public void EndGame()
    {
        _isPlaying = false;
        gamePanel.SetActive(false);
    }

    private void SetupRandomZone()
    {
        successZoneImage.fillAmount = successAngleSize / 360f;
        _targetZoneStartAngle = UnityEngine.Random.Range(0f, 360f - successAngleSize);
        successZone.localEulerAngles = new Vector3(0, 0, -_targetZoneStartAngle);
    }

    private void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        _currentAngle += rotationSpeed * Time.deltaTime;

        if(_currentAngle >= 360f)
        {
            _currentAngle -= 360f;
        }

        needle.localEulerAngles = new Vector3(0, 0, -_currentAngle);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckResult();
        }
    }

    private void CheckResult()
    {
        _isPlaying = false;

        float zoneEndAngle = _targetZoneStartAngle + successAngleSize;

        if(_currentAngle >= _targetZoneStartAngle && _currentAngle <= zoneEndAngle)
        {
            Debug.Log("성공");
            if (_onSuccess != null)
            {
                _onSuccess();
            }
        }
        else
        {
            Debug.Log("실패");
            if (_onFail != null)
            {
                _onFail();
            }
        }

        EndGame();
    }
}
