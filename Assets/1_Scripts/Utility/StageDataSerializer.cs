using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StageDataSerializer
{
    // TODO: 플레이어가 두 명일땐 상관없지만, 여러 명일 경우 데이터 덮어쓰기 가능성 존재
    // TODO: StageCell이 MonoBehaviour라 new로 생성 불가능
    private static StageCell[] deserializedStageArray; 
    
    public static byte[] Serialize(object customObject)
    {
        StageCell[] stageArray = (StageCell[])customObject;
        List<byte> byteList = new List<byte>();
        byte buffer = 0;
        int bitCount = 0;
        
        // 1 StageCell = 1 bit (StageCell.IsBlock) + 4 bit (StageCell.TetrisBlockColor) = 5 bit
        for (int i = 0; i < stageArray.Length; i++)
        {
            if (bitCount == 8)
            {
                byteList.Add(buffer);
                buffer = 0;
                bitCount = 0;
            }
            
            StageCell stageCell = stageArray[i]; 
            if (!stageCell.IsBlocked)
            {
                ++bitCount;
                continue;
            }
            buffer |= (byte)(1 << bitCount++);

            // TetrisBlockColor의 개수가 충분히 적으므로 4비트만 할당
            int tetrisBlockColorIndex = (int)stageCell.TetrisBlockColor;
            for (int j = 0; j < 4; j++)
            {
                int bit = (tetrisBlockColorIndex >> j) & 1;
                buffer |= (byte)(bit << bitCount++);

                if (bitCount == 8)
                {
                    byteList.Add(buffer);
                    buffer = 0;
                    bitCount = 0;
                }
            }
        }
        
        MemoryStream stream = new MemoryStream(sizeof(byte) * byteList.Count);
        stream.Write(byteList.ToArray(), 0, sizeof(byte) * byteList.Count);
        return stream.ToArray();
    }

    public static StageCell[] Deserialize(byte[] bytes)
    {
        int byteCount = 0;
        int bitCount = 0;
        int stageCellCount = TetrisDefine.TetrisStageRows * TetrisDefine.TetrisStageCols;
        
        for (int i = 0; i < stageCellCount; i++)
        {
            byte buffer = bytes[byteCount];
            
            StageCell stageCell = deserializedStageArray[i];
            stageCell.Reset();

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

                int bit = (buffer >> bitCount) & 1;
                tetrisBlockColorIndex |= bit << bitCount++;
            }
            
            stageCell.SetBlock((TetrisBlockColorType)tetrisBlockColorIndex);
        }

        return deserializedStageArray;
    }
}
