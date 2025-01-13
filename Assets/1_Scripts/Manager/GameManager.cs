using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Idle,
    Playing
}

public class GameManager : SingletonBehaviour<GameManager>
{
    public GameState GameState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        GameState = GameState.Idle;
        
        Begin();
    }

    private void Start()
    {
        
    }

    public void Begin()
    {
        GameState = GameState.Playing;
    }
}
