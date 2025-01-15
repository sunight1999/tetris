using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuType
{
    Option,
    Countdown
}

public class UIManager : SingletonBehaviour<UIManager>
{
    public bool IsMenuOpened { get; private set; }
    [SerializeField] private UI[] _managedUIs;

    protected override void Awake()
    {
        base.Awake();

        IsMenuOpened = false;
    }

    public void SetVisibility(MenuType menu, bool isVisible)
    {
        foreach (UI ui in _managedUIs)
            ui.SetVisibility(false);
        
        IsMenuOpened = isVisible;
        _managedUIs[(int)menu].SetVisibility(isVisible);

        Time.timeScale = isVisible ? 0f : 1f;

        if (isVisible)
        {
            Time.timeScale = 0f;
            return;
        }
        
        // 게임 도중 팝업 UI 창을 닫으면 카운터다운 UI 표출
        bool isPopUp = menu != MenuType.Countdown && _managedUIs[(int)menu].isPopUp;
        if (isPopUp && GameManager.Instance.GameState == GameState.Playing)
            SetVisibility(MenuType.Countdown, true);
        else
            Time.timeScale = 1f;
    }

    #region 버튼 OnClick 이벤트 콜백
    public void SetOptionVisibility(bool isVisible)
    {
        SetVisibility(MenuType.Option, isVisible);
    }
    #endregion
}
