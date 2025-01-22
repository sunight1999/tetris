using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField]
    private UI[] managedUIArray = null;

    private void Start()
    {
        GameManager.Instance.StartGameEvent += OnStartGame;
    }

    private void OnDestroy()
    {
        if (GameManager.IsAlive)
        {
            GameManager.Instance.StartGameEvent -= OnStartGame;
        }
    }

    public void OnStartGame()
    {
        Reset();
    }

    public void Reset()
    {
        foreach (UI ui in managedUIArray)
        {
            ui.SetVisibility(false);
        }
    }

    public void SetVisibility(MenuType menu, bool isVisible)
    {
        Reset();
        
        managedUIArray[(int)menu].SetVisibility(isVisible);
    }
}
