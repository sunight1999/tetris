using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TetrisPlayer : MonoBehaviourPun, IPunInstantiateMagicCallback 
{
    private Stage stage = null;
    private PlayerInput playerInput = null;
    private StateInfo stateInfo = null;

    public Queue<int> HitQueue = new Queue<int>();

    public Stage Stage => stage;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        GameManager.Instance.StartGameEvent += OnStartGame;
        GameManager.Instance.EndGameEvent += OnEndGame;
    }

    private void OnDisable()
    {
        stateInfo.Reset();

        GameManager.Instance.StartGameEvent -= OnStartGame;
        GameManager.Instance.EndGameEvent -= OnEndGame;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = this;

        PlayerInitData playerInitData = GameManager.Instance.GetPlayerInitData(info.Sender);
        Init(playerInitData);
    }

    public void OnStartGame()
    {
        stage.OnGameStart();
        stateInfo.Disable();
    }

    public void OnEndGame()
    {
        stage.OnGameEnd();
    }

    public void Init(PlayerInitData playerInitData)
    {
        stage = playerInitData.stage;
        stateInfo = playerInitData.stateInfo;

        stage.SetPlayer(playerInitData.player);
        stateInfo.Init(playerInput.ReadyKey.ToString());
    }

    public void SetReady(bool isReady)
    {
        Hashtable readyPropertyHashTable = new Hashtable()
        {
            { TetrisDefine.PlayerIsReadyProperty, isReady }
        };
        PhotonNetwork.SetPlayerCustomProperties(readyPropertyHashTable);
    }

    public void SetReadyUI(bool isReady)
    {
        if (isReady)
        {
            stateInfo.Ready();
        }
    }

    public void Win()
    {
        stateInfo.SetWin();
        stage.OnGameEnd();
    }

    public void Lose()
    {
        stateInfo.SetLose();
        stage.OnGameEnd();
    }
    
    public void Attack(int obstacleNum)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.IsLocal)
            {
                // 로컬에선 다른 플레이어의 HitQueue를 직접 처리하지 않고 스테이지 정보를 통째로 받아오므로 RpcTarget.Others 사용
                TetrisPlayer tetrisPlayer = (TetrisPlayer)player.TagObject;
                tetrisPlayer.photonView.RPC("Hit", RpcTarget.Others, obstacleNum);
                break;
            }
        }
    }
    
    [PunRPC]
    public void Hit(int obstacleNum)
    {
        Debug.Log(obstacleNum);
        
        HitQueue.Enqueue(obstacleNum);
    }
}
