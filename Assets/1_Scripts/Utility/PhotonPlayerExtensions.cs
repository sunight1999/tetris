using Photon.Realtime;

public static class PhotonPlayerExtensions
{
    public static TetrisPlayer GetTetrisPlayer(this Player player)
    {
        return player.TagObject as TetrisPlayer;
    }
}
