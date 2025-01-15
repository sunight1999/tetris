using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Stage : MonoBehaviour
{
    [SerializeField]
    private StageCell[] stage = null;
    
    [SerializeField]
    private BlockInfoUI nextBlockInfoUI = null;
    
    [SerializeField]
    private BlockInfoUI holdBlockInfoUI = null;
    
    [SerializeField]
    private int createPositionX = 4;
    
    [SerializeField]
    private float blockDownTime = 0.25f;
    
    [SerializeField]
    private float lineCompleteTime = .5f;
    
    private bool isPlacing = false;
    
    private TetrisBlock nextBlock = null;
    private TetrisBlock holdBlock = null;
    private TetrisBlock currentBlock = null;
    private int currentBlockX = 0;
    private int currentBlockY = 0;
    private int currentHighestY = 0;
    private int currentBlockRotation = 0;

    private List<int> completedLines = new List<int>();
    
    private Player player = null;
    private List<StageCell> previousStepCellList = new List<StageCell>();
    private Coroutine coroutineStageUpdate = null;

    private Random random = new Random();
    
    private TetrisBlock.BlockShape CurrentBlockShape => currentBlock.blockShapes[currentBlockRotation];

    public void Init()
    {
        isPlacing = false;
        currentHighestY = TetrisDefine.TetrisStageRows - 1;
        completedLines.Clear();
        previousStepCellList.Clear();

        foreach (StageCell stageCell in stage)
        {
            stageCell.Reset();
        }

        if (coroutineStageUpdate != null)
        {
            StopCoroutine(coroutineStageUpdate);
        }
    }

    public void SetPlayer(Player inPlayer)
    {
        player = inPlayer;
    }
    
    public void Play()
    {
        Init();
        coroutineStageUpdate = StartCoroutine(StageUpdateCoroutine());
    }

    public void Stop()
    {
        StopCoroutine(coroutineStageUpdate);
        coroutineStageUpdate = null;
    }

    private IEnumerator StageUpdateCoroutine()
    {
        // 시작 맵 및 블록 구성
        AddObstacleLines(3);
        CreateBlock();
        
        while (GameManager.Instance.GameState == GameState.Playing)
        {
            yield return new WaitForSeconds(blockDownTime);

            // 완성된 라인을 확인하는 도중엔 다른 작업 중지
            while (!isPlacing)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            if (!TryMoveBlock(Direction.Down))
            {
                // 블록을 해당 위치에 확정
                foreach (StageCell stageCell in previousStepCellList)
                {
                    stageCell.SetBlock(currentBlock);
                }

                // 추후 라인을 제거할 때 확인 범위를 좁히기 위해 가장 높은 블록의 y 값 저장
                if (currentHighestY > currentBlockY)
                {
                    currentHighestY = currentBlockY;

                    // 블록이 천장에 닿아 게임 오버 
                    if (currentHighestY == 0)
                    {
                        GameManager.Instance.SetLose(player);
                        break;
                    }
                }
                
                previousStepCellList.Clear();

                // 블록이 바닥이나 다른 블록에 닿았을 경우 라인 완성 검사
                CheckCompleteAndNewLines();
            }
        }
    }

#region 블록 생성 및 이동, 관리
    public bool TryMoveBlock(Direction direction)
    {
        if (!isPlacing)
            return true;

        int nextBlockX = currentBlockX;
        int nextBlockY = currentBlockY;

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

        if (CheckCollision(currentBlock, nextBlockX, nextBlockY, currentBlockRotation))
            return false;

        currentBlockX = nextBlockX;
        currentBlockY = nextBlockY;

        ClearPreviousStep();
        DrawStep();
        return true;
    }
    
    public void RotateBlock()
    {
        int nextRotation = (currentBlockRotation + 1) % TetrisDefine.TetrisBlockMaxRotation;
        TetrisBlock.BlockShape rotatedBlockShape = currentBlock.blockShapes[nextRotation];
        
        int rotatedBlockX = currentBlockX;
        int rotatedBlockY = currentBlockY;

        // 벽 가까이에서 회전할 경우 위치 보정
        if (rotatedBlockX + rotatedBlockShape.width >= TetrisDefine.TetrisStageCols)
        {
            rotatedBlockX = TetrisDefine.TetrisStageCols - rotatedBlockShape.width;
        }

        if (rotatedBlockY + rotatedBlockShape.height >= TetrisDefine.TetrisStageRows)
        {
            rotatedBlockY = TetrisDefine.TetrisStageRows - rotatedBlockShape.height;
        }
        
        // 회전 위치에 대해 충돌 검사 진행
        if (CheckCollision(currentBlock, rotatedBlockX, rotatedBlockY, nextRotation))
            return;
        
        // 현재 블록 정보를 회전한 블록으로 업데이트
        currentBlockX = rotatedBlockX;
        currentBlockY = rotatedBlockY;
        currentBlockRotation = nextRotation;

        ClearPreviousStep();
        DrawStep();
    }

    public void HoldBlock()
    {
        if (!isPlacing)
            return;

        ClearPreviousStep();
        
        if (holdBlock == null)
        {
            holdBlock = currentBlock;
            CreateBlock();
        }
        else
        {
            (currentBlock, holdBlock) = (holdBlock, currentBlock);

            // 블록을 변경했을 때 충돌이 발생한다면 Y 위치 조정
            while (CheckCollision(currentBlock, currentBlockX, currentBlockY, currentBlockRotation) && currentBlockY > 0)
            {
                --currentBlockY;
            }
            
            DrawStep();
        }

        holdBlockInfoUI.BlockImage.sprite = holdBlock.blockImage;
    }
    
    public void DropBlock()
    {
        // 제일 밑에서부터 블록을 배치할 수 있는 위치 탐색
        for (int i = TetrisDefine.TetrisStageRows - CurrentBlockShape.height; i > currentBlockY + CurrentBlockShape.height; i--)
        {
            if (!CheckCollision(currentBlock, currentBlockX, i, currentBlockRotation))
            {
                currentBlockY = i;
                ClearPreviousStep();
                DrawStep();
            }
        }
    }

    private void CreateBlock(TetrisBlock targetBlock = null)
    {
        // 파라미터로 지정된 블록이 없다면 다음 블록이나 랜덤 블록 사용
        if (targetBlock == null)
        {
            targetBlock = nextBlock ?? TetrisDefine.Instance.GetRandomTetrisBlock();
        }
        
        currentBlock = targetBlock;
        nextBlock = TetrisDefine.Instance.GetRandomTetrisBlock();
        nextBlockInfoUI.BlockImage.sprite = nextBlock.blockImage;
        
        currentBlockX = createPositionX;
        currentBlockY = 0;
        currentBlockRotation = 0;
        isPlacing = true;

        DrawStep();
    }

    private void DrawStep()
    {
        // 현재 블록의 위치를 스테이지에 표시
        for (int i = 0; i < CurrentBlockShape.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (currentBlockY + i);

            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!CurrentBlockShape.shape[(i * TetrisDefine.TetrisBlockCols) + j])
                {
                    continue;
                }

                StageCell stageCell = stage[offset + currentBlockX + j];
                stageCell.SetBlockTemporarily(currentBlock);
                previousStepCellList.Add(stageCell);
            }
        }
    }

    private void ClearPreviousStep()
    {
        // 이전 스텝의 블록 잔상 제거
        foreach (StageCell stageCell in previousStepCellList)
        {
            stageCell.Reset();
        }

        previousStepCellList.Clear();
    }

    private bool CheckCollision(TetrisBlock block, int blockPositionX, int blockPositionY, int blockRotation)
    {
        TetrisBlock.BlockShape blockShape = block.blockShapes[blockRotation];
        
        // 검사할 위치가 스테이지를 벗어나는지 확인
        if (blockPositionX < 0 || blockPositionX + blockShape.width > TetrisDefine.TetrisStageCols ||
            blockPositionY < 0 || blockPositionY + blockShape.height > TetrisDefine.TetrisStageRows)
        {
            return true;
        }

        // 검사할 위치가 다른 블록에 닿는지 확인
        for (int i = 0; i < blockShape.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (blockPositionY + i);
            for (int j = 0; j < blockShape.width; j++)
            {
                if (!blockShape.shape[(i * TetrisDefine.TetrisBlockCols) + j])
                {
                    continue;
                }

                StageCell stageCell = stage[offset + blockPositionX + j];
                if (stageCell.IsBlocked)
                {
                    return true;
                }
            }
        }

        return false;
    }
#endregion
    
#region 라인 검사 및 관리
    public void AddObstacleLines(int obstacleLineNum)
    {
        if (obstacleLineNum == 0)
            return;
            
        // 생성할 장애물 블록 높이만큼 기존 블록들을 모두 위로 이동
        for (int i = 0; i < TetrisDefine.TetrisStageRows; i++)
        {
            int fromLine = i;
            int toLine = fromLine - obstacleLineNum;

            if (toLine < 0)
            {
                continue;
            }
                
            SwapLine(fromLine, toLine);
        }

        // 장애물 블록 생성
        int holeIndex = random.Next(0, TetrisDefine.TetrisStageCols);
        for (int i = TetrisDefine.TetrisStageRows - obstacleLineNum; i < TetrisDefine.TetrisStageRows; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * i;
            for (int j = 0; j < TetrisDefine.TetrisStageCols; j++)
            {
                if (j == holeIndex)
                {
                    continue;
                }

                StageCell stageCell = stage[offset + j];
                stageCell.SetBlock(TetrisDefine.ObstacleCellColor);
            }
        }

        currentHighestY -= obstacleLineNum;
    }

    private void CheckCompleteAndNewLines()
    {
        isPlacing = false;
        StartCoroutine(CheckCompleteAndNewLinesCoroutine());
    }

    private IEnumerator CheckCompleteAndNewLinesCoroutine()
    {
        // 완성된 라인 존재 여부 확인
        for (int i = TetrisDefine.TetrisStageRows - 1; i >= currentHighestY; i--)
        {
            bool isCompleted = true;
            int offset = TetrisDefine.TetrisStageCols * i;
            for (int j = 0; j < TetrisDefine.TetrisStageCols; j++)
            {
                StageCell stageCell = stage[offset + j];
                if (!stageCell.IsBlocked)
                {
                    isCompleted = false;
                    break;
                }
            }

            if (isCompleted)
            {
                completedLines.Add(i);
            }
        }

        // 완성된 라인이 있다면 제거 후 스테이지 정리
        if (completedLines.Count > 0)
        {
            CompleteLines();
            yield return new WaitForSeconds(lineCompleteTime);
            AdjustLines();
            yield return new WaitForSeconds(lineCompleteTime);
        }

        // 블록을 쌓는 동안 받은 공격이 있다면 모두 처리
        int obstacleLineNum = 0;
        while (player.HitQueue.Count > 0)
        {
            obstacleLineNum += player.HitQueue.Dequeue();
        }
        
        AddObstacleLines(obstacleLineNum);
        
        // 다음 블록 생성
        CreateBlock();
    }

    private void CompleteLines()
    {
        foreach (int line in completedLines)
        {
            int offset = TetrisDefine.TetrisStageCols * line;
            for (int i = 0; i < TetrisDefine.TetrisStageCols; i++)
            {
                StageCell stageCell = stage[offset + i];
                stageCell.Reset();
            }
        }
        
        // 완성한 줄이 2줄 이상일 경우 완성한 줄 - 1개의 장애물 블록을 상대방에게 공격
        if (completedLines.Count > 1)
        {
            GameManager.Instance.AttackTo(player, completedLines.Count - 1);
        }
    }
    
    private void AdjustLines()
    {
        if (completedLines.Count == 0)
            return;

        int adjustRangeBottom = completedLines[0];
        int adjustRangeTop = currentHighestY;
        
        // 블록이 있는 라인 중 가장 윗 라인이 완성되면 해당 라인 위로는 블록이 없으므로 블록 검사 범위 축소
        for (int i = completedLines.Count - 1; i >= 0; i--)
        {
            if (completedLines[i] == adjustRangeTop)
            {
                completedLines.Remove(i);
                --adjustRangeTop;
            }
        }

        int headLine = adjustRangeBottom;
        int tailLine = adjustRangeBottom - 1;
        while (tailLine >= adjustRangeTop)
        {
            if (completedLines.Contains(tailLine))
            {
                --tailLine;
                continue;
            }

            SwapLine(tailLine, headLine);

            --tailLine;
            --headLine;
        }
        
        currentHighestY += completedLines.Count;
        completedLines.Clear();
    }

    private void SwapLine(int fromLine, int toLine)
    {
        for (int i = 0; i < TetrisDefine.TetrisStageCols; i++)
        {
            StageCell fromStageCell = stage[TetrisDefine.TetrisStageCols * fromLine + i];
            StageCell toStageCell = stage[TetrisDefine.TetrisStageCols * toLine + i];

            if (!fromStageCell.IsBlocked)
            {
                continue;
            }
            
            Color color = fromStageCell.GetBlockColor();
            fromStageCell.Reset();
            toStageCell.SetBlock(color);
        }
    }
#endregion
}
