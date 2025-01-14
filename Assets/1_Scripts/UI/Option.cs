using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Option : UI
{
    [SerializeField] private GameObject _optionPanel;
    [SerializeField] private GameObject _blurImage;
    
    public override void SetVisibility(bool isVisible)
    {
        _optionPanel.SetActive(isVisible);
        _blurImage.SetActive(isVisible);
    }

    public void Restart()
    {
        if (GameManager.Instance.GameState == GameState.Playing)
            GameManager.Instance.Play();
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
