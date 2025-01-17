using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : SingletonBehaviourPunCallbacks<GameManager>
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
        Hashtable props = new Hashtable()
        {
            { TetrisDefine.PlayerLoadedLevelProperty, true }
        };
        PhotonNetwork.SetPlayerCustomProperties(props);
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
        // 플레이어의 씬 로딩이 끝나면 플레이어 오브젝트 생성 및 초기화 진행
        if (changedProps.TryGetValue(TetrisDefine.PlayerLoadedLevelProperty, out object loadedLevelValue))
        {
            bool isPlayerLoadedLevel = (bool)loadedLevelValue;
            if (isPlayerLoadedLevel)
            {
                PlayerInitData playerInitData = new PlayerInitData();
                if (targetPlayer.IsLocal)
                {
                    playerInitData.stage = stageArray[0];
                    playerInitData.stateInfo = stateInfoArray[0];
                }
                else
                {
                    playerInitData.stage = stageArray[1];
                    playerInitData.stateInfo = stateInfoArray[1];
                }
                
                GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
                TetrisPlayer tetrisPlayer = playerObject.GetComponent<TetrisPlayer>();
                tetrisPlayer.Init(playerInitData);
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

    private bool CheckAllPlayerIsReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(TetrisDefine.PlayerIsReadyProperty, out object readyValue))
            {
                if ((bool)readyValue)
                {
                    continue;
                }
            }

            return false;
        }

        return true;
    }
}
