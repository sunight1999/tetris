public class GameManager : SingletonBehaviour<GameManager>
{
    public Player[] players = null;

    public GameState GameState { get; private set; } = GameState.Idle;

    private void Update()
    {
        if (GameState != GameState.Idle)
            return;

        if (players[0].IsReady && players[1].IsReady)
        {
            Play();
        }
    }

    public void Play()
    {
        GameState = GameState.Playing;
        
        players[0].Play();
        players[1].Play();
    }

    public void AttackTo(Player from, int obstacleNum)
    {
        if (from == players[0])
        {
            players[1].Hit(obstacleNum);
        }
        else
        {
            players[0].Hit(obstacleNum);
        }
    }

    public void SetLose(Player player)
    {
        GameState = GameState.Idle;

        if (player == players[0])
        {
            players[0].Lose();
            players[1].Win();
        }
        else
        {
            players[0].Win();
            players[1].Lose();
        }
    }
}
