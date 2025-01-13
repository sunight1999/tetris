using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Stage : MonoBehaviour
{
    [SerializeField] private StageCell[] _stage;
    
    [SerializeField] private int _createPositionX = 4;
    [SerializeField] private float _blockDownTime = 1f;

    private bool _isPlacing;
    private TetrisBlock _currentBlock;
    private int _currentBlockX;
    private int _currentBlockY;

    private List<StageCell> _previousStepCells;
    
    private void Awake()
    {
        _isPlacing = false;
        _previousStepCells = new List<StageCell>();
    }

    private void Start()
    {
        Begin();
    }

    public void Begin()
    {
        StartCoroutine(StageUpdateCoroutine());

        CreateBlock();
    }

    IEnumerator StageUpdateCoroutine()
    {
        while (GameManager.Instance.GameState == GameState.Playing)
        {
            yield return new WaitForSeconds(_blockDownTime);

            if (!_isPlacing)
                continue;
            
            // 블록이 바닥이나 다른 블록에 닿았을 경우 라인 완성 검사
            if (CheckCollision())
            {
                _previousStepCells.Clear();
                CheckLines();
                continue;
            }
            
            StepBlock();
        }
    }

    private void CreateBlock()
    {
        int blockIndex = Random.Range(0, TetrisDefine.Instance.tetrisBlocks.Length);
        _currentBlock = TetrisDefine.Instance.tetrisBlocks[blockIndex];

        _currentBlockX = _createPositionX;
        _currentBlockY = -1;
        _isPlacing = true;
    }
    
    private void StepBlock()
    {
        // 이전 스텝의 블록 잔상 제거
        if (_previousStepCells.Count > 0)
        {
            foreach (StageCell stageCell in _previousStepCells)
                stageCell.Reset();
            
            _previousStepCells.Clear();
        }

        ++_currentBlockY;

        // 현재 블록의 위치를 스테이지에 표시
        for (int i = 0; i < _currentBlock.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (_currentBlockY + i);
            
            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!_currentBlock.blockShape[(i * TetrisDefine.TetrisBlockCols) + j])
                    continue;
                
                StageCell stageCell = _stage[offset + _currentBlockX + j];
                stageCell.SetBlock(_currentBlock);
                _previousStepCells.Add(stageCell);
            }
        }
    }

    private bool CheckCollision()
    {
        // 현재 블록이 다른 블록에 닿았는지 확인
        /*for (int i = 0; i < _currentBlock.height; i++)
        {
            // 스테이지를 벗어나는 경우 처리
            int nextLineY = _currentBlockY + i + 1; 
            if (nextLineY >= TetrisDefine.TetrisStageRows)
                continue;
            
            int offset = TetrisDefine.TetrisStageCols * nextLineY;
            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!_currentBlock.blockShape[(i * TetrisDefine.TetrisBlockCols) + j])
                    continue;
                
                StageCell stageCell = _stage[offset + _currentBlockX + j];
                if (stageCell.IsBlocked)
                    return true;
            }
        }*/
        
        // 현재 블록이 바닥에 닿았는지 확인
        if (_currentBlockY + _currentBlock.height >= TetrisDefine.TetrisStageRows)
            return true;

        return false;
    }
    
    private void CheckLines()
    {
        _isPlacing = false;
        
        // 완성이 된 라인이 있는지 스테이지 검사
        
        CreateBlock();
    }
}
