using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Stage : MonoBehaviour
{
    [SerializeField] private StageCell[] _stage;
    [SerializeField] private BlockInfoUI nextBlockInfoUI;
    [SerializeField] private BlockInfoUI holdBlockInfoUI;
    
    [SerializeField] private int _createPositionX = 4;
    [SerializeField] private float _blockDownTime = 1f;
    [SerializeField] private float _lineCompleteTime = .5f;

    private bool _isPlacing;
    
    private TetrisBlock _nextBlock;
    private TetrisBlock _holdBlock;
    private TetrisBlock _currentBlock;
    private int _currentBlockX;
    private int _currentBlockY;
    private int _currentHighestY;
    private int _currentBlockRotation;

    private TetrisBlock.BlockShape CurrentBlockShape => _currentBlock.blockShapes[_currentBlockRotation];
    
    private List<int> _completedLines;
    
    private Player _player = null;
    private List<StageCell> _previousStepCells;
    private Coroutine _stageCoroutine = null;
        
    private void Awake()
    {
        _nextBlock = null;
        _holdBlock = null;
        _completedLines = new List<int>();
        _previousStepCells = new List<StageCell>();
    }

    private void Init()
    {
        _isPlacing = false;
        _currentHighestY = TetrisDefine.TetrisStageRows - 1;
        _completedLines.Clear();
        _previousStepCells.Clear();
        
        foreach (StageCell stageCell in _stage)
            stageCell.Reset();
        
        if (_stageCoroutine != null)
            StopCoroutine(_stageCoroutine);
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }
    
    public void Play()
    {
        Init();
        _stageCoroutine = StartCoroutine(StageUpdateCoroutine());
    }

    public void Stop()
    {
        StopCoroutine(_stageCoroutine);
        _stageCoroutine = null;
    }

    IEnumerator StageUpdateCoroutine()
    {
        // 시작 맵 및 블록 구성
        AddObstacleLines(3);
        CreateBlock();
        
        while (GameManager.Instance.GameState == GameState.Playing)
        {
            yield return new WaitForSeconds(_blockDownTime);

            // 완성된 라인을 확인하는 도중엔 다른 작업 중지
            while (!_isPlacing)
                yield return new WaitForSeconds(0.1f);
            
            if (!TryMoveBlock(Direction.Down))
            {
                // 블록을 해당 위치에 확정
                foreach (StageCell stageCell in _previousStepCells)
                    stageCell.SetBlock(_currentBlock);

                // 추후 라인을 제거할 때 확인 범위를 좁히기 위해 가장 높은 블록의 y 값 저장
                if (_currentHighestY > _currentBlockY)
                {
                    _currentHighestY = _currentBlockY;

                    // 블록이 천장에 닿아 게임 오버 
                    if (_currentHighestY == 0)
                    {
                        GameManager.Instance.SetLose(_player);
                        break;
                    }
                }
                
                _previousStepCells.Clear();

                // 블록이 바닥이나 다른 블록에 닿았을 경우 라인 완성 검사
                CheckCompleteAndNewLines();
            }
        }
    }

    #region 블록 생성 및 이동, 관리
    private void CreateBlock(TetrisBlock targetBlock = null)
    {
        // 파라미터로 지정된 블록이 없다면 다음 블록이나 랜덤 블록 사용
        if (targetBlock == null)
            targetBlock = _nextBlock ?? TetrisDefine.Instance.GetRandomTetrisBlock();
        
        _currentBlock = targetBlock;
        _nextBlock = TetrisDefine.Instance.GetRandomTetrisBlock();
        nextBlockInfoUI.BlockImage.sprite = _nextBlock.blockImage;
        
        _currentBlockX = _createPositionX;
        _currentBlockY = 0;
        _currentBlockRotation = 0;
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

        if (CheckCollision(_currentBlock, nextBlockX, nextBlockY, _currentBlockRotation))
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
        for (int i = 0; i < CurrentBlockShape.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (_currentBlockY + i);

            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!CurrentBlockShape.shape[(i * TetrisDefine.TetrisBlockCols) + j])
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

    private bool CheckCollision(TetrisBlock block, int blockPositionX, int blockPositionY, int blockRotation)
    {
        TetrisBlock.BlockShape blockShape = block.blockShapes[blockRotation];
        
        // 검사할 위치가 스테이지를 벗어나는지 확인
        if (blockPositionX < 0 || blockPositionX + blockShape.width > TetrisDefine.TetrisStageCols ||
            blockPositionY < 0 || blockPositionY + blockShape.height > TetrisDefine.TetrisStageRows)
            return true;

        // 검사할 위치가 다른 블록에 닿는지 확인
        for (int i = 0; i < blockShape.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (blockPositionY + i);
            for (int j = 0; j < blockShape.width; j++)
            {
                if (!blockShape.shape[(i * TetrisDefine.TetrisBlockCols) + j])
                    continue;

                StageCell stageCell = _stage[offset + blockPositionX + j];
                if (stageCell.IsBlocked)
                    return true;
            }
        }

        return false;
    }
    
    public void RotateBlock()
    {
        int nextRotation = (_currentBlockRotation + 1) % TetrisDefine.TetrisBlockMaxRotation;
        TetrisBlock.BlockShape rotatedBlockShape = _currentBlock.blockShapes[nextRotation];
        
        int rotatedBlockX = _currentBlockX;
        int rotatedBlockY = _currentBlockY;

        // 벽 가까이에서 회전할 경우 위치 보정
        if (rotatedBlockX + rotatedBlockShape.width >= TetrisDefine.TetrisStageCols)
            rotatedBlockX = TetrisDefine.TetrisStageCols - rotatedBlockShape.width;

        if (rotatedBlockY + rotatedBlockShape.height >= TetrisDefine.TetrisStageRows)
            rotatedBlockY = TetrisDefine.TetrisStageRows - rotatedBlockShape.height;
        
        // 회전 위치에 대해 충돌 검사 진행
        if (CheckCollision(_currentBlock, rotatedBlockX, rotatedBlockY, nextRotation))
            return;
        
        // 현재 블록 정보를 회전한 블록으로 업데이트
        _currentBlockX = rotatedBlockX;
        _currentBlockY = rotatedBlockY;
        _currentBlockRotation = nextRotation;

        ClearPreviousStep();
        DrawStep();
    }

    public void HoldBlock()
    {
        if (!_isPlacing)
            return;

        ClearPreviousStep();
        
        if (_holdBlock == null)
        {
            _holdBlock = _currentBlock;
            CreateBlock();
        }
        else
        {
            (_currentBlock, _holdBlock) = (_holdBlock, _currentBlock);

            // 블록을 변경했을 때 충돌이 발생한다면 Y 위치 조정
            while (CheckCollision(_currentBlock, _currentBlockX, _currentBlockY, _currentBlockRotation) && _currentBlockY > 0)
                --_currentBlockY;
            
            DrawStep();
        }

        holdBlockInfoUI.BlockImage.sprite = _holdBlock.blockImage;
    }
    
    public void DropBlock()
    {
        // 제일 밑에서부터 블록을 배치할 수 있는 위치 탐색
        for (int i = TetrisDefine.TetrisStageRows - CurrentBlockShape.height; i > _currentBlockY + CurrentBlockShape.height; i--)
        {
            if (!CheckCollision(_currentBlock, _currentBlockX, i, _currentBlockRotation))
            {
                _currentBlockY = i;
                ClearPreviousStep();
                DrawStep();
            }
        }
    }
    #endregion
    
    #region 라인 검사 및 관리
    
    private void CheckCompleteAndNewLines()
    {
        _isPlacing = false;
        StartCoroutine(CheckCompleteAndNewLinesCoroutine());
    }

    IEnumerator CheckCompleteAndNewLinesCoroutine()
    {
        // 완성된 라인 존재 여부 확인
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

        // 완성된 라인이 있다면 제거 후 스테이지 정리
        if (_completedLines.Count > 0)
        {
            CompleteLines();
            yield return new WaitForSeconds(_lineCompleteTime);
            AdjustLines();
            yield return new WaitForSeconds(_lineCompleteTime);
        }

        // 블록을 쌓는 동안 받은 공격이 있다면 모두 처리
        int obstacleLineNum = 0;
        while (_player.HitQueue.Count > 0)
            obstacleLineNum += _player.HitQueue.Dequeue();
        
        AddObstacleLines(obstacleLineNum);
        
        // 다음 블록 생성
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
        
        // 완성한 줄이 2줄 이상일 경우 완성한 줄 - 1개의 장애물 블록을 상대방에게 공격
        if (_completedLines.Count > 1)
            GameManager.Instance.AttackTo(_player, _completedLines.Count - 1);
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
    
    public void AddObstacleLines(int num)
    {
        if (num == 0)
            return;
        
        // 생성할 장애물 블록 높이만큼 기존 블록들을 모두 위로 이동
        for (int i = 0; i < TetrisDefine.TetrisStageRows; i++)
        {
            int from = i;
            int to = from - num;

            if (to < 0)
                continue;
            
            SwapLine(from, to);
        }

        // 장애물 블록 생성
        int holeIndex = Random.Range(0, TetrisDefine.TetrisStageCols - 1);
        for (int i = TetrisDefine.TetrisStageRows - num; i < TetrisDefine.TetrisStageRows; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * i;
            for (int j = 0; j < TetrisDefine.TetrisStageCols; j++)
            {
                if (j == holeIndex)
                    continue;

                StageCell stageCell = _stage[offset + j];
                stageCell.SetBlock(TetrisDefine.ObstacleCellColor);
            }
        }

        _currentHighestY -= num;
    }
    
    #endregion
}
