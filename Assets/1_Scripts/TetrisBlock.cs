using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TetrisBlock
{
    public int id = -1;
    public string name = string.Empty;
    public Color color = Color.gray;
    public Sprite blockImage = null;
    public List<BlockShape> blockShapes = null;
    
    public void CaculateAllRotations()
    {
        bool isValidShape = blockShapes[0].shape.Length == TetrisDefine.TetrisBlockRows * TetrisDefine.TetrisBlockCols; 
        if (!isValidShape)
        {
            Debug.LogWarning($"{name} 블록의 모양이 제대로 정의되어 있지 않습니다.");
            return;
        }
        
        blockShapes[0].CaculateSize();
        
        // 회전시킨 블록 모양을 미리 계산해 저장
        for (int i = 1; i < TetrisDefine.TetrisBlockMaxRotation; i++)
        {
            blockShapes.Add(blockShapes[i - 1].Rotate());
        }
    }
    
    [Serializable]
    public class BlockShape
    {
        public bool[] shape = null;
        [HideInInspector] public int height = -1;
        [HideInInspector] public int width = -1;
    
        public BlockShape()
        {
            shape = new bool[TetrisDefine.TetrisBlockRows * TetrisDefine.TetrisBlockCols];
        }
        
        /// <summary>
        /// 블록의 가로 길이와 세로 길이를 계산
        /// </summary>
        public void CaculateSize()
        {
            height = -1;
            width = -1;
    
            for (int i = 0; i < TetrisDefine.TetrisBlockRows; i++)
            {
                int offset = TetrisDefine.TetrisBlockCols * i;
                for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
                {
                    if (!shape[offset + j])
                    {
                        continue;
                    }
    
                    height = i;
                    width = width < j ? j : width;
                }
            }
    
            // 0부터 시작하는 인덱스가 아니라 실제 길이를 원하므로 1씩 추가
            ++height;
            ++width;
        }
    
        /// <summary>
        /// 블록을 시계 방향으로 90도 회전
        /// </summary>
        public BlockShape Rotate()
        {
            BlockShape newBlockShape = new BlockShape();
            
            for (int i = 0; i < height; i++)
            {
                int offset = TetrisDefine.TetrisBlockCols * i;
                for (int j = 0; j < width; j++)
                {
                    int targetIndex = (TetrisDefine.TetrisBlockCols * j) + (height - i - 1);
                    newBlockShape.shape[targetIndex] = shape[offset + j];
                }
            }
            
            newBlockShape.CaculateSize();
            return newBlockShape;
        }
    }
}
