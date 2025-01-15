using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField]
    private UI[] managedUIArray = null;

    public bool IsMenuOpened { get; private set; } = false;

    public void SetVisibility(MenuType menu, bool isVisible)
    {
        foreach (UI ui in managedUIArray)
            ui.SetVisibility(false);
        
        IsMenuOpened = isVisible;
        managedUIArray[(int)menu].SetVisibility(isVisible);

        Time.timeScale = isVisible ? 0f : 1f;

        if (isVisible)
        {
            Time.timeScale = 0f;
            return;
        }
        
        // 게임 도중 팝업 UI 창을 닫으면 카운터다운 UI 표출
        bool isPopUp = menu != MenuType.Countdown && managedUIArray[(int)menu].isPopUp;
        bool isPlaying = GameManager.Instance.GameState == GameState.Playing;
        if (isPopUp && isPlaying)
        {
            SetVisibility(MenuType.Countdown, true);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

#region 버튼 OnClick 이벤트 콜백
    public void SetOptionVisibility(bool isVisible)
    {
        SetVisibility(MenuType.Option, isVisible);
    }
#endregion
}
