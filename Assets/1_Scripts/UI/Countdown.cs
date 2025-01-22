using System.Collections;
using TMPro;
using UnityEngine;

public class Countdown : UI
{
    [SerializeField]
    private float countdown = 3f;
    
    [SerializeField]
    private TextMeshProUGUI countdownText = null;

    private void OnEnable()
    {
        Count();
    }
    
    private void Count()
    {
        countdownText.text = $"{countdown}";
        StartCoroutine(CoroutineCount());
    }

    private IEnumerator CoroutineCount()
    {
        float newCountdown = countdown;
        while (newCountdown > 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            newCountdown -= 1f;
            countdownText.text = $"{(int)newCountdown}";
        }
        
        UIManager.Instance.SetVisibility(MenuType.Countdown, false);
        GameManager.Instance.ResumeGame();
    }
}
