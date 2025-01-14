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
    [SerializeField] private float _lineCompleteTime = .5f;

    private bool _isPlacing;
    private TetrisBlock _currentBlock;
    private int _currentBlockX;
    private int _currentBlockY;
    
    private int _currentHighestY;
    private List<int> _completedLines;
    
    private List<StageCell> _previousStepCells;
    
    private void Awake()
    {
        _isPlacing = false;
        _currentHighestY = TetrisDefine.TetrisStageRows - 1;
        _completedLines = new List<int>();
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
            
            if (!TryMoveBlock(Direction.Down))
            {
                // 블록을 해당 위치에 확정
                foreach (StageCell stageCell in _previousStepCells)
                    stageCell.SetBlock(_currentBlock);

                // 추후 라인을 제거할 때 확인 범위를 좁히기 위해 가장 높은 블록의 y 값 저장
                if (_currentHighestY > _currentBlockY)
                    _currentHighestY = _currentBlockY;
                
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

    public bool TryMoveBlock(Direction direction)
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
            for (int j = 0; j < block.width; j++)
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
        StartCoroutine(CheckLinesCoroutine());
    }

    IEnumerator CheckLinesCoroutine()
    {
        // 완성된 라인 확인
        for (int i = TetrisDefine.TetrisStageRows - 1; i >= _currentHighestY; i--)
        {
            bool isCompleted = true;
            int offset = TetrisDefine.TetrisStageCols * i;
            for (int j = 0; j < TetrisDefine.TetrisStageCols; j++)
            {
                StageCell stageCell = _stage[offset + j];
                if (!stageCell.IsBlocked)
                {
                    isCompleted = false;
                    break;
                }
            }

            if (isCompleted)
                _completedLines.Add(i);
        }

        if (_completedLines.Count > 0)
        {
            CompleteLines();
            yield return new WaitForSeconds(_lineCompleteTime);
            AdjustLines();
            yield return new WaitForSeconds(_lineCompleteTime);
        }
        
        CreateBlock();
    }

    private void CompleteLines()
    {
        foreach (int line in _completedLines)
        {
            int offset = TetrisDefine.TetrisStageCols * line;
            for (int i = 0; i < TetrisDefine.TetrisStageCols; i++)
            {
                StageCell stageCell = _stage[offset + i];
                stageCell.Reset();
            }
        }
    }
    
    private void AdjustLines()
    {
        if (_completedLines.Count == 0)
            return;

        int adjustRangeBottom = _completedLines[0];
        int adjustRangeTop = _currentHighestY;
        
        // 블록이 있는 라인 중 가장 윗 라인이 완성되면 해당 라인 위로는 블록이 없으므로 블록 검사 범위 축소
        for (int i = _completedLines.Count - 1; i >= 0; i--)
        {
            if (_completedLines[i] == adjustRangeTop)
            {
                _completedLines.Remove(i);
                --adjustRangeTop;
            }
        }

        int headLine = adjustRangeBottom;
        int tailLine = adjustRangeBottom - 1;
        while (tailLine >= adjustRangeTop)
        {
            if (_completedLines.Contains(tailLine))
            {
                --tailLine;
                continue;
            }

            SwapLine(tailLine, headLine);

            --tailLine;
            --headLine;
        }
        
        _currentHighestY += _completedLines.Count;
        _completedLines.Clear();
    }

    private void SwapLine(int fromLine, int toLine)
    {
        for (int i = 0; i < TetrisDefine.TetrisStageCols; i++)
        {
            StageCell fromStageCell = _stage[TetrisDefine.TetrisStageCols * fromLine + i];
            StageCell toStageCell = _stage[TetrisDefine.TetrisStageCols * toLine + i];

            if (!fromStageCell.IsBlocked)
                continue;
            
            Color color = fromStageCell.GetBlockColor();
            fromStageCell.Reset();
            toStageCell.SetBlock(color);
        }
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
