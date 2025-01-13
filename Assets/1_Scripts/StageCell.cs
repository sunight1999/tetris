using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageCell : MonoBehaviour
{
    [SerializeField] private Image _cellImage;
    
    public bool IsBlocked { get; private set; }

    public void Reset()
    {
        _cellImage.color = TetrisDefine.NormalCellColor;
        IsBlocked = false;
    }
    
    public void SetBlock(TetrisBlock block)
    {
        IsBlocked = true;
        _cellImage.color = block.color;
    }
}
