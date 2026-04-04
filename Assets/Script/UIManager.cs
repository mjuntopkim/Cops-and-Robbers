using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI _interactUI;

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
}
