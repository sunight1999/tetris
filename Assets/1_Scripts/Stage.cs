using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class Stage : MonoBehaviourPun, IPunObservable
{
    [SerializeField]
    private StageCell[] stageArray = null;
    
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

    private List<int> completedLineList = new List<int>();
    
    private Player assignedPlayer = null;
    private TetrisPlayer assignedTetrisPlayer = null;
    private List<StageCell> previousStepCellList = new List<StageCell>();
    private Coroutine coroutineStageUpdate = null;

    private Random random = new Random();

    public StageCell[] StageArray => stageArray;
    private TetrisBlock.BlockShape CurrentBlockShape => currentBlock.BlockShapes[currentBlockRotation];

    private TetrisPlayer AssignedTetrisPlayer
    {
        get
        {
            if (assignedTetrisPlayer == null)
            {
                assignedTetrisPlayer = (TetrisPlayer)assignedPlayer.TagObject;
            }

            return assignedTetrisPlayer;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (assignedPlayer == null || GameManager.Instance.GameState != GameState.Playing)
            return;

        if (stream.IsWriting)
        {
            stream.SendNext(nextBlock != null ? (byte)nextBlock.ID : byte.MaxValue);
            stream.SendNext(holdBlock != null ? (byte)holdBlock.ID : byte.MaxValue);
            stream.SendNext(StageDataSerializer.Serialize(stageArray));
        }
        else
        {
            if (assignedPlayer.IsLocal)
                return;
            
            SetNextBlockImage((byte)stream.ReceiveNext());
            SetHoldBlockImage((byte)stream.ReceiveNext());
            StageData[] receivedStageDataArray = StageDataSerializer.Deserialize((byte[])stream.ReceiveNext());
            if (receivedStageDataArray != null)
            {
                ForceUpdateStage(receivedStageDataArray);
            }
        }
    }
    
    public void OnGameStart()
    {
        Init();

        if (assignedPlayer.IsLocal)
        {
            coroutineStageUpdate = StartCoroutine(StageUpdateCoroutine());
        }
    }

    public void OnGameEnd()
    {
        if (coroutineStageUpdate != null)
        {
            StopCoroutine(coroutineStageUpdate);
        }
        coroutineStageUpdate = null;
    }

    public void SetPlayer(Player player)
    {
        assignedPlayer = player;
    }

    private void Init()
    {
        isPlacing = false;
        currentHighestY = TetrisDefine.TetrisStageRows - 1;
        completedLineList.Clear();
        previousStepCellList.Clear();

        foreach (StageCell stageCell in stageArray)
        {
            stageCell.Reset();
        }

        if (coroutineStageUpdate != null)
        {
            StopCoroutine(coroutineStageUpdate);
        }
    }

    private void ForceUpdateStage(StageData[] newStageDataArray)
    {
        if (stageArray.Length != newStageDataArray.Length)
        {
            Debug.LogWarning($"전달받은 스테이지 정보가 유효하지 않습니다. (newStageArray.Length={newStageDataArray.Length})");
            return;
        }
        
        for (int i = 0; i < stageArray.Length; i++)
        {
            StageCell stageCell = stageArray[i];
            
            stageCell.Reset();
            if (newStageDataArray[i].isBlocked)
            {
                stageCell.SetBlock(newStageDataArray[i].blockColorType);
            }
        }
    }

    private IEnumerator StageUpdateCoroutine()
    {
        // 시작 맵 및 블록 구성
        //AddObstacleLines(3);

        if (!CreateBlock())
        {
            GameManager.Instance.SetLose(assignedPlayer);
            yield break;
        }
        
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
                    if (currentHighestY <= 0)
                    {
                        GameManager.Instance.SetLose(assignedPlayer);
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
    
    public void TryRotateBlock()
    {
        if (!isPlacing)
            return;
        
        int nextRotation = (currentBlockRotation + 1) % TetrisDefine.TetrisBlockMaxRotation;
        TetrisBlock.BlockShape rotatedBlockShape = currentBlock.BlockShapes[nextRotation];
        
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

        SetHoldBlockImage(holdBlock.ID);
    }
    
    public void DropBlock()
    {
        if (!isPlacing)
            return;
        
        // 현재 위치에서부터 블록을 배치할 수 있는 위치 탐색
        int i = currentBlockY;
        for (; i <= TetrisDefine.TetrisStageRows - CurrentBlockShape.height; i++)
        {
            if (CheckCollision(currentBlock, currentBlockX, i, currentBlockRotation))
            {
                break;
            }
        }
        
        // i가 충돌이 감지된 위치이므로 -1 수행
        currentBlockY = i - 1;
        
        ClearPreviousStep();
        DrawStep();
    }

    private bool CreateBlock(TetrisBlock targetBlock = null)
    {
        // 파라미터로 지정된 블록이 없다면 다음 블록이나 랜덤 블록 사용
        if (targetBlock == null)
        {
            targetBlock = nextBlock ?? TetrisDefine.Instance.GetRandomTetrisBlock();
        }
        
        currentBlock = targetBlock;
        nextBlock = TetrisDefine.Instance.GetRandomTetrisBlock();
        SetNextBlockImage(nextBlock.ID);
        
        currentBlockX = createPositionX;
        currentBlockY = 0;
        currentBlockRotation = 0;
        isPlacing = true;

        if (CheckCollision(currentBlock, currentBlockX, currentBlockY, currentBlockRotation))
            return false;
        
        DrawStep();
        return true;
    }

    private void DrawStep()
    {
        // 현재 블록의 위치를 스테이지에 표시
        for (int i = 0; i < CurrentBlockShape.height; i++)
        {
            int offset = TetrisDefine.TetrisStageCols * (currentBlockY + i);
            
            // 블록이 -1 지점에서 생성되므로 해당 시점에 드롭을 수행하면 offset이 -가 될 수도 있음
            if (offset < 0)
            {
                continue;
            }
            
            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!CurrentBlockShape.shape[(i * TetrisDefine.TetrisBlockCols) + j])
                {
                    continue;
                }

                StageCell stageCell = stageArray[offset + currentBlockX + j];
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
        TetrisBlock.BlockShape blockShape = block.BlockShapes[blockRotation];
        
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

                StageCell stageCell = stageArray[offset + blockPositionX + j];
                if (stageCell.IsBlocked)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void SetNextBlockImage(int tetrisBlockIndex)
    {
        if (tetrisBlockIndex >= TetrisDefine.Instance.tetrisBlockArray.Length)
            return;
        
        nextBlockInfoUI.BlockImage.sprite = TetrisDefine.Instance.tetrisBlockArray[tetrisBlockIndex].BlockImage;
    }
    
    private void SetHoldBlockImage(int tetrisBlockIndex)
    {
        if (tetrisBlockIndex >= TetrisDefine.Instance.tetrisBlockArray.Length)
            return;
        
        holdBlockInfoUI.BlockImage.sprite = TetrisDefine.Instance.tetrisBlockArray[tetrisBlockIndex].BlockImage;
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

                StageCell stageCell = stageArray[offset + j];
                stageCell.SetBlock(TetrisBlockColorType.Obstacle);
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
                StageCell stageCell = stageArray[offset + j];
                if (!stageCell.IsBlocked)
                {
                    isCompleted = false;
                    break;
                }
            }

            if (isCompleted)
            {
                completedLineList.Add(i);
            }
        }

        // 완성된 라인이 있다면 제거 후 스테이지 정리
        if (completedLineList.Count > 0)
        {
            CompleteLines();
            yield return new WaitForSeconds(lineCompleteTime);
            AdjustLines();
            yield return new WaitForSeconds(lineCompleteTime);
        }

        // 블록을 쌓는 동안 받은 공격이 있다면 모두 처리
        int obstacleLineNum = 0;
        while (AssignedTetrisPlayer.HitQueue.Count > 0)
        {
            obstacleLineNum += AssignedTetrisPlayer.HitQueue.Dequeue();
        }
        
        AddObstacleLines(obstacleLineNum);
        
        // 다음 블록 생성
        CreateBlock();
    }

    private void CompleteLines()
    {
        foreach (int line in completedLineList)
        {
            int offset = TetrisDefine.TetrisStageCols * line;
            for (int i = 0; i < TetrisDefine.TetrisStageCols; i++)
            {
                StageCell stageCell = stageArray[offset + i];
                stageCell.Reset();
            }
        }
        
        // 완성한 줄이 2줄 이상일 경우 완성한 줄 - 1개의 장애물 블록을 상대방에게 공격
        if (completedLineList.Count > 1)
        {
            AssignedTetrisPlayer.Attack(completedLineList.Count - 1);
        }
    }
    
    private void AdjustLines()
    {
        if (completedLineList.Count == 0)
            return;

        int adjustRangeBottom = completedLineList[0];
        int adjustRangeTop = currentHighestY;
        
        // 블록이 있는 라인 중 가장 윗 라인이 완성되면 해당 라인 위로는 블록이 없으므로 블록 검사 범위 축소
        for (int i = completedLineList.Count - 1; i >= 0; i--)
        {
            if (completedLineList[i] == adjustRangeTop)
            {
                completedLineList.Remove(i);
                --adjustRangeTop;
            }
        }

        int headLine = adjustRangeBottom;
        int tailLine = adjustRangeBottom - 1;
        while (tailLine >= adjustRangeTop)
        {
            if (completedLineList.Contains(tailLine))
            {
                --tailLine;
                continue;
            }

            SwapLine(tailLine, headLine);

            --tailLine;
            --headLine;
        }
        
        currentHighestY += completedLineList.Count;
        completedLineList.Clear();
    }

    private void SwapLine(int fromLine, int toLine)
    {
        for (int i = 0; i < TetrisDefine.TetrisStageCols; i++)
        {
            StageCell fromStageCell = stageArray[TetrisDefine.TetrisStageCols * fromLine + i];
            StageCell toStageCell = stageArray[TetrisDefine.TetrisStageCols * toLine + i];

            if (!fromStageCell.IsBlocked)
            {
                continue;
            }
            
            fromStageCell.Reset();
            toStageCell.SetBlock(fromStageCell.TetrisBlockColor);
        }
    }
#endregion
}
