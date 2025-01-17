using UnityEngine;

[RequireComponent(typeof(TetrisPlayer))]
public class PlayerInput : MonoBehaviour
{
    private TetrisPlayer tetrisPlayer = null;
    
    [field: SerializeField]
    public KeyCode LeftKey { get; private set;} = KeyCode.None;
    
    [field: SerializeField]
    public KeyCode DownKey { get; private set;} = KeyCode.None;
    
    [field: SerializeField]
    public KeyCode RightKey { get; private set;} = KeyCode.None;
    
    [field: SerializeField]
    public KeyCode RotateKey { get; private set;} = KeyCode.None;
    
    [field: SerializeField]
    public KeyCode DropKey { get; private set;} = KeyCode.None;
    
    [field: SerializeField]
    public KeyCode HoldKey { get; private set;} = KeyCode.None;
    
    [field: SerializeField]
    public KeyCode ReadyKey { get; private set;} = KeyCode.None;

    private void Awake()
    {
        tetrisPlayer = GetComponent<TetrisPlayer>();
    }

    private void Update()
    {
        if (!tetrisPlayer.photonView.IsMine)
            return;
        
        if (GameManager.Instance.GameState == GameState.Pause)
            return;
        
        if (GameManager.Instance.GameState == GameState.Idle)
        {
            if (Input.GetKeyDown(ReadyKey))
            {
                tetrisPlayer.Ready();
            }
        }
        
        if (Input.GetKeyDown(LeftKey))
            tetrisPlayer.Stage.TryMoveBlock(Direction.Left);
        if (Input.GetKeyDown(DownKey))
            tetrisPlayer.Stage.TryMoveBlock(Direction.Down);
        if (Input.GetKeyDown(RightKey))
            tetrisPlayer.Stage.TryMoveBlock(Direction.Right);
        if (Input.GetKeyDown(RotateKey))
            tetrisPlayer.Stage.TryRotateBlock();
        if (Input.GetKeyDown(DropKey))
            tetrisPlayer.Stage.DropBlock();
        if (Input.GetKeyDown(HoldKey))
            tetrisPlayer.Stage.HoldBlock();
    }
}
