using UnityEngine;
using UnityEngine.UI;

public class StageCell : MonoBehaviour
{
    [SerializeField]
    private Image cellImage = null;

    public bool IsBlocked { get; private set; } = false;

    public void Reset()
    {
        cellImage.color = TetrisDefine.NormalCellColor;
        IsBlocked = false;
    }
    
    public void SetBlockTemporarily(TetrisBlock block)
    {
        cellImage.color = block.color;
    }

    public void SetBlock(TetrisBlock block)
    {
        IsBlocked = true;
        cellImage.color = block.color;
    }

    public void SetBlock(Color color)
    {
        IsBlocked = true;
        cellImage.color = color;
    }

    public Color GetBlockColor()
    {
        return cellImage.color;
    }
}
