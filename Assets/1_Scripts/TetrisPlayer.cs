using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TetrisPlayer : MonoBehaviourPun
{
    private Stage stage = null;
    private PlayerInput playerInput = null;
    private StateInfo stateInfo = null;
    
    public bool IsReady { get; private set; } = false;
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
    
    public void OnStartGame()
    {
        // 다음 게임을 위해 준비 상태를 false로 전환
        IsReady = false;
        
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
    }
    
    public void Ready()
    {
        IsReady = true;
        stateInfo.Ready();
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
            if (player != PhotonNetwork.LocalPlayer)
            {
                TetrisPlayer tetrisPlayer = (TetrisPlayer)player.TagObject;
                tetrisPlayer.Hit(obstacleNum);
            }
        }
    }
    
    public void Hit(int obstacleNum)
    {
        HitQueue.Enqueue(obstacleNum);
    }
}
