using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI _interactUI;
    [SerializeField] private TextMeshProUGUI _alertUI;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void SetInteractUIActive(string message, bool isActive)
    {
        if (_interactUI == null) return;

        if (isActive)
        {
            _interactUI.text = message;

            if (!_interactUI.gameObject.activeSelf)
            {
                _interactUI.gameObject.SetActive(true);
            }
        }
        else
        {
            if (_interactUI.gameObject.activeSelf)
            {
                _interactUI.gameObject.SetActive(false);
            }
        }
    }

    public void ShowGlobalAlarm(string message, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(AlarmDisplayRoutine(message, duration));
    }

    private IEnumerator AlarmDisplayRoutine(string message, float duration)
    {
        _alertUI.text = message;
        _alertUI.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        _alertUI.gameObject.SetActive(false);
    }
}
