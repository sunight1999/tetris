using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private StateInfo _stateInfo;
    
    [SerializeField] private KeyCode _leftKey;
    [SerializeField] private KeyCode _downKey;
    [SerializeField] private KeyCode _rightKey;
    [SerializeField] private KeyCode _rotateKey;
    [SerializeField] private KeyCode _dropKey;
    [SerializeField] private KeyCode _holdKey;
    [SerializeField] private KeyCode _readyKey;
    [SerializeField] private Stage _stage;
    
    public bool IsReady { get; private set; }
    public Queue<int> HitQueue;
    
    private void Awake()
    {
        IsReady = false;
        HitQueue = new Queue<int>();
    }

    private void Start()
    {
        _stateInfo.Init(_readyKey.ToString());
        _stage.SetPlayer(this);
    }

    void Update()
    {
        if (UIManager.Instance.IsMenuOpened)
            return;
        
        if (GameManager.Instance.GameState != GameState.Playing)
        {
            if (Input.GetKeyDown(_readyKey))
                Ready();
            
            return;
        }

        if (Input.GetKeyDown(_leftKey))
            _stage.TryMoveBlock(Direction.Left);
        if (Input.GetKeyDown(_downKey))
            _stage.TryMoveBlock(Direction.Down);
        if (Input.GetKeyDown(_rightKey))
            _stage.TryMoveBlock(Direction.Right);
        if (Input.GetKeyDown(_rotateKey))
            _stage.RotateBlock();
        if (Input.GetKeyDown(_holdKey))
            _stage.HoldBlock();
        if (Input.GetKeyDown(_dropKey))
            _stage.DropBlock();
    }

    public void Ready()
    {
        IsReady = true;
        _stateInfo.Ready();
    }

    public void Play()
    {
        // 다음 게임을 위해 준비 상태를 false로 전환
        IsReady = false;
        
        _stage.Play();
        _stateInfo.Disable();
    }

    public void Hit(int obstacleNum)
    {
        HitQueue.Enqueue(obstacleNum);
    }

    public void Win()
    {
        _stateInfo.SetWin();
        _stage.Stop();
    }

    public void Lose()
    {
        _stateInfo.SetLose();
        _stage.Stop();
    }
}
