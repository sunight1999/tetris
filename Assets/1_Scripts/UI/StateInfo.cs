using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StateInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _readyText;

    private string _readyKey;
    
    public void Init(string readyKey)
    {
        gameObject.SetActive(true);
        _readyText.text = $"Press {readyKey} key to ready";
        _readyKey = readyKey;
    }

    public void Ready()
    {
        _readyText.text = "Ready";
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void SetWin()
    {
        gameObject.SetActive(true);
        _readyText.text = $"You Win\nPress {_readyKey} key to ready";
    }

    public void SetLose()
    {
        gameObject.SetActive(true);
        _readyText.text = $"You lose\nPress {_readyKey} key to ready";
    }
}
