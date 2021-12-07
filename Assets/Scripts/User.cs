[System.Serializable]
public class User
{
    public UserID userID;


    public FriendData[] friends;
    public FriendData[] friendRequests;


    public LastMatch lastFinishedMatches;
    // public FinishedMatchInfo[] lastFinishedMatches;
    public WaitingGames[] waitingGames;

    public MyUserDatas myUserDatas;
}


[System.Serializable]
public class UserID
{
    public string userID;
}


[System.Serializable]
public class LastMatch
{
    public FinishedMatchInfo[] items;
}


[System.Serializable]
public class MyUserDatas
{
    public string nickName = "";
    public int trophy;
    public int avatarIndex = 0;
    public int allGames;
    public int allGamesWon;
    public int allCorrectAnswers;

}