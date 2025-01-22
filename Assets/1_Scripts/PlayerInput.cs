using UnityEngine;

[RequireComponent(typeof(TetrisPlayer))]
public class PlayerInput : MonoBehaviour
{
    private TetrisPlayer tetrisPlayer = null;
    private float[] autoMoveTimeArray = new float[3];
    private KeyCode[] autoMoveTargetKeyArray = new KeyCode[3];

    [field: SerializeField]
    public float AutoMoveDelay { get; private set; } = 0.15f;
    
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

    private void Start()
    {
        if (!tetrisPlayer.photonView.IsMine)
        {
            enabled = false;
        }

        autoMoveTargetKeyArray[0] = LeftKey;
        autoMoveTargetKeyArray[1] = DownKey;
        autoMoveTargetKeyArray[2] = RightKey;
    }

    private void Update()
    {
        if (GameManager.Instance.GameState == GameState.OperatingPause || GameManager.Instance.GameState == GameState.Pause)
            return;
        
        if (GameManager.Instance.GameState == GameState.Idle)
        {
            if (Input.GetKeyDown(ReadyKey))
            {
                tetrisPlayer.SetReady(true);
            }

            return;
        }

        for (int i = 0; i < autoMoveTargetKeyArray.Length; i++)
        {
            if (Input.GetKeyDown(autoMoveTargetKeyArray[i]))
            {
                tetrisPlayer.Stage.TryMoveBlock((Direction)i);
            }
        }
        
        if (Input.GetKeyDown(RotateKey))
            tetrisPlayer.Stage.TryRotateBlock();
        if (Input.GetKeyDown(DropKey))
            tetrisPlayer.Stage.DropBlock();
        if (Input.GetKeyDown(HoldKey))
            tetrisPlayer.Stage.HoldBlock();

        for (int i = 0; i < autoMoveTargetKeyArray.Length; i++)
        {
            if (!Input.GetKey(autoMoveTargetKeyArray[i]))
            {
                continue;
            }

            autoMoveTimeArray[i] += Time.deltaTime;
            if (autoMoveTimeArray[i] >= AutoMoveDelay)
            {
                tetrisPlayer.Stage.TryMoveBlock((Direction)i);
                autoMoveTimeArray[i] = 0f;
            }
        }

        for (int i = 0; i < autoMoveTargetKeyArray.Length; i++)
        {
            if (Input.GetKeyUp(autoMoveTargetKeyArray[i]))
            {
                autoMoveTimeArray[i] = 0f;
            }
        }
    }
}
