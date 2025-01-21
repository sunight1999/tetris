using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

public class Option : UI
{
    [SerializeField]
    private GameObject optionPanel = null;
    
    [SerializeField]
    private GameObject blurImage = null;
    
    public override void SetVisibility(bool isVisible)
    {
        optionPanel.SetActive(isVisible);
        blurImage.SetActive(isVisible);
    }

    public void Restart()
    {
        if (GameManager.Instance.GameStatePrevPause == GameState.Playing)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte)TetrisEventCode.StartGameEvent, null, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
