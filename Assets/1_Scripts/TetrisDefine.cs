using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public enum Direction
{
    Left,
    Down,
    Right
}

public enum GameState
{
    Idle,
    Playing,
    Pause
}

public enum MenuType
{
    Option,
    Countdown
}

public enum PlayerInitDataType
{
    Stage,
    StateInfo
}

public enum PlayerInputType
{
    LeftKeyDowned,
    DownKeyDowned,
    RightKeyDowned,
    RotateKeyDowned,
    HoldKeyDowned,
    DropKeyDowned,
    ReadyKeyDowned,
}

public enum TetrisBlockColorType
{
    Normal,
    Obstacle,
    Red,
    Yellow,
    Green,
    Orange,
    Sky,
    Purple,
    Blue
}

public enum TetrisEventCode : byte
{
    AllPlayerLoadedLevelEvent,
    AllPlayerIsReadyEvent,
    GameEndEvent
}

public class TetrisDefine : SingletonBehaviour<TetrisDefine>
{
    // 플레이어 커스텀 프로퍼티 키
    public const string PlayerLoadedLevelProperty = "PlayerLoadedLevel";
    public const string PlayerIsReadyProperty = "PlayerIsReady";
    
    public const int PlayerCount = 2;
    public const float PlayerSearchingInterval = .5f;

    public const int TetrisStageRows = 20;
    public const int TetrisStageCols = 10;
    public const int TetrisStageCellCount = TetrisDefine.TetrisStageRows * TetrisDefine.TetrisStageCols;

    public const int TetrisBlockMaxRotation = 4;
    public const int TetrisBlockCount = 7;
    public const int TetrisBlockRows = 4;
    public const int TetrisBlockCols = 4;
    public const int TetrisBlockCellCount = TetrisDefine.TetrisBlockRows * TetrisDefine.TetrisBlockCols;

    public const int InvalidIndex = -1;

    public static readonly Color[] tetrisBlockColors = new Color[]
    {
        new Color(0f, 0f, 0f, 0.5f),
        new Color(0.2f, 0.2f, 0.2f, 1f),
        new Color(0.8f, 0f, 0f, 1f),
        new Color(0.8f, 0.8f, 0f, 1f),
        new Color(0f, 0.8f, 0.18f, 1f),
        new Color(0.85f, 0.45f, 0f, 1f),
        new Color(0f, 0.85f, 0.78f, 1f),
        new Color(0.5f, 0f, 0.85f, 1f),
        new Color(0f, 0.44f, 0.85f, 1f),
    };
    
    // TODO: 블록 정보 json으로 정의하고 로딩하기
    public TetrisBlock[] tetrisBlockArray;

    private Random random = new Random();

    protected override void Awake()
    {
        base.Awake();

        foreach (TetrisBlock tetrisBlock in tetrisBlockArray)
        {
            tetrisBlock.CaculateAllRotations();
        }
    }

    public TetrisBlock GetRandomTetrisBlock()
    {
        int randomIndex = random.Next(0, tetrisBlockArray.Length);
        return tetrisBlockArray[randomIndex];
    }
}
