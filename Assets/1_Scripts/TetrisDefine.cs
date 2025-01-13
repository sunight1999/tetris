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
    
    public TetrisBlock(int row = TetrisDefine.TetrisBlockRows, int col = TetrisDefine.TetrisBlockCols)
    {
        name = "block";
        color = Color.gray;
        blockShape = new bool[row * col];
        height = -1;
    }

    public void SetHeight(int inHeight)
    {
        height = inHeight;
    }
}

public class TetrisDefine : SingletonBehaviour<TetrisDefine>
{
    public const int TetrisStageRows = 20;
    public const int TetrisStageCols = 10;
    public const int TetrisBlockRows = 4;
    public const int TetrisBlockCols = 3;

    public static readonly Color NormalCellColor = new Color(0f, 0f, 0f, 0.5f);
    
    public TetrisBlock[] tetrisBlocks;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < tetrisBlocks.Length; i++)
        {
            TetrisBlock block = tetrisBlocks[i];
            int lastCellIndex = -1;
            for (int j = 0; j < block.blockShape.Length; j++)
            {
                if (block.blockShape[j])
                    lastCellIndex = j;
            }

            if (lastCellIndex == -1)
                Debug.LogWarning($"{block.name} 블록의 모양이 제대로 설정되지 않았습니다.");
            else
                block.SetHeight((int)(lastCellIndex / TetrisBlockCols) + 1);

            tetrisBlocks[i] = block;
        }
    }
}
