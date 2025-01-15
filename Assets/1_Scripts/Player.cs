using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private StateInfo stateInfo = null;
    
    [SerializeField]
    private KeyCode leftKey = KeyCode.None;
    
    [SerializeField]
    private KeyCode downKey = KeyCode.None;
    
    [SerializeField]
    private KeyCode rightKey = KeyCode.None;
    
    [SerializeField]
    private KeyCode rotateKey = KeyCode.None;
    
    [SerializeField]
    private KeyCode dropKey = KeyCode.None;
    
    [SerializeField]
    private KeyCode holdKey = KeyCode.None;
    
    [SerializeField]
    private KeyCode readyKey = KeyCode.None;
    
    [SerializeField]
    private Stage stage = null;

    public bool IsReady { get; private set; } = false;
    public Queue<int> HitQueue = new Queue<int>();

    private void Start()
    {
        stateInfo.Init(readyKey.ToString());
        stage.SetPlayer(this);
    }

    private void Update()
    {
        if (UIManager.Instance.IsMenuOpened)
            return;
        
        if (GameManager.Instance.GameState != GameState.Playing)
        {
            if (Input.GetKeyDown(readyKey))
            {
                Ready();
            }
            
            return;
        }

        if (Input.GetKeyDown(leftKey))
            stage.TryMoveBlock(Direction.Left);
        if (Input.GetKeyDown(downKey))
            stage.TryMoveBlock(Direction.Down);
        if (Input.GetKeyDown(rightKey))
            stage.TryMoveBlock(Direction.Right);
        
        if (Input.GetKeyDown(rotateKey))
            stage.RotateBlock();
        if (Input.GetKeyDown(holdKey))
            stage.HoldBlock();
        if (Input.GetKeyDown(dropKey))
            stage.DropBlock();
    }

    public void Ready()
    {
        IsReady = true;
        stateInfo.Ready();
    }

    public void Play()
    {
        // 다음 게임을 위해 준비 상태를 false로 전환
        IsReady = false;
        
        stage.Play();
        stateInfo.Disable();
    }

    public void Hit(int obstacleNum)
    {
        HitQueue.Enqueue(obstacleNum);
    }

    public void Win()
    {
        stateInfo.SetWin();
        stage.Stop();
    }

    public void Lose()
    {
        stateInfo.SetLose();
        stage.Stop();
    }
}
