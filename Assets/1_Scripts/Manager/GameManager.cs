using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public enum GameState
{
    Idle,
    Playing
}

public class GameManager : SingletonBehaviour<GameManager>
{
    public Player[] players;
    
    public GameState GameState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        GameState = GameState.Idle;
    }

    private void Start()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        if (playerObjects.Length != TetrisDefine.PlayerCount)
        {
            Debug.LogWarning($"플레이어 수가 {TetrisDefine.PlayerCount}명이어야 합니다.");
            return;
        }
        
        players[0] = playerObjects[0].GetComponent<Player>();
        players[1] = playerObjects[1].GetComponent<Player>();
    }

    private void Update()
    {
        if (GameState != GameState.Idle)
            return;
            
        if (players[0].IsReady && players[1].IsReady)
            Play();
    }

    public void Play()
    {
        GameState = GameState.Playing;
        
        players[0].Play();
        players[1].Play();
    }

    public void AttackTo(Player from, int obstacleNum)
    {
        if (from == players[0])
            players[1].Hit(obstacleNum);
        else
            players[0].Hit(obstacleNum);
    }

    public void SetLose(Player player)
    {
        GameState = GameState.Idle;

        if (player == players[0])
        {
            players[0].Lose();
            players[1].Win();
        }
        else
        {
            players[0].Win();
            players[1].Lose();
        }
    }
}
