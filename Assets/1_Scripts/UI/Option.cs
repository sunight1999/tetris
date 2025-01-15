using UnityEditor;
using UnityEngine;

public class Option : UI
{
    [SerializeField]
    private GameObject optionPanel = null;
    
    [SerializeField]
    private GameObject blurImage = null;
    
    public override void SetVisibility(bool isVisible)
    {
        optionPanel.SetActive(isVisible);
        blurImage.SetActive(isVisible);
    }

    public void Restart()
    {
        if (GameManager.Instance.GameState == GameState.Playing)
        {
            GameManager.Instance.Play();
        }
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
