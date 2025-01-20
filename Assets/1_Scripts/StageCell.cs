using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StageCellData
{
    public bool isBlocked = false;
    public TetrisBlockColorType tetrisBlockColor = TetrisBlockColorType.Normal;
}

[Serializable]
public class StageCell : MonoBehaviour
{
    [SerializeField]
    private Image cellImage = null;

    public bool IsBlocked { get; private set; } = false;
    public bool IsTemporarilyBlocked { get; private set; } = false;
    public TetrisBlockColorType TetrisBlockColor { get; private set; }

    public void Reset()
    {
        cellImage.color = TetrisDefine.tetrisBlockColors[(int)TetrisBlockColorType.Normal];
        IsBlocked = false;
        IsTemporarilyBlocked = false;
    }
    
    public void SetBlockTemporarily(TetrisBlock block)
    {
        IsTemporarilyBlocked = true;
        TetrisBlockColor = block.BlockColor;
        cellImage.color = TetrisDefine.tetrisBlockColors[(int)block.BlockColor];
    }

    public void SetBlock(TetrisBlock block)
    {
        IsTemporarilyBlocked = false;
        IsBlocked = true;
        TetrisBlockColor = block.BlockColor;
        cellImage.color = TetrisDefine.tetrisBlockColors[(int)block.BlockColor];
    }

    public void SetBlock(TetrisBlockColorType color)
    {
        IsTemporarilyBlocked = false;
        IsBlocked = true;
        TetrisBlockColor = color;
        cellImage.color = TetrisDefine.tetrisBlockColors[(int)TetrisBlockColor];
    }
}
