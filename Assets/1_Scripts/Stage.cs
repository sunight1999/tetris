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
            
            if (!MoveBlock(Direction.Down))
            {
                // 블록을 해당 위치에 확정
                foreach (StageCell stageCell in _previousStepCells)
                    stageCell.SetBlock(_currentBlock);
                
                _previousStepCells.Clear();

                // 블록이 바닥이나 다른 블록에 닿았을 경우 라인 완성 검사
                CheckLines();
                continue;
            }
        }
    }

    private void CreateBlock()
    {
        int blockIndex = Random.Range(0, TetrisDefine.Instance.tetrisBlocks.Length);
        _currentBlock = TetrisDefine.Instance.tetrisBlocks[blockIndex];

        _currentBlockX = _createPositionX;
        _currentBlockY = 0;
        _isPlacing = true;

        DrawStep();
    }

    public bool MoveBlock(Direction direction)
    {
        if (!_isPlacing)
            return true;

        int nextBlockX = _currentBlockX;
        int nextBlockY = _currentBlockY;

        switch (direction)
        {
            case Direction.Left:
                --nextBlockX;
                break;

            case Direction.Down:
                ++nextBlockY;
                break;

            case Direction.Right:
                ++nextBlockX;
                break;
        }

        if (CheckCollision(_currentBlock, nextBlockX, nextBlockY))
            return false;

        _currentBlockX = nextBlockX;
        _currentBlockY = nextBlockY;

        ClearPreviousStep();
        DrawStep();
        return true;
    }

    private void DrawStep()
    {
        // 현재 블록의 위치를 스테이지에 표시
        for (int i = 0; i < _currentBlock.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (_currentBlockY + i);

            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!_currentBlock.blockShape[(i * TetrisDefine.TetrisBlockCols) + j])
                    continue;

                StageCell stageCell = _stage[offset + _currentBlockX + j];
                stageCell.SetBlockTemporarily(_currentBlock);
                _previousStepCells.Add(stageCell);
            }
        }
    }

    private void ClearPreviousStep()
    {
        // 이전 스텝의 블록 잔상 제거
        foreach (StageCell stageCell in _previousStepCells)
            stageCell.Reset();

        _previousStepCells.Clear();
    }

    private bool CheckCollision(TetrisBlock block, int blockPositionX, int blockPositionY)
    {
        // 검사할 위치가 스테이지를 벗어나는지 확인
        if (blockPositionX < 0 || blockPositionX + block.width > TetrisDefine.TetrisStageCols ||
            blockPositionY < 0 || blockPositionY + block.height > TetrisDefine.TetrisStageRows)
            return true;

        // 검사할 위치가 다른 블록에 닿는지 확인
        for (int i = 0; i < block.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (blockPositionY + i);
            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!block.blockShape[(i * TetrisDefine.TetrisBlockCols) + j])
                    continue;

                StageCell stageCell = _stage[offset + blockPositionX + j];
                if (stageCell.IsBlocked)
                    return true;
            }
        }

        return false;
    }
    
    private void CheckLines()
    {
        _isPlacing = false;
        
        // 완성이 된 라인이 있는지 스테이지 검사
        
        CreateBlock();
    }

    public void RotateBlock()
    {
        TetrisBlock rotatedBlock = new TetrisBlock(_currentBlock);
        rotatedBlock.Rotate();

        int rotatedBlockX = _currentBlockX;
        int rotatedBlockY = _currentBlockY;

        // 벽 가까이에서 회전할 경우 위치 보정
        if (rotatedBlockX + rotatedBlock.width >= TetrisDefine.TetrisStageCols)
            rotatedBlockX = TetrisDefine.TetrisStageCols - rotatedBlock.width;

        if (rotatedBlockY + rotatedBlock.height >= TetrisDefine.TetrisStageRows)
            rotatedBlockY = TetrisDefine.TetrisStageRows - rotatedBlock.height;

        // 회전 위치에 대해 충돌 검사 진행
        if (CheckCollision(rotatedBlock, rotatedBlockX, rotatedBlockY))
            return;
        
        // 현재 블록 정보를 회전한 블록으로 업데이트
        _currentBlock = rotatedBlock;
        _currentBlockX = rotatedBlockX;
        _currentBlockY = rotatedBlockY;

        ClearPreviousStep();
        DrawStep();
    }

    public void DropBlock()
    {

    }
}
