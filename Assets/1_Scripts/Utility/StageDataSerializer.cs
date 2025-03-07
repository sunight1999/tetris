using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StageData
{
    public bool isBlocked = false;
    public TetrisBlockColorType blockColorType = TetrisBlockColorType.Normal;

    public void Reset()
    {
        isBlocked = false;
        blockColorType = TetrisBlockColorType.Normal;
    }
}

public class StageDataSerializer
{
    // TODO: 플레이어가 두 명일땐 상관없지만, 여러 명일 경우 데이터 덮어쓰기 가능성 존재
    private static StageData[] deserializedStageDataArray = new StageData[TetrisDefine.TetrisStageCellCount];
    
    public static byte[] Serialize(object customObject)
    {
        StageCell[] stageArray = (StageCell[])customObject;
        List<byte> byteList = new List<byte>();
        byte buffer = 0;
        int bitCount = 0;
        
        // 1 StageCell = 1 bit (StageCell.IsBlock || StageCell.IsTemporarilyBlocked) + 4 bit (StageCell.TetrisBlockColor) = 5 bit
        for (int i = 0; i < stageArray.Length; i++)
        {
            if (bitCount == 8)
            {
                byteList.Add(buffer);
                buffer = 0;
                bitCount = 0;
            }
            
            StageCell stageCell = stageArray[i]; 
            if (!stageCell.IsBlocked && !stageCell.IsTemporarilyBlocked)
            {
                ++bitCount;
                continue;
            }
            
            // 로컬에서 상대방 stage에 대해 충돌 처리를 진행하지 않으므로 IsBlocked와 IsTemporarilyBlocked를 구분하지 않고 모두 1로 설정
            buffer |= (byte)(1 << bitCount++);

            // TetrisBlockColor의 개수가 충분히 적으므로 4비트만 할당
            int tetrisBlockColorIndex = (int)stageCell.TetrisBlockColor;
            for (int j = 0; j < 4; j++)
            {
                if (bitCount == 8)
                {
                    byteList.Add(buffer);
                    buffer = 0;
                    bitCount = 0;
                }
                
                int bit = (tetrisBlockColorIndex >> j) & 1;
                buffer |= (byte)(bit << bitCount++);
            }
        }
        
        // 버퍼에 남은 데이터 flush
        byteList.Add(buffer);
        
        MemoryStream stream = new MemoryStream(sizeof(byte) * byteList.Count);
        stream.Write(byteList.ToArray(), 0, sizeof(byte) * byteList.Count);
        return stream.ToArray();
    }

    public static StageData[] Deserialize(byte[] bytes)
    {
        int byteCount = 0;
        int bitCount = 0;
        int stageCellCount = TetrisDefine.TetrisStageCellCount;

        if (bytes.Length < TetrisDefine.TetrisMinPacketSize)
        {
            Debug.LogWarning($"유효하지 않은 데이터 길이({bytes.Length})입니다.");
            return null;
        }
        
        for (int i = 0; i < stageCellCount; i++)
        {
            byte buffer = bytes[byteCount];
            if (bitCount == 8)
            {
                ++byteCount;
                bitCount = 0;
                buffer = bytes[byteCount];
            }

            StageData stageData = deserializedStageDataArray[i];
            if (stageData == null)
            {
                stageData = new StageData();
                deserializedStageDataArray[i] = stageData;
            }
            stageData.Reset();

            bool isBlocked = ((buffer >> bitCount++) & 1) == 1;
            if (!isBlocked)
            {
                continue;
            }
            
            // 블록이 존재하는 셀인 경우 블록 정보 역직렬화 수행
            int tetrisBlockColorIndex = 0;
            for (int j = 0; j < 4; j++)
            {
                if (bitCount == 8)
                {
                    ++byteCount;
                    bitCount = 0;
                    buffer = bytes[byteCount];
                }

                int bit = (buffer >> bitCount++) & 1;
                tetrisBlockColorIndex |= bit << j;
            }

            stageData.isBlocked = true;
            stageData.blockColorType = (TetrisBlockColorType)tetrisBlockColorIndex;
        }

        return deserializedStageDataArray;
    }
}
