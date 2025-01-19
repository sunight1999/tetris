using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : SingletonBehaviourPunCallbacks<GameManager>, IOnEventCallback
{
    public event Action StartGameEvent;
    public event Action PauseGameEvent;
    public event Action EndGameEvent;
    
    [SerializeField]
    private GameObject playerPrefab = null;

    [SerializeField]
    private Stage[] stageArray = null;
    
    [SerializeField]
    private StateInfo[] stateInfoArray = null;

    public GameState GameState { get; private set; } = GameState.Idle;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Joining)
            return;

        TryCreatePlayer();
    }

    public override void OnJoinedRoom()
    {
        TryCreatePlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName}가 게임에 입장했습니다.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName}이 게임을 떠났습니다.");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 플레이어의 씬 로딩이 끝나면 플레이어 오브젝트 생성 및 초기화 진행
        if (changedProps.TryGetValue(TetrisDefine.PlayerLoadedLevelProperty, out object loadedLevelValue))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                // 이벤트 레이즈
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
                SendOptions sendOptions = new SendOptions() { Reliability = true };
                PhotonNetwork.RaiseEvent((byte)TetrisEventCode.AllPlayerLoadedLevelEvent, null, raiseEventOptions, sendOptions);
            }
        }

        // 레디 상태가 변경된 플레이어가 있으면 전체 레디 상태 점검
        if (changedProps.ContainsKey(TetrisDefine.PlayerIsReadyProperty))
        {
            if (CheckAllPlayerIsReady())
            {
                StartGame();
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch ((TetrisEventCode)photonEvent.Code)
        {
            case TetrisEventCode.AllPlayerLoadedLevelEvent:
                break;

            case TetrisEventCode.AllPlayerIsReadyEvent:
                break;
        }
    }

    public void StartGame()
    {
        GameState = GameState.Playing;
        StartGameEvent?.Invoke();
    }

    public void EndGame()
    {
        GameState = GameState.Idle;
        EndGameEvent?.Invoke();
    }

    public void LeaveGame(GameObject player)
    {
        EndGame();
        PhotonNetwork.Destroy(player);
        PhotonNetwork.LeaveRoom();
    }

    public void SetLose(Player player)
    {
        EndGame();

        // TODO: 게임 결과 상태 네트워크 공유 필요
        if (player.IsLocal)
        {
            
        }
        else
        {
            
        }
    }

    private void TryCreatePlayer()
    {
        if (PhotonNetwork.LocalPlayer.TagObject != null)
            return;

        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
        TetrisPlayer tetrisPlayer = playerObject.GetComponent<TetrisPlayer>();
        tetrisPlayer.Init(stageArray[0], stateInfoArray[0]);
    }

    private bool CheckAllPlayerLoadedLevel()
    {
        return CheckAllPlayerIs(TetrisDefine.PlayerLoadedLevelProperty, true);
    }

    private bool CheckAllPlayerIsReady()
    {
        return CheckAllPlayerIs(TetrisDefine.PlayerIsReadyProperty, true);
    }

    private bool CheckAllPlayerIs(string propertyKey, bool isValue)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(propertyKey, out object readyValue))
            {
                if ((bool)readyValue == isValue)
                {
                    continue;
                }
            }

            return false;
        }

        return true;
    }
}
