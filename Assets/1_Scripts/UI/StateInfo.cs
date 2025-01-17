using TMPro;
using UnityEngine;

public class StateInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI readyText = null;

    private string readyKey = string.Empty;

    public void Reset()
    {
        readyText.text = $"Waiting TetrisPlayer...";
        readyKey = string.Empty;
    }
    
    public void Init(string inReadyKey)
    {
        gameObject.SetActive(true);
        readyText.text = $"Press {inReadyKey} key to ready";
        readyKey = inReadyKey;
    }

    public void Ready()
    {
        readyText.text = "Ready";
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void SetWin()
    {
        gameObject.SetActive(true);
        readyText.text = $"You Win\nPress {readyKey} key to ready";
    }

    public void SetLose()
    {
        gameObject.SetActive(true);
        readyText.text = $"You lose\nPress {readyKey} key to ready";
    }
}
