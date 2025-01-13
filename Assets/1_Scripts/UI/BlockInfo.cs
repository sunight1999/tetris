using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockInfo : MonoBehaviour
{
    [SerializeField] private Image _blockImage;
    [SerializeField] private TextMeshProUGUI _infoText;

    public void SetBlock(TetrisBlock block)
    {
        
    }
    
    public void SetInfoText(string text)
    {
        _infoText.text = text;
    }
}
