using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string gameVersion = "1";

    [SerializeField]
    private GameObject controlPanel = null;

    [SerializeField]
    private GameObject progressText = null;

    private bool isConnecting = false;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = $"Player{new System.Random().Next() * 10000}";
        PhotonPeer.RegisterType(typeof(StageCell[]), 0, StageDataSerializer.Serialize, StageDataSerializer.Deserialize);
    }

    public override void OnConnectedToMaster()
    {
        if (!isConnecting)
            return;

        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetControlPanelVisibility(true);
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel("MainGame");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("방에 접속하거나 새로운 방을 생성할 수 없습니다. 네트워크 설정을 확인해주세요.");
        SetControlPanelVisibility(true);
    }

    public void GameStart()
    {
        Connect();
    }
    
    public void GameExit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Connect()
    {
        isConnecting = true;
        SetControlPanelVisibility(false);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    private void SetControlPanelVisibility(bool isVisible)
    {
        controlPanel.SetActive(isVisible);
        progressText.SetActive(!isVisible);
    }
}
