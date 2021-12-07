using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public UserID userID;

    public FriendData[] friends;
    public FriendData[] friendRequests;

    public LastMatch lastFinishedMatches;
    public WaitingGames[] waitingGames;

    public MyUserDatas myUserDatas;


    public PlayerData(User _user)
    {
        userID = new UserID
        {
            userID = _user.userID.userID
        };

        myUserDatas = new MyUserDatas();
        myUserDatas.allCorrectAnswers = _user.myUserDatas.allCorrectAnswers;
        myUserDatas.allGames = _user.myUserDatas.allGames;
        myUserDatas.allGamesWon = _user.myUserDatas.allGamesWon;
        myUserDatas.avatarIndex = _user.myUserDatas.avatarIndex;
        myUserDatas.nickName = _user.myUserDatas.nickName;
        myUserDatas.trophy = _user.myUserDatas.trophy;

        friends = new FriendData[_user.friends.Length];
        for (int i = 0; i < friends.Length; i++)
        {
            friends[i] = new FriendData();
            friends[i] = _user.friends[i];
        }

        friendRequests = new FriendData[_user.friendRequests.Length];
        for (int i = 0; i < friendRequests.Length; i++)
        {
            friendRequests[i] = new FriendData();
            friendRequests[i] = _user.friendRequests[i];
        }

        lastFinishedMatches = new LastMatch
        {
            items = new FinishedMatchInfo[_user.lastFinishedMatches.items.Length]
        };
        for (int i = 0; i < _user.lastFinishedMatches.items.Length; i++)
        {
            lastFinishedMatches.items[i] = new FinishedMatchInfo();
            lastFinishedMatches.items[i] = _user.lastFinishedMatches.items[i];
        }

        waitingGames = new WaitingGames[_user.waitingGames.Length];
        for (int i = 0; i < _user.waitingGames.Length; i++)
        {
            waitingGames[i] = new WaitingGames();
            waitingGames[i] = _user.waitingGames[i];
        }





    }
}
