[System.Serializable]
public class FinishedMatchInfo
{
    public string opID;
    public string oppName;
    public int oppAvatarIndex;
    public int myScore;
    public int oppScore;
    public int category;
}


[System.Serializable]
public class WaitingGames
{
    public int category;
    public int myScore;
    public string myID;
    public bool amIHost;

    public int oppScore;
    public string oppName;
    public int oppAvatarIndex;
    public string opID;

    public bool finished;
    public int[] questionIndexes;
    public int[] questionAnswers;

    public bool gameWithFriend;

}

[System.Serializable]
public class CreatedRoomDatas
{
    public int category;
    public string hostID;
    public int[] questionIndexes;
    public int[] questionAnswers;
    public int hostScore;
    public string hostNickname;
    public int hostAvatarIndex;
}

[System.Serializable]
public class FriendData
{
    public string friendID;
    public int friendAvatarIndex;
    public string friendNickName;
    public int friendTrophy;
}

[System.Serializable]
public class LastID
{
    public string lastID = "";
}