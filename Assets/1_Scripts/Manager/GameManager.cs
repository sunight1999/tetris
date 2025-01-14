using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameState
{
    Idle,
    Playing
}

public class GameManager : SingletonBehaviour<GameManager>
{
    public GameState GameState { get; private set; }
    private Player[] _players = new Player[TetrisDefine.PlayerCount];

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
        
        _players[0] = playerObjects[0].GetComponent<Player>();
        _players[1] = playerObjects[1].GetComponent<Player>();
    }

    private void Update()
    {
        if (GameState != GameState.Idle)
            return;
            
        if (_players[0].IsReady && _players[1].IsReady)
            Play();
    }

    public void Play()
    {
        GameState = GameState.Playing;
        
        _players[0].Play();
        _players[1].Play();
    }

    public void AttackTo(Player from, int obstacleNum)
    {
        if (from == _players[0])
            _players[1].Hit(obstacleNum);
        else
            _players[0].Hit(obstacleNum);
    }

    public void SetLose(Player player)
    {
        GameState = GameState.Idle;

        if (player == _players[0])
        {
            _players[0].Lose();
            _players[1].Win();
        }
        else
        {
            _players[0].Win();
            _players[1].Lose();
        }
    }
}
