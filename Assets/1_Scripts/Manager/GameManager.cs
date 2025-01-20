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
        if (changedProps.TryGetValue(TetrisDefine.PlayerIsReadyProperty, out object isReadyValue))
        {
            TetrisPlayer tetrisPlayer = (TetrisPlayer)targetPlayer.TagObject;
            tetrisPlayer.SetReadyUI((bool)isReadyValue);
        }
        
        if (!PhotonNetwork.IsMasterClient)
            return;
            
        if (CheckAllPlayerIsReady())
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions() { Reliability = true };
            PhotonNetwork.RaiseEvent((byte)TetrisEventCode.AllPlayerIsReadyEvent, null, raiseEventOptions, sendOptions);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch ((TetrisEventCode)photonEvent.Code)
        {
            case TetrisEventCode.AllPlayerIsReadyEvent:
                StartGame();
                break;
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerList[0].IsMasterClient)
            {
                stageArray[0].photonView.TransferOwnership(PhotonNetwork.PlayerList[0]);
                stageArray[1].photonView.TransferOwnership(PhotonNetwork.PlayerList[1]);
            }
            else
            {
                stageArray[0].photonView.TransferOwnership(PhotonNetwork.PlayerList[1]);
                stageArray[1].photonView.TransferOwnership(PhotonNetwork.PlayerList[0]);
            }
        }
        
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

    public PlayerInitData GetPlayerInitData(Player player)
    {
        if (player.IsMasterClient)
            return new PlayerInitData(player, stageArray[0], stateInfoArray[0]);
        else
            return new PlayerInitData(player, stageArray[1], stateInfoArray[1]);
    }

    private void TryCreatePlayer()
    {
        if (PhotonNetwork.LocalPlayer.TagObject != null)
            return;

        int index = PhotonNetwork.IsMasterClient ? 0 : 1;
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
        
        // 항상 마스터 클라이언트는 첫 번째 스테이지를 일반 클라이언트는 두 번째 스테이지를 소유하지만,
        // 소유권과 상관없이 본인은 항상 첫 번째 스테이지에서 플레이할 수 있게 스테이지 위치 적절히 변경
        stageArray[index].gameObject.transform.SetSiblingIndex(0);
    }

    private bool CheckAllPlayerIsReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(TetrisDefine.PlayerIsReadyProperty, out object readyValue))
            {
                if ((bool)readyValue == true)
                {
                    continue;
                }
            }

            return false;
        }

        return true;
    }
}
