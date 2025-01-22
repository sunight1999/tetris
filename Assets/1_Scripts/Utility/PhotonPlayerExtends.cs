using Photon.Realtime;

public static class PhotonPlayerExtends
{
    public static TetrisPlayer GetTetrisPlayer(this Player player)
    {
        if (player.TagObject == null)
            return null;
        
        return player.TagObject as TetrisPlayer;
    }
}
