using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlockInfo : MonoBehaviour
{
    [SerializeField] private Image _blockImage;
    [SerializeField] private TextMeshProUGUI _infoText;

    public void SetBlock(TetrisBlock block)
    {
        _blockImage.sprite = TetrisDefine.Instance.tetrisBlockImages[block.id];
    }
    
    public void SetInfoText(string text)
    {
        _infoText.text = text;
    }
}
