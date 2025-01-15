using UnityEngine;
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
    Playing
}

public enum MenuType
{
    Option,
    Countdown
}

public class TetrisDefine : SingletonBehaviour<TetrisDefine>
{
    public const int PlayerCount = 2;

    public const int TetrisStageRows = 20;
    public const int TetrisStageCols = 10;
    
    public const int TetrisBlockMaxRotation = 4;
    public const int TetrisBlockCount = 7;
    public const int TetrisBlockRows = 4;
    public const int TetrisBlockCols = 4;

    public const int InvalidIndex = -1;
    
    public static readonly Color NormalCellColor = new Color(0f, 0f, 0f, 0.5f);
    public static readonly Color ObstacleCellColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    
    public TetrisBlock[] tetrisBlocks;

    private Random random = new Random();

    protected override void Awake()
    {
        base.Awake();

        foreach (TetrisBlock tetrisBlock in tetrisBlocks)
        {
            tetrisBlock.CaculateAllRotations();
        }
    }

    public TetrisBlock GetRandomTetrisBlock()
    {
        int randomIndex = random.Next(0, tetrisBlocks.Length);
        return tetrisBlocks[randomIndex];
    }
}
