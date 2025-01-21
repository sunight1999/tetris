using System;
using ExitGames.Client.Photon;
using Photon.Pun;
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
        GameManager.Instance.StartGameEvent -= OnStartGame;
    }

    public void OnStartGame()
    {
        Init();
    }

    public void Init()
    {
        foreach (UI ui in managedUIArray)
        {
            ui.SetVisibility(false);
        }
    }

    public void SetVisibility(MenuType menu, bool isVisible)
    {
        Init();
        
        managedUIArray[(int)menu].SetVisibility(isVisible);
    }
}
