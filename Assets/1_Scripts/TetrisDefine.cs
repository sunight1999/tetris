using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TetrisBlock
{
    public string name;
    public Color color;
    public bool[] blockShape;
    public int height;
    public int width;
    
    public TetrisBlock(int row = TetrisDefine.TetrisBlockRows, int col = TetrisDefine.TetrisBlockCols)
    {
        name = "block";
        color = Color.gray;
        blockShape = new bool[row * col];
        height = -1;
        width = -1;
    }

    public void CaculateSize()
    {
        for (int i = 0; i < TetrisDefine.TetrisBlockRows; i++)
        {
            int offset = TetrisDefine.TetrisBlockCols * i;
            for (int j = 0; j < TetrisDefine.TetrisBlockCols; j++)
            {
                if (!blockShape[offset + j])
                    continue;

                height = i;
                width = width < j ? j : width;
            }
        }

        // 0부터 시작하는 인덱스가 아니라 실제 길이를 원하므로 1씩 추가
        ++height;
        ++width;
    }

    public void Rotate()
    {
        bool[] newBlockShape = new bool[TetrisDefine.TetrisBlockRows * TetrisDefine.TetrisBlockCols];

        for (int i = 0; i < height; i++)
        {
            int offset = TetrisDefine.TetrisBlockCols * i;
            for (int j = 0; j < width; j++)
            {
                int targetIndex = (TetrisDefine.TetrisBlockCols * j) + (height - i - 1);
                newBlockShape[targetIndex] = blockShape[offset + j];
            }
        }

        newBlockShape.CopyTo(blockShape, 0);
        CaculateSize();
    }
}

public class TetrisDefine : SingletonBehaviour<TetrisDefine>
{
    public const int TetrisStageRows = 20;
    public const int TetrisStageCols = 10;
    public const int TetrisBlockRows = 4;
    public const int TetrisBlockCols = 4;

    public static readonly Color NormalCellColor = new Color(0f, 0f, 0f, 0.5f);
    
    public TetrisBlock[] tetrisBlocks;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < tetrisBlocks.Length; i++)
        {
            TetrisBlock block = tetrisBlocks[i];

            if (block.blockShape.Length != TetrisDefine.TetrisBlockRows * TetrisDefine.TetrisBlockCols)
                Debug.LogWarning($"{block.name} 블록의 크기가 잘못되었습니다. {TetrisDefine.TetrisBlockRows}x{TetrisDefine.TetrisBlockCols} 크기인지 확인해주세요.");

            block.CaculateSize();
            tetrisBlocks[i] = block;
        }
    }
}
