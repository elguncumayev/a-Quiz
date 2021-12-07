using System.Collections.Generic;
using UnityEngine;

public class LocalUser : MonoBehaviour
{
    #region Singleton
    private static LocalUser _instance;
    public static LocalUser Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion
    
    public string ID;
    public string nickName;
    public int avatarIndex;
    public int trophy;

    public int numberOfAllGames;
    public int numberOfWonGames;
    public int numberOfCorrectAnswers;

    public List<FinishedMatchInfo> finishedGames;
    public List<FriendData> friends;
    public List<FriendData> friendRequests;
    public List<WaitingGames> waitingGames;

    public string facebookID;
    public string facebookNickname;

    public CreatedRoomDatas currentCreatedGame;
}
