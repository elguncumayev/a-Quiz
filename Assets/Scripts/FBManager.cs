using System.Collections;
using System.IO;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
//using Newtonsoft.Json;

public class FBManager : MonoBehaviour
{
    #region Singleton

    private static FBManager instance;

    public static FBManager Instance { get => instance; }

    private void Awake()
    {
        instance = this;
        reference = FirebaseDatabase.DefaultInstance.RootReference;

    }

    #endregion

    DatabaseReference reference;
    public User user;
    [Space]
    public PlayerData playerData;
    [Space]
    // createdRoomDatas will be stored in fb when we will create room, in awg, after all questions will be answered
    public CreatedRoomDatas createdRoomDatas;
    public WaitingGames waitingGamesInUser;
    [Space]
    public string hostId;
    public string lastIDString;
    private readonly string localID = "lclID";

    private void Start()
    {
        //reference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("reference:_" + reference + "_");
        // ReadUserDatas(user.userID.userID);
        ReadDecodingKey();
        //FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        //RemoveListeners();
        AddListeners();
    }

    #region EventListeners

    void RemoveListeners()
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("friendRequests")
        .ChildAdded -= NewFriendRequestAdded;

        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("friends")
        .ChildAdded -= NewFriendtAdded;

        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("friends")
        .ChildRemoved -= FriendRemovedYou;

        //FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("waitingGames")
        //.ChildAdded -= NewWaitingGamesAdded;

    }

    void AddListeners()
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("friendRequests")
        .ChildAdded += NewFriendRequestAdded;

        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("friends")
        .ChildAdded += NewFriendtAdded;

        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("friends")
        .ChildRemoved += FriendRemovedYou;

        //FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.userID.userID).Child("waitingGames")
        //.ChildAdded += NewWaitingGamesAdded;
    }

    bool CheckIfFriendExistsInList(FriendData friendToCheck, List<FriendData> friendsList)
    {

        for (int i = 0; i < friendsList.Count; i++)
        {
            if (friendToCheck.friendID == friendsList[i].friendID)
            {
                return true;
            }
        }
        return false;
    }

    bool CheckWGExistsInList(WaitingGames wgToCheck, List<WaitingGames> wgs)
    {
        for (int i = 0; i < wgs.Count; i++)
        {
            if (ListsAreEqual(wgToCheck.questionIndexes, wgs[i].questionIndexes) )
            {
                return true;
            }
        }
        return false;
    }

    int FindIndexOfFriendFromFriends(FriendData rFriends, List<FriendData> friends)
    {
        int res = -1;

        for (int i = 0; i < friends.Count; i++)
        {
            if (rFriends.friendID == friends[i].friendID)
            {
                return i;
            }
        }

        return res;
    }

    void NewFriendRequestAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log("---------------NewFriendRequestAdded---------------");
        FriendData newFriend = new FriendData()
        {
            friendAvatarIndex = int.Parse(args.Snapshot.Child("friendAvatarIndex").Value.ToString()),
            friendTrophy = int.Parse(args.Snapshot.Child("friendTrophy").Value.ToString()),
            friendID = args.Snapshot.Child("friendID").Value.ToString(),
            friendNickName = args.Snapshot.Child("friendNickName").Value.ToString()
        };
        if (!CheckIfFriendExistsInList(newFriend, LocalUser.Instance.friendRequests)) // If we dont have that friend, then add it
        { 
            Debug.Log("--------------------...adding new friend request...--------------------");
            LocalUser.Instance.friendRequests.Add(newFriend);
            MenuLogic.Instance.WriteDataToUI();
        }
    }

    void NewFriendtAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log("---------------NewFriendtAdded---------------");

        FriendData newFriend = new FriendData()
        {
            friendAvatarIndex = int.Parse(args.Snapshot.Child("friendAvatarIndex").Value.ToString()),
            friendTrophy = int.Parse(args.Snapshot.Child("friendTrophy").Value.ToString()),
            friendID = args.Snapshot.Child("friendID").Value.ToString(),
            friendNickName = args.Snapshot.Child("friendNickName").Value.ToString()
        };
        if (!CheckIfFriendExistsInList(newFriend, LocalUser.Instance.friends)) // If we dont have that friend, then add it
        {
            Debug.Log("--------------------...adding new friend request...--------------------");
            LocalUser.Instance.friends.Add(newFriend);
            MenuLogic.Instance.WriteDataToUI();
        }
    }

    void FriendRemovedYou(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log("---------------FriendRemovedYou---------------");
        FriendData removingFriend = new FriendData()
        {
            friendAvatarIndex = int.Parse(args.Snapshot.Child("friendAvatarIndex").Value.ToString()),
            friendTrophy = int.Parse(args.Snapshot.Child("friendTrophy").Value.ToString()),
            friendID = args.Snapshot.Child("friendID").Value.ToString(),
            friendNickName = args.Snapshot.Child("friendNickName").Value.ToString()
        };

        if (CheckIfFriendExistsInList(removingFriend, LocalUser.Instance.friends)) // If we dont have that friend, then add it
        {
            Debug.Log("--------------------...removing friend from you...--------------------");
            int rFriendIndex = FindIndexOfFriendFromFriends(removingFriend, LocalUser.Instance.friends);
            LocalUser.Instance.friends.RemoveAt(rFriendIndex);
            MenuLogic.Instance.WriteDataToUI();
        }
    }

    void NewWaitingGamesAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log("---------------NewWaitingGamesAdded---------------");
        WaitingGames newWaitingGame = new WaitingGames()
        {
            amIHost = bool.Parse(args.Snapshot.Child("amIHost").Value.ToString()),
            category = int.Parse(args.Snapshot.Child("category").Value.ToString()),
            finished = bool.Parse(args.Snapshot.Child("finished").Value.ToString()),
            gameWithFriend = bool.Parse(args.Snapshot.Child("gameWithFriend").Value.ToString()),
            myID = args.Snapshot.Child("myID").Value.ToString(),
            myScore = int.Parse(args.Snapshot.Child("myScore").Value.ToString()),
            opID = args.Snapshot.Child("opID").Value.ToString(),
            oppAvatarIndex = int.Parse(args.Snapshot.Child("oppAvatarIndex").Value.ToString()),
            oppName = args.Snapshot.Child("oppName").Value.ToString(),
            oppScore = int.Parse(args.Snapshot.Child("oppScore").Value.ToString()),
        };
        newWaitingGame.questionIndexes = new int[10];
        for (int i = 0; i < 10; i++)
        {
            newWaitingGame.questionIndexes[i] = int.Parse(args.Snapshot.Child("questionIndexes").Child(i.ToString()).Value.ToString());
        }

        if ( !CheckWGExistsInList(newWaitingGame, LocalUser.Instance.waitingGames) )
        {
            Debug.Log("--------------------...Adding WG...--------------------");
            LocalUser.Instance.waitingGames.Add(newWaitingGame);
            MenuLogic.Instance.WriteDataToUI();
        }
        //ReadUserDatas(LocalUser.Instance.ID);
    }
    
    #endregion

    #region Overall Functions

    public void SaveUserPrivateDatas()
    {
        SetLocalDatasToUserObject();
        if (LocalUser.Instance.ID == "" || LocalUser.Instance.ID == null)
        {
            LocalUser.Instance.ID = "VIRTUAL";
        }
        string json = JsonUtility.ToJson(user.myUserDatas);
        reference.Child("Users").Child(LocalUser.Instance.ID).Child("myUserDatas").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Successfully added to DB");
            }
        });
    }

    int FirstEmptyIndex(DataSnapshot snapshotParent)
    {
        int c = 0;
        while (true)
        {
            if (!snapshotParent.Child(c.ToString()).Exists)
            {
                return c;
            }
            c++;
        }
    }

    public void ReadDecodingKey()
    {
        reference.Child("key").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                //key = snapshot.Value.ToString();
                DataManager.Instance.LoadData(snapshot.Value.ToString());
            }
        });
    }

    public void ReadUserDatas(string myID)
    {
        Debug.Log("---------ReadUserDatas---------");
        reference.Child("Users").Child(myID).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    RefreshLocalUserDatas(myID);
                    StartCoroutine(SetCloudDatasToVariables(snapshot));
                }
                else
                {
                    Debug.Log("User: _" + myID + "_ not found");
                    //Debug.Log("Creating new one...");
                    //GetLastID();
                }
            }
        });
    }

    IEnumerator SetCloudDatasToVariables(DataSnapshot snapshot)
    {
        yield return null;
        Debug.Log("User found: " + user.userID.userID);
        Debug.Log("snapshot.key: " + snapshot.Key);        
        user.myUserDatas = new MyUserDatas();
        user.userID = new UserID
        {
            userID = snapshot.Child("userID").Child("userID").Value.ToString()
        };
        //Debug.Log(snapshot.Child("myUserDatas").Child("nickName").Equals(null));
        user.myUserDatas.nickName = snapshot.Child("myUserDatas").Child("nickName").Value.ToString();
        user.myUserDatas.trophy = int.Parse(snapshot.Child("myUserDatas").Child("trophy").Value.ToString());
        user.myUserDatas.avatarIndex = int.Parse(snapshot.Child("myUserDatas").Child("avatarIndex").Value.ToString());
        user.myUserDatas.allGames = int.Parse(snapshot.Child("myUserDatas").Child("allGames").Value.ToString());
        user.myUserDatas.allGamesWon = int.Parse(snapshot.Child("myUserDatas").Child("allGamesWon").Value.ToString());
        user.myUserDatas.allCorrectAnswers = int.Parse(snapshot.Child("myUserDatas").Child("allCorrectAnswers").Value.ToString());
        int c = 0;

        user.friends = new FriendData[snapshot.Child("friends").ChildrenCount];
        user.friendRequests = new FriendData[snapshot.Child("friendRequests").ChildrenCount];
        user.lastFinishedMatches.items = new FinishedMatchInfo[snapshot.Child("lastFinishedMatches").Child("items").ChildrenCount];
        user.waitingGames = new WaitingGames[snapshot.Child("waitingGames").ChildrenCount]; 

        if (snapshot.Child("friends").Exists)
        {
            // READING FRIENDS
            foreach (DataSnapshot childs in snapshot.Child("friends").Children)
            {
                Debug.Log("Friends: " + childs.Child("friendID").Value.ToString());
                user.friends[c] = new FriendData()
                {
                    friendID = childs.Child("friendID").Value.ToString(),
                    friendNickName = childs.Child("friendNickName").Value.ToString(),
                    friendAvatarIndex = int.Parse(childs.Child("friendAvatarIndex").Value.ToString()),
                    friendTrophy = int.Parse(childs.Child("friendTrophy").Value.ToString())
                };
                c++;
            }
        }
        if (snapshot.Child("friendRequests").Exists)
        {
            // READING FRIENDS REQUESTS 
            Debug.Log("READING FRIENDS REQUESTs");
            c = 0;
            foreach (DataSnapshot childs in snapshot.Child("friendRequests").Children)
            {
                Debug.Log("Users: " + childs.Child("friendID").Value.ToString());
                user.friendRequests[c] = new FriendData()
                {
                    friendID = childs.Child("friendID").Value.ToString(),
                    friendNickName = childs.Child("friendNickName").Value.ToString(),
                    friendAvatarIndex = int.Parse(childs.Child("friendAvatarIndex").Value.ToString()),
                    friendTrophy = int.Parse(childs.Child("friendTrophy").Value.ToString())
                };
                c++;
            }
        }
        if (snapshot.Child("lastFinishedMatches").Exists)
        {
            // READING Last Matches
            Debug.Log("READING Last Matches: " + snapshot.Child("lastFinishedMatches").Child("items").ChildrenCount);
            c = 0;
            foreach (DataSnapshot childs in snapshot.Child("lastFinishedMatches").Child("items").Children)
            {
                Debug.Log("READING each of Finished Games, " + c);
                user.lastFinishedMatches.items[c] = new FinishedMatchInfo
                {
                    myScore = int.Parse(childs.Child("myScore").Value.ToString()),
                    oppScore = int.Parse(childs.Child("oppScore").Value.ToString()),
                    oppAvatarIndex = int.Parse(childs.Child("oppAvatarIndex").Value.ToString()),
                    opID = childs.Child("opID").Value.ToString(),
                    oppName = childs.Child("oppName").Value.ToString(),
                    category = int.Parse(childs.Child("category").Value.ToString())
                };
                c++;
            }
        }
        if (snapshot.Child("waitingGames").Exists)
        {
            // READING WaitingGames
            Debug.Log("READING WaitingGames");
            
            c = 0;
            foreach (DataSnapshot childs in snapshot.Child("waitingGames").Children)
            {
                user.waitingGames[c] = new WaitingGames
                {
                    myID = childs.Child("myID").Value.ToString(),
                    amIHost = bool.Parse(childs.Child("amIHost").Value.ToString()),
                    myScore = int.Parse(childs.Child("myScore").Value.ToString()),
                    oppScore = int.Parse(childs.Child("oppScore").Value.ToString()),
                    oppAvatarIndex = int.Parse(childs.Child("oppAvatarIndex").Value.ToString()),
                    opID = childs.Child("opID").Value.ToString(),
                    oppName = childs.Child("oppName").Value.ToString(),
                    category = int.Parse(childs.Child("category").Value.ToString()),
                    finished = bool.Parse(childs.Child("finished").Value.ToString()),
                    gameWithFriend = bool.Parse(childs.Child("gameWithFriend").Value.ToString()),
                    questionIndexes = new int[10],
                    questionAnswers = new int[10]
                };
                if (childs.Child("questionIndexes").Exists)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        user.waitingGames[c].questionIndexes[i] = int.Parse(childs.Child("questionIndexes").Child(i.ToString()).Value.ToString());
                        //user.waitingGames[c].questionAnswers[i] = int.Parse(childs.Child("questionAnswers").Child(i.ToString()).Value.ToString());
                    }
                }
                if (childs.Child("questionAnswers").Exists)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        user.waitingGames[c].questionAnswers[i] = int.Parse(childs.Child("questionAnswers").Child(i.ToString()).Value.ToString());
                        //user.waitingGames[c].questionAnswers[i] = int.Parse(childs.Child("questionAnswers").Child(i.ToString()).Value.ToString());
                    }
                }
                c++;
            }
            Debug.Log("WaitingGames length from db: " + c);
        }
        Debug.Log("Reading finished");
        StartCoroutine(SetUserDatasToLocalUser());
    }

    IEnumerator SetUserDatasToLocalUser()
    {
        //Debug.Log("finishedGames length: " + LocalUser.Instance.finishedGames.Count);
        //Debug.Log("waitinggames length: "  + LocalUser.Instance.waitingGames.Count);
        //Debug.Log("friends length: " + LocalUser.Instance.friends.Count);
        //Debug.Log("friendRequests length: " + LocalUser.Instance.friendRequests.Count);

        yield return null;
        LocalUser.Instance.nickName = user.myUserDatas.nickName;
        LocalUser.Instance.ID = user.userID.userID;
        LocalUser.Instance.avatarIndex = user.myUserDatas.avatarIndex;
        LocalUser.Instance.trophy = user.myUserDatas.trophy;

        LocalUser.Instance.numberOfAllGames = user.myUserDatas.allGames;
        LocalUser.Instance.numberOfWonGames = user.myUserDatas.allGamesWon;
        LocalUser.Instance.numberOfCorrectAnswers = user.myUserDatas.allCorrectAnswers;

        LocalUser.Instance.finishedGames = new List<FinishedMatchInfo>();
        for (int i = 0; i < user.lastFinishedMatches.items.Length; i++)
        {
            LocalUser.Instance.finishedGames.Add(user.lastFinishedMatches.items[i]);
        }

        LocalUser.Instance.waitingGames = new List<WaitingGames>();
        for (int i = 0; i < user.waitingGames.Length; i++)
        {
            LocalUser.Instance.waitingGames.Add(user.waitingGames[i]);
        }

        LocalUser.Instance.friends = new List<FriendData>();
        for (int i = 0; i < user.friends.Length; i++)
        {
            LocalUser.Instance.friends.Add(user.friends[i]);
        }

        LocalUser.Instance.friendRequests = new List<FriendData>();
        for (int i = 0; i < user.friendRequests.Length; i++)
        {
            LocalUser.Instance.friendRequests.Add(user.friendRequests[i]);
        }

        MenuLogic.Instance.WriteDataToUI();
        //AddListeners();
        Debug.Log("Datas set to localuser");
    }

    public void RefreshLocalUserDatas(string playerID)
    {
        //Debug.Log("Refreshing local datas");
        LocalUser.Instance.ID = playerID;
        //LocalUser.Instance.nickName = PlayGamesController.Instance.firstNickName;
        LocalUser.Instance.avatarIndex = 0;
        LocalUser.Instance.trophy = 0;
        LocalUser.Instance.numberOfAllGames = 0;
        LocalUser.Instance.numberOfWonGames = 0;
        LocalUser.Instance.numberOfCorrectAnswers = 0;
        //Debug.Log("numberOfCorrectAnswers: " + LocalUser.Instance.numberOfCorrectAnswers);
        LocalUser.Instance.finishedGames = new List<FinishedMatchInfo>();
        LocalUser.Instance.friends = new List<FriendData>();
        LocalUser.Instance.friendRequests = new List<FriendData>();
        LocalUser.Instance.waitingGames = new List<WaitingGames>();

        //LocalUser.Instance.finishedGames = null;
        //LocalUser.Instance.friends = null;
        //LocalUser.Instance.friendRequests = null;
        //LocalUser.Instance.waitingGames = null;

    }

    public void SaveUserDatas()
    {
        SetLocalDatasToUserObject();
        if (user.userID.userID == "" || user.userID.userID == null)
        {
            user.userID.userID = "VIRTUAL";
        }
        string json = JsonUtility.ToJson(user);

        Debug.Log("Saving json: " + json);

        reference.Child("Users").Child(user.userID.userID).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("successfully added data to firebase");
            }
            else
            {
                Debug.Log("not successfull");

            }
        });
    }

    IEnumerator SaveUserDatasAfterOneFrame()
    {
        yield return null;
        SetLocalDatasToUserObject();
        if (user.userID.userID == "" || user.userID.userID == null)
        {
            user.userID.userID = "VIRTUAL";
        }
        string json = JsonUtility.ToJson(user);

        Debug.Log("Saving json: " + json);

        reference.Child("Users").Child(user.userID.userID).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("successfully added data to firebase");
                CommonData.Instance.menuCanvas.SetActive(true);
                CommonData.Instance.optionsCanvas.SetActive(false);
                CommonData.Instance.categoryCanvas.SetActive(false);
                CommonData.Instance.joinRoomCanvas.SetActive(false);
                CommonData.Instance.gameCanvas.SetActive(false);
                CommonData.Instance.endGameCanvas.SetActive(false);
                CommonData.Instance.firstNicknameCanvas.SetActive(false);
                MenuLogic.Instance.WriteDataToUI();
            }
            else
            {
                Debug.Log("not successfull");
            }
        });
    }


    void SetLocalDatasToUserObject()
    {
        user.myUserDatas.nickName = LocalUser.Instance.nickName;
        user.userID.userID = LocalUser.Instance.ID;
        user.myUserDatas.avatarIndex = LocalUser.Instance.avatarIndex;
        user.myUserDatas.trophy = LocalUser.Instance.trophy;

        user.myUserDatas.allGames = LocalUser.Instance.numberOfAllGames;
        user.myUserDatas.allGamesWon = LocalUser.Instance.numberOfWonGames;
        user.myUserDatas.allCorrectAnswers = LocalUser.Instance.numberOfCorrectAnswers;

        user.lastFinishedMatches.items = new FinishedMatchInfo[LocalUser.Instance.finishedGames.Count];
        for (int i = 0; i < user.lastFinishedMatches.items.Length; i++)
        {
            user.lastFinishedMatches.items[i] = LocalUser.Instance.finishedGames[i];
        }

        user.waitingGames = new WaitingGames[LocalUser.Instance.waitingGames.Count];
        for (int i = 0; i < user.waitingGames.Length; i++)
        {
            user.waitingGames[i] = LocalUser.Instance.waitingGames[i];
        }
        user.friends = new FriendData[LocalUser.Instance.friends.Count];
        for (int i = 0; i < user.friends.Length; i++)
        {
            user.friends[i] = LocalUser.Instance.friends[i];
        }
        user.friendRequests = new FriendData[LocalUser.Instance.friendRequests.Count];
        for (int i = 0; i < user.friendRequests.Length; i++)
        {
            user.friendRequests[i] = LocalUser.Instance.friendRequests[i];

        }
        Debug.Log("Datas set to user");
    }


    #endregion

    #region Friend System Functions

    public void SaveGameDatasToFriendAndMe(WaitingGames _currentWaitingGameForMe) // Host friend finish edende
    {
        string json;
        WaitingGames friendWaitingGame = new WaitingGames()
        {
            oppAvatarIndex = LocalUser.Instance.avatarIndex,
            opID = LocalUser.Instance.ID,
            oppName = LocalUser.Instance.nickName,
            oppScore = _currentWaitingGameForMe.myScore,

            myScore = _currentWaitingGameForMe.oppScore,
            myID = _currentWaitingGameForMe.opID,
            questionIndexes = _currentWaitingGameForMe.questionIndexes,
            questionAnswers = _currentWaitingGameForMe.questionAnswers,
            category = _currentWaitingGameForMe.category,
            finished = false,
            gameWithFriend = true,
            amIHost = false
        };

        _currentWaitingGameForMe.gameWithFriend = true;
        _currentWaitingGameForMe.finished = false;
        _currentWaitingGameForMe.amIHost = true;

        // Saving to friend's wg...
        reference.Child("Users").Child(friendWaitingGame.myID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int c = 0;
                while (true)
                {
                    if (!snapshot.Child(c.ToString()).Exists) // creating game in the first emty index
                    {
                        if (friendWaitingGame.myID == "" || friendWaitingGame.myID == null)
                        {
                            friendWaitingGame.myID = "VIRTUAL";
                        }
                        Debug.Log("Creating at usr/wg at index: " + c);
                        json = JsonUtility.ToJson(friendWaitingGame);
                        reference.Child("Users").Child(friendWaitingGame.myID).Child("waitingGames").Child(c.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("successfully added to friend's wg firebase");
                            }
                            else
                            {
                                Debug.Log("not successfull");
                            }
                        });
                        break;
                    }
                    c++;
                }
            }
        });

        // Saving to my wg...
        reference.Child("Users").Child(_currentWaitingGameForMe.myID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int c = 0;
                while (true)
                {
                    if (!snapshot.Child(c.ToString()).Exists) // creating game in the first emty index
                    {
                        Debug.Log("Creating at usr/wg at index: " + c);
                        if (_currentWaitingGameForMe.myID == "" || _currentWaitingGameForMe.myID == null)
                        {
                            _currentWaitingGameForMe.myID = "VIRTUAL";
                        }
                        json = JsonUtility.ToJson(_currentWaitingGameForMe);
                        reference.Child("Users").Child(_currentWaitingGameForMe.myID).Child("waitingGames").Child(c.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("successfully added to my wg firebase");
                            }
                            else
                            {
                                Debug.Log("not successfull");
                            }
                        });
                        break;
                    }
                    c++;
                }
            }
        });
    }

    public void SaveToMyLastMatchesAndFriendsWaitingGames(WaitingGames _currentWaitingGameForMe) // Client friend finish edende
    {

        /*
         * Save to my(client) lastMatches
         * Delete older wg from my usr/wg
         * Save to friend's(host) wg
         * Delete older wg from opp's usr/wg 
        */


        int[] tempArr = new int[10];
        string json = "";

        FinishedMatchInfo myLastMatch = new FinishedMatchInfo
        {
            opID = _currentWaitingGameForMe.opID,
            oppAvatarIndex = _currentWaitingGameForMe.oppAvatarIndex,
            oppName = _currentWaitingGameForMe.oppName,
            oppScore = _currentWaitingGameForMe.oppScore,
            myScore = _currentWaitingGameForMe.myScore,
            category = _currentWaitingGameForMe.category
        };

        WaitingGames friendWaitingGameForOpp = new WaitingGames()
        {
            opID = LocalUser.Instance.ID,
            oppAvatarIndex = LocalUser.Instance.avatarIndex,
            oppName = LocalUser.Instance.nickName,
            oppScore = _currentWaitingGameForMe.myScore,

            myScore = _currentWaitingGameForMe.oppScore,
            myID = _currentWaitingGameForMe.opID,
            questionIndexes = _currentWaitingGameForMe.questionIndexes,
            questionAnswers = _currentWaitingGameForMe.questionAnswers,
            category = _currentWaitingGameForMe.category,
            finished = true,
            gameWithFriend = true,
            amIHost = true
        };

        LocalUser.Instance.finishedGames.Add(myLastMatch);
        if (LocalUser.Instance.finishedGames.Count > 5) LocalUser.Instance.finishedGames.RemoveAt(0);

        // Save to my lastmatches
        reference.Child("Users").Child(LocalUser.Instance.ID).Child("lastFinishedMatches").Child("items").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                int c = 0;
                while (true)
                {
                    if (!snapshot.Child(c.ToString()).Exists) // find empty index
                    {
                        Debug.Log("Saving to my(client) lastMatches, at index: " + c);
                        // Saveing to index c
                        json = JsonUtility.ToJson(myLastMatch);
                        Debug.Log("Json to save in my lastmatches: " + json);
                        snapshot.Child(c.ToString()).Reference.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("Successfully added to db");
                            }
                            else
                            {
                                Debug.Log("Not added to db");
                            }
                        });
                        break;
                    }
                    c++;
                }
            }
            else
            {
                Debug.Log("User with this ID is not found: " + LocalUser.Instance.ID);
            }

        });

        // Delete from my wg
        reference.Child("Users").Child(LocalUser.Instance.ID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Removing from my wg...");
                DataSnapshot snapshot = task.Result;

                StartCoroutine( ASYNC_DeleteFromMyWG(tempArr, friendWaitingGameForOpp, snapshot) );
            }
        });

        // Delete and save to friend's wg
        reference.Child("Users").Child(friendWaitingGameForOpp.myID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Reached " + friendWaitingGameForOpp.myID + "'s wg");
                DataSnapshot snapshot = task.Result;
                StartCoroutine( ASYNC_SAvingToFriendsWGAndDeleting(snapshot, tempArr, friendWaitingGameForOpp, json) );
                Debug.Log("Reached line 734");
            }
            else
            {
                Debug.Log("User with this ID is not found: " + LocalUser.Instance.ID);
            }

        });

    }


    IEnumerator ASYNC_SAvingToFriendsWGAndDeleting(DataSnapshot snapshot, int[] tempArr, WaitingGames friendWaitingGameForOpp, string json)
    {
        yield return null;
        Debug.Log("Adding to friend's wg...");
        foreach (DataSnapshot childs in snapshot.Children)
        {
            for (int i = 0; i < 10; i++)
            {
                tempArr[i] = int.Parse(childs.Child("questionIndexes").Child(i.ToString()).Value.ToString());
            }
            if (ArraysAreEqual(tempArr, friendWaitingGameForOpp.questionIndexes))
            {
                // Found the index
                int tempIndex = int.Parse(childs.Key.ToString());
                Debug.Log("WG in friend, deleted from index: " + childs.Key.ToString());
                childs.Reference.RemoveValueAsync(); // deleting the wg
                json = JsonUtility.ToJson(friendWaitingGameForOpp);
                if (friendWaitingGameForOpp.myID == "" || friendWaitingGameForOpp.myID == null)
                {
                    friendWaitingGameForOpp.myID = "VIRTUAL";
                }
                reference.Child("Users").Child(friendWaitingGameForOpp.myID).Child("waitingGames").Child(tempIndex.ToString()).Reference.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Successfully added to db + 768");
                    }
                    else
                    {
                        Debug.Log("Not added to db");
                    }
                });
                break;
            }
        }
    }


    IEnumerator ASYNC_DeleteFromMyWG(int[] tempArr, WaitingGames friendWaitingGameForOpp, DataSnapshot snapshot)
    {
        yield return null;
        foreach (DataSnapshot childs in snapshot.Children)
        {
            for (int i = 0; i < 10; i++)
            {
                tempArr[i] = int.Parse(childs.Child("questionIndexes").Child(i.ToString()).Value.ToString());
            }
            if (ArraysAreEqual(tempArr, friendWaitingGameForOpp.questionIndexes))
            {
                // Found the index
                Debug.Log("WG in my user, deleted from index: " + childs.Key.ToString());
                childs.Reference.RemoveValueAsync(); // deleting the wg
                break;
            }
        }
    }

    //void DebugWaitingGames(WaitingGames waitingGame, string waitingGameName)
    //{
    //    string res = waitingGameName + ":\n";
    //    res += "OpID:_" + waitingGame.opID +
    //        "_\nOpAvatarIndex:_" + waitingGame.oppAvatarIndex +
    //        "_\noppName:_" + waitingGame.oppName +
    //        "_\noppScore:_" + waitingGame.oppScore +

    //        "_\nmyScore:_" + waitingGame.myScore +
    //        "_\nmyID:_" + waitingGame.myID +
    //        "_\nfinished:_" + waitingGame.finished +
    //        "_\ngameWithFriend:_" + waitingGame.gameWithFriend +

    //        "_\nquestionIndexes:_" + waitingGame.questionIndexes[0] + "_" + waitingGame.questionIndexes[1] + waitingGame.questionIndexes[2] + "..." +
    //        "_\nquestionAnswers:_" + waitingGame.questionAnswers[0] + "_" + waitingGame.questionAnswers[1] + waitingGame.questionAnswers[2] + "..." +

    //        "_\ncategory:_" + waitingGame.category + "_";
    //    Debug.Log(res);
    //}

    //void DebugLastMatch(FinishedMatchInfo lastMatch, string lastMatchName)
    //{
    //    string res = lastMatchName + ":\n";
    //    res += "OpID:_" + lastMatch.opID +
    //        "_\nOpAvatarIndex:_" + lastMatch.oppAvatarIndex +
    //        "_\noppName:_" + lastMatch.oppName +
    //        "_\noppScore:_" + lastMatch.oppScore +
    //        "_\nmyScore:_" + lastMatch.myScore +
    //        "_\ncategory:_" + lastMatch.category + "_";
    //    Debug.Log(res);
    //}

    public void SaveFriend()
    {

        string json = JsonUtility.ToJson(user.friends);


        if (user.userID.userID == "" || user.userID.userID== null)
        {
            user.userID.userID = "VIRTUAL";
        }
        reference.Child("Users").Child(user.userID.userID).Child("friends").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("successfully added data to firebase");
                Debug.Log("successfully added to UI");
            }
            else
            {
                Debug.Log("not successfull");

            }
        });
    }

    //public void DeleteFriendIdFromFriendRequests(string friendID, int index)
    //{
    //    reference.Child("Users").Child(user.userID.userID).Child("friendRequests").Child(index.ToString()).RemoveValueAsync();
    //}

    bool CheckIfIAlreadySentFriendRequest(DataSnapshot fRequestsSnapshot)
    {
        foreach (DataSnapshot child in fRequestsSnapshot.Children)
        {
            if (child.Child("friendID").Value.ToString() == LocalUser.Instance.ID)
            {
                return true;
            }
        }
        return false;
    }

    public void SendFriendRequest(string friendID)
    {
        reference.Child("Users").Child(friendID).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // if this user exist
                if (snapshot.Exists)
                {
                    Debug.Log("The user exists: " + friendID);
                    
                    if ( !CheckIfIAlreadySentFriendRequest( snapshot.Child("friendRequests") ) ) // If i havent already sent fr request
                    {
                        int tempC = 0;
                        while (true)
                        {
                            // 0,2,3,4--- 1 is empty, so, add to index 1
                            if (!snapshot.Child("friendRequests").Child(tempC.ToString()).Exists)
                            {
                                Debug.Log("Adding the userid to index: " + tempC);
                                Debug.Log("My ID: " + user.userID.userID);


                                // Send friend request
                                FriendData myData = new FriendData
                                {
                                    friendID = LocalUser.Instance.ID,
                                    friendAvatarIndex = LocalUser.Instance.avatarIndex,
                                    friendNickName = LocalUser.Instance.nickName,
                                    friendTrophy = LocalUser.Instance.trophy
                                };
                                string json = JsonUtility.ToJson(myData); // my ID
                                Debug.Log("usr/" + friendID + "/friendRequests/" + tempC + "/-----");
                                Debug.Log("Json: " + json);
                                if (friendID == "" || friendID == null)
                                {
                                    friendID = "VIRTUAL";
                                }
                                reference.Child("Users").Child(friendID).Child("friendRequests").Child(tempC.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                                {
                                    if (task.IsCompleted)
                                    {
                                        // Show in my UI that, friend request is sent
                                        Debug.Log("FR request is sent to " + friendID + " from " + user.userID.userID);
                                    }

                                });
                                break;
                            }
                            tempC++;
                        }
                    }
                }
                // if this user does not exist
                else
                {
                    // show that the user I search does not exist
                    Debug.Log("The user does not exist: " + friendID);

                }
            }
        });
    }

    public void AcceptFriendRequest(string friendID)
    {
        // Save to my friends
        reference.Child("Users").Child(user.userID.userID).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("My Friend requests count " + snapshot.Child("friendRequests").ChildrenCount);

                foreach (DataSnapshot child in snapshot.Child("friendRequests").Children)
                {
                    if (child.Child("friendID").Value.ToString() == friendID)
                    {
                        // if we found the player
                        Debug.Log("We found friend request at index: " + child.Key.ToString());
                        child.Reference.RemoveValueAsync();
                        if (!WeAreAlreadyFriends(snapshot, friendID)) // check if we are not already friends
                        {
                            // Save the friend ID to friendIDs
                            Debug.Log("We are not friends yet");

                            FriendData friendData = new FriendData
                            {
                                friendID = child.Child("friendID").Value.ToString(),
                                friendAvatarIndex = int.Parse(child.Child("friendAvatarIndex").Value.ToString()),
                                friendNickName = child.Child("friendNickName").Value.ToString(),
                                friendTrophy = int.Parse(child.Child("friendTrophy").Value.ToString())
                            };
                            Debug.Log("FriendDatas: " + friendData);
                            SaveToFriendIDs(friendData, FirstEmptyIndex(snapshot.Child("friends")));
                            //// Save to other player's friends
                            friendData.friendID = LocalUser.Instance.ID;
                            friendData.friendAvatarIndex = LocalUser.Instance.avatarIndex;
                            friendData.friendNickName = LocalUser.Instance.nickName;
                            friendData.friendTrophy = LocalUser.Instance.trophy;

                            reference.Child("Users").Child(friendID).Child("friends").GetValueAsync().ContinueWithOnMainThread(task =>
                            {

                                if (task.IsCompleted)
                                {
                                    DataSnapshot snapshot = task.Result;
                                    string json = JsonUtility.ToJson(friendData);
                                    int firstEmptyIndex = FirstEmptyIndex(snapshot);
                                    snapshot.Child(firstEmptyIndex.ToString()).Reference.SetRawJsonValueAsync(json);
                                }
                            });

                        }
                        else
                        {
                            Debug.Log("We are already friends");
                        }


                        break;
                    }
                }
            }
        });
    }

    bool WeAreAlreadyFriends(DataSnapshot userSnapshot, string friendID)
    {

        foreach (DataSnapshot child in userSnapshot.Child("friends").Children)
        {
            if (child.Child("friendID").Value.ToString() == friendID)
            {
                return true;
            }
        }
        return false;

    }

    void SaveToFriendIDs(FriendData friendDatas, int firstEmptyIndex)
    {
        Debug.Log("Saving to friendIDs: " + friendDatas.friendID);
        string json = JsonUtility.ToJson(friendDatas);
        Debug.Log("json file: " + json);
        if (user.userID.userID == "" || user.userID.userID == null)
        {
            user.userID.userID = "VIRTUAL";
        }
        reference.Child("Users").Child(user.userID.userID.ToString()).Child("friends").Child(firstEmptyIndex.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                Debug.Log("FriendID: " + friendDatas.friendID + " saved to index: " + firstEmptyIndex);
            }
        });
    }

    public void DeclineFriendRequest(string friendIDString)
    {
        reference.Child("Users").Child(user.userID.userID.ToString()).Child("friendRequests").GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot child in snapshot.Children)
                {
                    if (child.Child("friendID").Value.ToString() == friendIDString)
                    {
                        Debug.Log("Friend request found at index: " + child.Key.ToString());
                        child.Reference.RemoveValueAsync();
                        break;
                    }
                }
            }

        });
    }

    public void RemoveFriend(string friendIDString)
    {
        reference.Child("Users").Child(user.userID.userID).Child("friends").GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot child in snapshot.Children)
                {
                    if (child.Child("friendID").Value.ToString() == friendIDString)
                    {
                        Debug.Log("Friend found at index: " + child.Key.ToString());
                        child.Reference.RemoveValueAsync();
                        break;
                    }
                }
            }
        });


        reference.Child("Users").Child(friendIDString).Child("friends").GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot child in snapshot.Children)
                {
                    if (child.Child("friendID").Value.ToString() == LocalUser.Instance.ID)
                    {
                        Debug.Log("Friend found at index: " + child.Key.ToString());
                        child.Reference.RemoveValueAsync();
                        break;
                    }
                }
            }
        });
    }

    public void RefreshFriendRequests()
    {
        reference.Child("Users").Child(user.userID.userID.ToString()).Child("friendRequests").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.ChildrenCount > 0) // if there are friend requests
                {
                    // READING FRIENDS REQUESTS 
                    user.friendRequests = new FriendData[snapshot.ChildrenCount];
                    int c = 0;
                    foreach (DataSnapshot childs in snapshot.Children)
                    {
                        Debug.Log("Friend requests: " + childs.Child("friendID").Value.ToString());
                        user.friendRequests[c] = new FriendData()
                        {
                            friendID = childs.Child("friendID").Value.ToString(),
                            friendNickName = childs.Child("friendNickName").Value.ToString(),
                            friendAvatarIndex = int.Parse(childs.Child("friendID").Value.ToString()),
                            friendTrophy = int.Parse(childs.Child("friendID").Value.ToString())

                        };
                        c++;
                    }
                }
            }
        });

    }

    #endregion

    #region Open/Join Room Functions

    public void JoinRandomRoom(int category)
    {
        reference.Child("WaitingGames").Child(category.ToString()).Child("gameInfos").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("0th room:" + snapshot.Child("0").Child("hostID").Value + "_");
                Debug.Log("1st room:" + snapshot.Child("1").Child("hostID").Value + "_");
                Debug.Log("2nd room:" + snapshot.Child("2").Child("hostID").Value + "_");
                int childrenCount = (int)snapshot.ChildrenCount;
                if (childrenCount == 0) // if there is not any room in this category
                {
                    // Create new room
                    Debug.Log("Create new room as there is no room");
                    LocalUser.Instance.currentCreatedGame = new CreatedRoomDatas() { category = category };
                    CommonData.Instance.gameLogic.StartNewGame(false, 0);
                    return;
                }
                else // else if there is room in this category
                {
                    List<int> otherHostsRooms = new List<int>();
                    foreach (DataSnapshot child in snapshot.Children)
                    {
                        if (child.Child("hostID").Value.ToString() != LocalUser.Instance.ID) // if this room is not created by me, then add room index to tempArr
                        {
                            otherHostsRooms.Add( int.Parse(child.Key.ToString()) );
                        }
                    }
                    if (childrenCount != otherHostsRooms.Count ) // if all rooms are created by others
                    {
                        // Create new room
                        Debug.Log("Create new room as all rooms were created by me");
                        LocalUser.Instance.currentCreatedGame = new CreatedRoomDatas() { category = category };
                        CommonData.Instance.gameLogic.StartNewGame(false, 0);
                        return;
                    }
                    else // if there is at least one room, thst is not created by me
                    {
                        // Join room
                        int random = (int)UnityEngine.Random.Range(0, otherHostsRooms.Count);
                        Debug.Log("Joining room: " + otherHostsRooms[random]);
                        LocalUser.Instance.currentCreatedGame = new CreatedRoomDatas
                        {
                            hostAvatarIndex = int.Parse(snapshot.Child(otherHostsRooms[random].ToString()).Child("hostAvatarIndex").Value.ToString()),
                            hostNickname = snapshot.Child(otherHostsRooms[random].ToString()).Child("hostNickname").Value.ToString(),
                            hostID = snapshot.Child(otherHostsRooms[random].ToString()).Child("hostID").Value.ToString(),
                            hostScore = int.Parse(snapshot.Child(otherHostsRooms[random].ToString()).Child("hostScore").Value.ToString()),
                            questionAnswers = new int[10],
                            questionIndexes = new int[10],
                            category = category
                        };
                        for (int i = 0; i < 10; i++)
                        {
                            LocalUser.Instance.currentCreatedGame.questionAnswers[i] = int.Parse(snapshot.Child(otherHostsRooms[random].ToString()).Child("questionAnswers").Child(i.ToString()).Value.ToString());
                        }
                        for (int i = 0; i < 10; i++)
                        {
                            LocalUser.Instance.currentCreatedGame.questionIndexes[i] = int.Parse(snapshot.Child(otherHostsRooms[random].ToString()).Child("questionIndexes").Child(i.ToString()).Value.ToString());
                        }
                        DeleteRoom(otherHostsRooms[random], category); // Delete this ID from db
                        StartCoroutine(CommonData.Instance.gameLogic.JoinCreatedGame(false, 0));
                        return;
                    }
                }
                #region Old join/Create code

                //if (!snapshot.Child("0").Exists || !snapshot.Child("1").Exists || !snapshot.Child("2").Exists) // If any of rooms is empty, show questions, then create new room
                //{
                //    // Create room
                //    Debug.Log("Room will be created");
                //    LocalUser.Instance.currentCreatedGame = new CreatedRoomDatas() { category = category };
                //    CommonData.Instance.gameLogic.StartNewGame(false, 0);
                //}
                //// if there is no empty room, join one
                //else
                //{
                //    Debug.Log("All 3 rooms exist");
                //    List<int> tempList = new List<int> { };
                //    // Coheck not t join room created by myself
                //    for (int j = 0; j < 3; j++)
                //    {
                //        if (snapshot.Child(j.ToString()).Child("hostID").Value.ToString() != LocalUser.Instance.ID) // if the room is created by us
                //        {
                //            tempList.Add(j);
                //        }
                //    }
                //    Debug.Log("There are " + tempList.Count + " / 3 rooms that were created by others");

                //    if (tempList.Count == 0) // Create room
                //    {
                //        // All 3 rooms are created by us, so we cannot join any room. Create new one
                //        Debug.Log("As all rooms were created by us, creating new room...");
                //        LocalUser.Instance.currentCreatedGame = new CreatedRoomDatas() { category = category };
                //        CommonData.Instance.gameLogic.StartNewGame(false, 0);
                //        return;
                //    }
                //    else // Join room
                //    {
                //        // There is at least 1 room in first 3 of rooms that were created by other guy
                //        int randomRoom = (int)UnityEngine.Random.Range(0, tempList.Count);
                //        Debug.Log("Room made by others, Joining random room: " + tempList[randomRoom]);

                //        // Join
                //        Debug.Log("Joining random room: " + tempList[randomRoom]);
                //        LocalUser.Instance.currentCreatedGame = new CreatedRoomDatas
                //        {
                //            hostAvatarIndex = int.Parse(snapshot.Child(tempList[randomRoom].ToString()).Child("hostAvatarIndex").Value.ToString()),
                //            hostNickname = snapshot.Child(tempList[randomRoom].ToString()).Child("hostNickname").Value.ToString(),
                //            hostID = snapshot.Child(tempList[randomRoom].ToString()).Child("hostID").Value.ToString(),
                //            hostScore = int.Parse(snapshot.Child(tempList[randomRoom].ToString()).Child("hostScore").Value.ToString()),
                //            questionAnswers = new int[10],
                //            questionIndexes = new int[10],
                //            category = category
                //        };

                //        for (int i = 0; i < 10; i++)
                //        {
                //            LocalUser.Instance.currentCreatedGame.questionAnswers[i] = int.Parse(snapshot.Child(tempList[randomRoom].ToString()).Child("questionAnswers").Child(i.ToString()).Value.ToString());
                //        }

                //        for (int i = 0; i < 10; i++)
                //        {
                //            LocalUser.Instance.currentCreatedGame.questionIndexes[i] = int.Parse(snapshot.Child(tempList[randomRoom].ToString()).Child("questionIndexes").Child(i.ToString()).Value.ToString());
                //        }

                //        DeleteRoom(tempList[randomRoom], category); // Delete this ID from db
                //                                                    // Show questions locally, at the end, when questions are finished, save them to both
                //        StartCoroutine(CommonData.Instance.gameLogic.JoinCreatedGame(false, 0));


                //        return;
                //    }
                //}
                #endregion
            }
        });
    }

    public void AddToHostsWaitingGames(WaitingGames _waitingGames)
    {
        reference.Child("Users").Child(_waitingGames.myID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int c = 0;
                while (true)
                {
                    if (!snapshot.Child(c.ToString()).Exists)
                    {
                        string json = JsonUtility.ToJson(_waitingGames);
                        if (_waitingGames.myID == "" || _waitingGames.myID == null)
                        {
                            _waitingGames.myID = "VIRTUAL";
                        }
                        reference.Child("Users").Child(_waitingGames.myID).Child("waitingGames").Child(c.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                        {
                            Debug.Log("WaitingGames saved to host, at index: " + c);
                        });

                        break;
                    }

                    c++;
                }
            }

        });
    }

    public void CreateGame(CreatedRoomDatas _createdRoomDatas)
    {
        string json;
        // Saving to AWG
        reference.Child("WaitingGames").Child(_createdRoomDatas.category.ToString()).Child("gameInfos").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int c = 0;
                while (true)
                {
                    if (!snapshot.Child(c.ToString()).Exists) // creating game in the first emty index
                    {
                        Debug.Log("Creating at awg at index: " + c);
                        string json = JsonUtility.ToJson(_createdRoomDatas);
                        reference.Child("WaitingGames").Child(_createdRoomDatas.category.ToString()).Child("gameInfos").Child(c.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("successfully added data to firebase");
                            }
                            else
                            {
                                Debug.Log("not successfull");
                            }
                        });
                        break;
                    }
                    c++;
                }
            }
        });

        // And now, we will create under host/wg
        WaitingGames createdWaitingGame = new WaitingGames
        {
            myID = _createdRoomDatas.hostID,
            myScore = _createdRoomDatas.hostScore,
            questionIndexes = _createdRoomDatas.questionIndexes,
            oppScore = 0,
            oppAvatarIndex = 0,
            opID = "",
            oppName = "",
            category = _createdRoomDatas.category,
            finished = false,
            gameWithFriend = false,
            amIHost = true
        };

        reference.Child("Users").Child(_createdRoomDatas.hostID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int c = 0;
                while (true)
                {
                    if (!snapshot.Child(c.ToString()).Exists) // creating game in the first emty index
                    {
                        Debug.Log("Creating at usr/wg at index: " + c);
                        json = JsonUtility.ToJson(createdWaitingGame);
                        if (_createdRoomDatas.hostID == "" || _createdRoomDatas.hostID == null)
                        {
                            _createdRoomDatas.hostID = "VIRTUAL";
                        }
                        reference.Child("Users").Child(_createdRoomDatas.hostID).Child("waitingGames").Child(c.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("successfully added data to firebase");
                            }
                            else
                            {
                                Debug.Log("not successfull");
                            }
                        });
                        break;
                    }
                    c++;
                }
            }

        });




    }

    public void DeleteRoom(int index, int category)
    {
        reference.Child("WaitingGames").Child(category.ToString()).Child("gameInfos").Child(index.ToString()).RemoveValueAsync();
    }

    public void AddWaitingGamesToUser(string _userID)
    {
        reference.Child("Users").Child(_userID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int temp = 0;
                while (true)
                {
                    // counting from 0, if there is empty index, we add our datas to there
                    if (!snapshot.Child(temp.ToString()).Exists)
                    {
                        // Save datas to this location
                        Debug.Log("Adding waitinggames to index " + temp);
                        string json = JsonUtility.ToJson(waitingGamesInUser);
                        if (_userID == "" || _userID == null)
                        {
                            _userID = "VIRTUAL";
                        }
                        reference.Child("Users").Child(_userID).Child("waitingGames").Child(temp.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("successfully added data to firebase");
                            }
                            else
                            {
                                Debug.Log("not successfull");

                            }
                        });
                        break;
                    }
                    temp++;
                }


            }
        });

    }

    void PrintArray(int[] arr, string arrName)
    {
        string res = "";
        for (int i = 0; i < arr.Length; i++)
        {
            res += arrName + "[" + i + "] = " + arr[i] + "\n";
            //Debug.Log(arrName + "[" + i + "] = " + arr[i]);
        }
        Debug.Log(res);
    }

    public void SaveFinishedGame(WaitingGames waitingGameForClient) // client will call this function when client finishes questions
    {
        // my(client) lastMatches
        // opp(host)'s waitingGames
        Debug.Log("Host ID: " + waitingGameForClient.opID);
        Debug.Log("Client(My) ID: " + waitingGameForClient.myID);
        PrintArray(waitingGameForClient.questionIndexes, "QuestIndexes");
        #region Saving to my-client's lastMatches
        FinishedMatchInfo lastFinishedMatch = new FinishedMatchInfo()
        {
            myScore = waitingGameForClient.myScore,
            opID = waitingGameForClient.opID,
            oppAvatarIndex = waitingGameForClient.oppAvatarIndex,
            oppName = waitingGameForClient.oppName,
            oppScore = waitingGameForClient.oppScore,
            category = waitingGameForClient.category
        };
        reference.Child("Users").Child(LocalUser.Instance.ID).Child("lastFinishedMatches").Child("items").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int firstEmtyIndex = FirstEmptyIndex(snapshot);
                //Debug.Log("My(Client)/last matches, saving to index: " + firstEmtyIndex);
                string jsonLastMatches = JsonUtility.ToJson(lastFinishedMatch);
                if (LocalUser.Instance.ID == "" || LocalUser.Instance.ID == null)
                {
                    LocalUser.Instance.ID = "VIRTUAL";
                }
                reference.Child("Users").Child(LocalUser.Instance.ID).Child("lastFinishedMatches").Child("items").Child(firstEmtyIndex.ToString()).SetRawJsonValueAsync(jsonLastMatches).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Saved LastMatch to me(Client), at index: " + firstEmtyIndex);
                    }
                });
            }
        });
        #endregion

        #region Saving to host's waitingGames
        WaitingGames waitingGameForHost = new WaitingGames()
        {
            myScore = waitingGameForClient.oppScore,
            myID = waitingGameForClient.opID,

            opID = LocalUser.Instance.ID,
            oppAvatarIndex = LocalUser.Instance.avatarIndex,
            oppName = LocalUser.Instance.nickName,
            oppScore = waitingGameForClient.myScore,

            category = waitingGameForClient.category,

            questionAnswers = waitingGameForClient.questionAnswers,
            questionIndexes = waitingGameForClient.questionIndexes,

            finished = true,
            gameWithFriend = false,
            amIHost = true
        };

        // Save to host's waitingGames: Delete the same wg, save new one
        reference.Child("Users").Child(waitingGameForClient.opID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Host wg read");
                DataSnapshot snapshot = task.Result;
                StartCoroutine(HostWaitingGameReached(snapshot, waitingGameForClient, waitingGameForHost));
            }
        });
        #endregion
    }

    IEnumerator HostWaitingGameReached(DataSnapshot snapshot, WaitingGames waitingGameForClient, WaitingGames waitingGameForHost)
    {
        yield return null;
        int[] tempArr = new int[10];
        Debug.Log("host has " + snapshot.ChildrenCount + " waiting games");
        Debug.Log("000000000000000000000000000000000000000000000000000000000000000000");
        PrintArray(waitingGameForClient.questionIndexes, "MustBeArr");
        Debug.Log("000000000000000000000000000000000000000000000000000000000000000000");
        foreach (DataSnapshot childs in snapshot.Children)
        {
            Debug.Log("Reading child: " + childs.Key.ToString() + "-----------------------------------------");
            if (childs.Child("questionIndexes").Exists)
            {
                for (int i = 0; i < childs.Child("questionIndexes").ChildrenCount; i++)
                {
                    tempArr[i] = int.Parse(childs.Child("questionIndexes").Child(i.ToString()).Value.ToString());
                    //Debug.Log("host/WG,  questionindexes[" + i + "] = " + tempArr[i]);
                }
                PrintArray(tempArr, "Arr" + childs.Key.ToString());
            }
            if (childs.Child("category").Value.ToString() == waitingGameForClient.category.ToString()
            && int.Parse(childs.Child("myScore").Value.ToString()) == waitingGameForHost.myScore
            && ArraysAreEqual(tempArr, waitingGameForClient.questionIndexes)) // if it is true waiting games
            {
                // Remove the waiting games from host
                int theWaitingGameIndex = int.Parse(childs.Key.ToString());
                Debug.Log("Index found at host/wg: " + theWaitingGameIndex);
                childs.Reference.RemoveValueAsync();

                // Then add new one

                string jsonWaitingGames = JsonUtility.ToJson(waitingGameForHost);
                Debug.Log("Host wg json:" + jsonWaitingGames);
                if (waitingGameForHost.myID == "" || waitingGameForHost.myID == null)
                {
                    waitingGameForHost.myID = "VIRTUAL";
                }
                reference.Child("Users").Child(waitingGameForHost.myID).Child("waitingGames").Child(theWaitingGameIndex.ToString()).SetRawJsonValueAsync(jsonWaitingGames).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Waiting Game deleted from host at index: " + theWaitingGameIndex + "\nAnd added new one");
                    }
                });
                break;
            }
        }
        Debug.Log("Saving to host/wg finished");
    }

    bool ArraysAreEqual(int[] arr1, int[] arr2)
    {
        for (int i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i])
            {
                return false;
            }
        }
        return true;
    }

    bool ListsAreEqual(int[] arr1, int[] arr2)
    {
        for (int i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i])
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region InMenuFunctions

    public void FromWaitingGamesToLastMatches(WaitingGames _waitingGames)
    {
        FinishedMatchInfo lastMatch = new FinishedMatchInfo
        {
            category = _waitingGames.category,
            myScore = _waitingGames.myScore,
            oppScore = _waitingGames.oppScore,
            opID = _waitingGames.opID,
            oppAvatarIndex = _waitingGames.oppAvatarIndex,
            oppName = _waitingGames.oppName
        };

        LocalUser.Instance.finishedGames.Add(lastMatch);
        if (LocalUser.Instance.finishedGames.Count > 5) LocalUser.Instance.finishedGames.RemoveAt(0);

        reference.Child("Users").Child(user.userID.userID).Child("lastFinishedMatches").Child("items").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                int c = 0;
                while (true)
                {
                    if (!snapshot.Child(c.ToString()).Exists)
                    {
                        string json = JsonUtility.ToJson(lastMatch);
                        if (user.userID.userID == "" || user.userID.userID == null)
                        {
                            user.userID.userID = "VIRTUAL";
                        }
                        reference.Child("Users").Child(user.userID.userID).Child("lastFinishedMatches").Child("items").Child(c.ToString()).SetRawJsonValueAsync(json).ContinueWithOnMainThread(tasl =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("Successfully added FB, last matches");
                            }
                        });
                        break;
                    }
                    c++;
                }
            }
        });


        reference.Child("Users").Child(user.userID.userID).Child("waitingGames").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("Reading usr/waitingGames");
                if (snapshot.Exists)
                {
                    StartCoroutine(ASYNC_read(_waitingGames, snapshot));

                }
            }
        });

    }

    IEnumerator ASYNC_read(WaitingGames _waitingGames, DataSnapshot snapshot)
    {
        yield return null;
        int waitingGameIndexToBeDeleted;
        int[] tempArr = new int[10];
        //for (int i = 0; i < childs.Child("questionIndexes").ChildrenCount; i++)
        Debug.Log("Checking each wg..., length is: " + snapshot.ChildrenCount);
        foreach (DataSnapshot childs in snapshot.Children)
        {
            for (int i = 0; i < 10; i++)
            {
                tempArr[i] = int.Parse(childs.Child("questionIndexes").Child(i.ToString()).Value.ToString());
            }
            PrintArray(tempArr, "Arr" + childs.Key.ToString());
            if (ArraysAreEqual(tempArr, _waitingGames.questionIndexes))
            {
                // Delete the found waitingGames
                waitingGameIndexToBeDeleted = int.Parse(childs.Key.ToString());
                Debug.Log("Deleting wg at index: " + waitingGameIndexToBeDeleted);
                reference.Child("Users").Child(LocalUser.Instance.ID).Child("waitingGames").Child(waitingGameIndexToBeDeleted.ToString()).RemoveValueAsync();
                break;
            }
        }
    }

    #endregion

    #region TestFunctions

    //public void SaveGame(int category)
    //{

    //    string json = JsonUtility.ToJson(createdRoomDatas);



    //    reference.Child("WaitingGames").Child(category.ToString()).Child(createdRoomDatas.hostID).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
    //    {
    //        if (task.IsCompleted)
    //        {
    //            Debug.Log("successfully added data to firebase");
    //            Debug.Log("successfully added to UI");
    //        }
    //        else
    //        {
    //            Debug.Log("not successfull");

    //        }
    //    });
    //}

    public void TestJson()
    {
        //Convert to JSON
        string playerToJson = JsonHelper.ToJson(user.lastFinishedMatches.items, true);
        //playerToJson = fixJson
        Debug.Log(playerToJson);
        if (user.userID.userID == "" || user.userID.userID == null)
        {
            user.userID.userID = "VIRTUAL";
        }
        reference.Child(user.userID.userID).Child("lastFinishedMatches").SetRawJsonValueAsync(playerToJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Completed");
            }
            else
            {
                Debug.Log("NOT Completed");
            }
        });
    }

    public void LimitLastFinishedMatches(int limit)
    {
        int count = LocalUser.Instance.finishedGames.Count;
        if (limit < LocalUser.Instance.finishedGames.Count)
        {
            for (int i = 0; i < count - limit; i++)
            {
                //Debug.Log("Removing at: " + i);
                LocalUser.Instance.finishedGames.RemoveAt(i);
            }
        }
    }

    public void SaveLastFinishedMatches()
    {
        user.lastFinishedMatches.items = LocalUser.Instance.finishedGames.ToArray();
        //Convert to JSON
        string playerToJson = JsonHelper.ToJson(user.lastFinishedMatches.items, true);
        Debug.Log(playerToJson);
        if (user.userID.userID == "" || user.userID.userID == null)
        {
            user.userID.userID = "VIRTUAL";
        }
        reference.Child(user.userID.userID).Child("lastFinishedMatches").SetRawJsonValueAsync(playerToJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Completed");
            }
            else
            {
                Debug.Log("NOT Completed");
            }
        });

    }

    public void SaveFirstLastID()
    {
        LastID lastID = new LastID()
        {
            lastID = "ADJFORB"
        };
        string json = JsonUtility.ToJson(lastID);
        reference.Child("LastID").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Successfully added to db, first lastID");
            }

        });
    }


    #endregion

    public void GetLastID()
    {
        reference.Child("LastID").Child("lastID").GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                DataSnapshot s = task.Result;
                StartCoroutine(GetLastIDCallBack(s));
            }
        });
    }

    public void GetLastID_LocallySaved()
    {
        Debug.Log("---------GetLastID_LocallySaved---------");
        reference.Child("LastID").Child("lastID").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot s = task.Result;
                StartCoroutine( SetLastID( s.Value.ToString() ) );
                LocalUser.Instance.ID = s.Value.ToString();
                PlayerPrefs.SetString(localID, LocalUser.Instance.ID);
                Debug.Log("GetLastID_LocallySaved, writing data to ui: " + LocalUser.Instance.nickName);
                MenuLogic.Instance.WriteDataToUI();
                SaveUserDatas();
                CommonData.Instance.firstNicknameCanvas.SetActive(false);
                CommonData.Instance.loadingCanvas.SetActive(false);
            }
        });
    }

    IEnumerator GetLastIDCallBack(DataSnapshot s)
    {
        yield return null;
        lastIDString = s.Value.ToString();
        Debug.Log("lastID: " + lastIDString);
        user.userID.userID = lastIDString;
        //user.myUserDatas.nickName = PlayGamesController.Instance.firstNickName;
        RefreshLocalUserDatas(lastIDString);
        MenuLogic.Instance.WriteDataToUI();
        SaveUserDatas();
        //PlayGamesController.Instance.ReadOrSaveToCloud(true);
        StartCoroutine(SetLastID(lastIDString));
    }

    IEnumerator SetLastID(string currentID)
    {
        yield return null;
        Debug.Log("1398");
        LastID lastID = new LastID()
        {
            lastID = DataManager.Instance.NextID(currentID)
        };
        string json = JsonUtility.ToJson(lastID);
        Debug.Log("JSON: " + json);
        reference.Child("LastID").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Successfully added to db, next ID: " + lastID.lastID);
            }
            else
            {
                Debug.Log("Adding to db not succesfull");
            }

        });
    }


    public void SavePlayerLocally()
    {
        SetLocalDatasToUserObject();
        LocalSaveSystem.SavePlayer(user);
    }

    public void LoadPlayerFromLocal()
    {
        PlayerData data = LocalSaveSystem.LoadPlayer();

        user.userID.userID = data.userID.userID;

        user.myUserDatas.allCorrectAnswers = data.myUserDatas.allCorrectAnswers;
        user.myUserDatas.allGames = data.myUserDatas.allGames;
        user.myUserDatas.allGamesWon = data.myUserDatas.allGamesWon;
        user.myUserDatas.avatarIndex = data.myUserDatas.avatarIndex;
        user.myUserDatas.nickName = data.myUserDatas.nickName;
        user.myUserDatas.trophy = data.myUserDatas.trophy;

        user.friends = new FriendData[data.friends.Length];
        for (int i = 0; i < data.friends.Length; i++)
        {
            user.friends[i] = new FriendData();
            user.friends[i] = data.friends[i];
        }

        user.friendRequests = new FriendData[data.friendRequests.Length];
        for (int i = 0; i < data.friendRequests.Length; i++)
        {
            user.friendRequests[i] = new FriendData();
            user.friendRequests[i] = data.friendRequests[i];
        }

        user.waitingGames = new WaitingGames[data.waitingGames.Length];
        for (int i = 0; i < data.waitingGames.Length; i++)
        {
            user.waitingGames[i] = new WaitingGames();
            user.waitingGames[i] = data.waitingGames[i];
        }

        user.lastFinishedMatches = new LastMatch
        {
            items = new FinishedMatchInfo[data.lastFinishedMatches.items.Length]
        };
        for (int i = 0; i < data.lastFinishedMatches.items.Length; i++)
        {
            user.lastFinishedMatches.items[i] = new FinishedMatchInfo();
            user.lastFinishedMatches.items[i] = data.lastFinishedMatches.items[i];
        }
        StartCoroutine(SetUserDatasToLocalUser());
    }

    public void SetFirstNickName()
    { 
    
    
    }


    #region IOS

    /*
     1) at start:
        --- it checks facebookID from playerprefs
        --- if there is facebookID in  playerprefs, get datas from db by that id
        --- else, do everything locally. Read facebookID from local, if there is one, continue with that. Else, create new account in local.
        --- But how can you play without having account in db, others cannot play with you normal
        --- So, the idea is to create an account in db as well.
        --- We have to store locally the id that is stored in db, so everytime user plays without login, it reads from the locally-saved id.
        --- When user logs in to the account:
                If the fb account already has account in db,
                    Play with fb account, but also locally saved id must not be deleted
                If the fb account doesnt have an account in db,
                    Create new acc in db, with datas of LocallySavedDatas,
                    Delete old LocallySavedDatas from db
        a) if first time:
            it reads lastId and changes it by one letter, 
            saves it under "AppIds"/fb.appid/lastId
            saves acc under users/lastId/...
            saves fbAppId in playerprefs
        b) if not first time:
            it reads: "AppIds"/fb.appid/lastid
            it reads "Users"/lastid
        c) when sending friendRequest:
            user search lastId
            my lastid goes to "Users"/friend's lastid/friendrequests
    2) each time, no login required. by the playerprefs, we get userdatas from db
     */

    public void ReadUserDatasByFacebook(string facebookID)
    {
        Debug.Log("---------------------------------------------ReadUserDatasByFacebook:_" + facebookID + "_---------");
        //Debug.Log("ReadUserDatasByFacebook:_" + reference.Key + "_");
        reference.Child("AppIDs").Child(facebookID).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Reading appID completed");
                DataSnapshot s = task.Result;
                if (s.Exists) // if we already have fb account in our db, delete older, local acc
                {
                    Debug.LogWarning("we already have fb account in our db");
                    user.userID.userID = s.Child("lastID").Value.ToString();
                    ReadUserDatas(user.userID.userID);
                    // Remove localSaved account from db
                    if (PlayerPrefs.HasKey(localID))
                    {
                        if (PlayerPrefs.GetString(localID) != "" || PlayerPrefs.GetString(localID) != null)
                        {
                            reference.Child("Users").Child(PlayerPrefs.GetString(localID)).RemoveValueAsync();
                        }
                        PlayerPrefs.DeleteKey(localID);
                    }
                }
                else // if it is first time, check if there is locally saved any datas
                {
                    Debug.LogWarning("it is first time");
                    if (PlayerPrefs.HasKey(localID)) // if we have a locally-saved account 
                    {
                        Debug.LogWarning("create new account with fb.userID");
                        // create new account with fb.userID
                        LocalUser.Instance.ID = PlayerPrefs.GetString(localID);
                        Debug.Log(facebookID + " is saving in " + LocalUser.Instance.ID);
                        StartCoroutine(SaveMyFIDToAPPIDs(facebookID, LocalUser.Instance.ID));
                        SaveUserDatas();
                        PlayerPrefs.DeleteKey(localID);
                        // Delete from db, localfacebookID
                    }
                    else // if we do not have a locally-saved account
                    {
                        // create new fresh account
                        Debug.LogWarning("create new fresh account");
                        StartCoroutine(IOS_GetLastID(facebookID));
                    }
                }
            }
            else
            {
                Debug.Log("Reading appID failed");
            }
        });
    }

    IEnumerator SaveMyFIDToAPPIDs(string facebookID, string _lastID) // we user this function, instead of SaveMyIDToAppIDs_SaveUserDatas, because, LastID/lastID should not be changed in this case
    {
        yield return null;
        LastID lastID = new LastID()
        {
            lastID = _lastID
        };
        string savingJson = JsonUtility.ToJson(lastID);
        Debug.Log("Saving json in appIDs: " + savingJson);
        reference.Child("AppIDs").Child(facebookID).SetRawJsonValueAsync(savingJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log(facebookID + " is saved in " + _lastID);
            }
        });
    }



    IEnumerator IOS_GetLastID(string facebookID) // first time
    {
        yield return null;
        reference.Child("LastID").Child("lastID").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot s = task.Result;
                string lastID = s.Value.ToString();
                LocalUser.Instance.nickName = LocalUser.Instance.facebookNickname;
                StartCoroutine(SaveMyIDToAppIDs_SaveUserDatas(facebookID, lastID));
            }
        });
    }

    IEnumerator SaveMyIDToAppIDs_SaveUserDatas(string facebookID, string _lastID)
    {
        yield return null;
        LastID lastID = new LastID()
        {
            lastID = _lastID
        };
        string savingJson = JsonUtility.ToJson(lastID);
        if (facebookID == "" || facebookID == null)
        {
            facebookID = "VIRTUAL";
        }
        reference.Child("AppIDs").Child(facebookID).SetRawJsonValueAsync(savingJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                StartCoroutine(SetLastID(_lastID));
                //RefreshLocalUserDatas(_lastID);
                //LocalUser.Instance.ID = _lastID;
                // set new datas to /users/..
                LocalUser.Instance.ID = _lastID;
                StartCoroutine(SaveUserDatasAfterOneFrame());
                CommonData.Instance.loadingCanvas.SetActive(false);
            }
        });
    }

    #endregion


}

