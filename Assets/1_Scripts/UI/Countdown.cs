using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : UI
{
    [SerializeField] private float _countdown = 3f;
    [SerializeField] private TextMeshProUGUI _countdownText;
    
    private void OnEnable()
    {
        Count();
    }

    private void Count()
    {
        _countdownText.text = $"{_countdown}";
        StartCoroutine(CountCoroutine());
    }

    IEnumerator CountCoroutine()
    {
        float countdown = _countdown;
        while (countdown > 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            countdown -= 1f;
            _countdownText.text = $"{(int)countdown}";
        }
        
        UIManager.Instance.SetVisibility(MenuType.Countdown, false);
    }
}
