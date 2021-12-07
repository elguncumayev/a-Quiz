using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using UnityEngine.UI;
using System;

public class FacebookController : MonoBehaviour
{
    // public GameObject Panel_Add;
    //public Text FB_userName;
    //public RawImage FB_useerDp;
    //public RawImage FB_useerDp_Raw;
    //public GameObject friendstxtprefab;
    //public GameObject GetFriendsPos;
    private static readonly string EVENT_PARAM_SCORE = "score";
    private static readonly string EVENT_NAME_GAME_PLAYED = "game_played";

    private readonly string facebookID = "fbID";
    private readonly string localID = "lclID";

    private readonly string appIsOpenedAtLeastOnce = "appOpen";

    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                if (FB.IsInitialized)
                    FB.ActivateApp();
                else
                    Debug.LogError("Couldn't initialize");
            },
            isGameShown =>
            {
                if (!isGameShown)
                    Time.timeScale = 0;
                else
                    Time.timeScale = 1;
            });
        }
        else
            FB.ActivateApp();
    }

    /*
     1) at start:
        --- it checks facebookID from playerprefs
        --- if there is facebookID in  playerprefs, get datas from db by that id
        --- else, do everything locally. Read facebookID from local, if there is one, continue with that. Else, create new account in local.
        --- But how can you play without having account in db, others cannot play with you normal
        --- So, the idea is to create an account in db as well.
        --- We have to store locally the id that is stored in db, so everytime user plays without login, it reads from the locally-saved id.
        --- So, we store 2nd id, called localfacebookID.
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
    private void Start()
    {
        Debug.Log("facebookID:_" + PlayerPrefs.GetString(facebookID) + "_");
        Debug.Log("localfacebookID:_" + PlayerPrefs.GetString(localID) + "_");
        if (PlayerPrefs.HasKey(facebookID)) // if we have an accountID that is connected to facebook, saved in playerprefs
        {
            LocalUser.Instance.facebookID = PlayerPrefs.GetString(facebookID);
            // read from db by the saved facebook id
            FBManager.Instance.ReadUserDatasByFacebook(PlayerPrefs.GetString(facebookID));
        }
        else // if we do not have a fb-connected id in playerprefs
        {
            if (PlayerPrefs.HasKey(localID)) // if we have a saved id, that is not connected to fb
            {
                // read from db by the locally-saved id
                FBManager.Instance.ReadUserDatas(PlayerPrefs.GetString(localID));
            }
            else // if we do not have either acc connected with fb, or account saved locally
            {
                // open first nickname panel, then, GetLastID_LocallySaved();
                StartCoroutine(MenuLogic.Instance.OpenFirstNicknamePanel());
                // create new acc in db and save it to local
                // FBManager.Instance.GetLastID_LocallySaved();
            }
        }
    }

    //private void Start()
    //{
    //    if (PlayerPrefs.HasKey(facebookID)) // if app is opened at least once
    //    {
    //        if (PlayerPrefs.HasKey(facebookID)) // if we have an account saved in playerprefs
    //        {
    //            // read from db by facebook id
    //            fbManager.ReadUserDatas(PlayerPrefs.GetString(facebookID));
    //        }
    //        else
    //        {
    //            // do nothing, start from 0, if user presses log in,
    //            //  if user has a aquiz account in that fb account, read fb datas from db.
    //            //  else if user doesnt have an fb account, save current aquiz datas to fb account in db
    //            // load datas from local, if they exist
    //            FBManager.Instance.LoadPlayerFromLocal();
    //        }
    //    }
    //    else // if it is first time opening this app
    //    {
    //        // do nothing, start from 0, if user presses log in,
    //        //  if user has a aquiz account in that fb account, read fb datas from db.
    //        //  else if user doesnt have an fb account, save current aquiz datas to fb account in db
    //        // save to local
    //        FBManager.Instance.SavePlayerLocally();
    //    }
    //}

    //void SetInit()
    //{
    //    if (FB.IsLoggedIn)
    //    {
    //        Debug.Log("Facebook is Logged in!");
    //    }
    //    else
    //    {
    //        Debug.Log("Facebook is not Logged in!");
    //    }
    //    DealWithFbMenus(FB.IsLoggedIn);
    //}

    //void OnHidenUnity(bool isGameShown)
    //{
    //    if (!isGameShown)
    //    {
    //        Time.timeScale = 0;
    //    }

    //    else
    //    {
    //        Time.timeScale = 1;
    //    }
    //}

    public void Facebook_LogIn_LogOut()
    {
        if (FB.IsLoggedIn)
        {
            CallLogout();
        }
        else
        {
            FBLogin();
        }
    }

    public void FBLogin()
    {
        List<string> permissions = new List<string>
        {
            "public_profile",

            "user_friends"
        };
        FB.LogInWithReadPermissions(permissions, AuthCallBack);
    }



    public void CallLogout()
    {
        StartCoroutine(FBLogout());
    }

    IEnumerator FBLogout()
    {
        FB.LogOut();
        while (FB.IsLoggedIn)
        {
            print("Logging Out");

            yield return null;
        }

        print("Logout Successful");

        //FB_useerDp.texture = null;

        //FB_userName.text = "";

    }





    //public void GetFriendsPlayingThisGame()
    //{
    //    string query = "/me/friends";

    //    FB.API(query, HttpMethod.GET, result =>
    //    {

    //        Debug.Log("the raw" + result.RawResult);

    //        var dictionary = (Dictionary<string, object>)Facebook.MiniJSON.Json.Deserialize(result.RawResult);

    //        var friendsList = (List<object>)dictionary["data"];

    //        foreach (var dict in friendsList)
    //        {
    //            GameObject go = Instantiate(friendstxtprefab);

    //            go.GetComponent<Text>().text = ((Dictionary<string, object>)dict)["name"].ToString();

    //            go.transform.SetParent(GetFriendsPos.transform, false);



    //            //  FriendsText[1].text += ((Dictionary<string, object>)dict)["name"];

    //        }
    //    });
    //}

    //public void FacebookSharefeed()
    //{
    //    string url = "https://developers.facebook.com/docs/unity/reference/current/FB.ShareLink";

    //    FB.ShareLink(

    //        new Uri(url),

    //        "Checkout unity3d teacher channel",

    //        "I just watched " + "22" + " times of this channel",

    //        null,
    //        ShareCallback);
    //}

    //private static void ShareCallback(IShareResult result)
    //{
    //    Debug.Log("ShareCallback");
    //    SpentCoins(2, "sharelink");
    //    if (result.Error != null)
    //    {
    //        Debug.LogError(result.Error);
    //        return;
    //    }
    //    Debug.Log(result.RawResult);
    //}

    // Start is called before the first frame update

    void AuthCallBack(IResult result)
    {
        if (result.Error != null)
        {
            Debug.Log(result.Error);
        }

        else
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("Facebook is Login!");
            }
            else
            {
                Debug.Log("Facebook is not Logged in!");
            }
            DealWithFbMenus(FB.IsLoggedIn);
            
        }
    }



    void DealWithFbMenus(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            FB.API("/me?fields=first_name", HttpMethod.GET, DisplayUsername);

            //FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayProfilePic);
        }
        else
        {
        }

    }

    void DisplayUsername(IResult result)
    {
        if (result.Error == null)
        {
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            //FBManager.Instance.RefreshLocalUserDatas("");
            string name = "" + result.ResultDictionary["first_name"];
            LocalUser.Instance.facebookNickname = name;
            //FB_userName.text = name;
            //Debug.Log("username: " + name);
            PlayerPrefs.SetString(facebookID, aToken.UserId);
            FBManager.Instance.ReadUserDatasByFacebook(aToken.UserId);
        }
        else
        {
            Debug.Log(result.Error);
        }
    }



    //void DisplayProfilePic(IGraphResult result)
    //{
    //    if (result.Texture != null)
    //    {
    //        Debug.Log("Profile Pic");
    //        //FB_useerDp.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
    //        FB_useerDp.texture = result.Texture;
    //    }

    //    else
    //    {
    //        Debug.Log(result.Error);
    //    }
    //}

    //public static void SpentCoins(int coins, string item)
    //{
    //    // setup parameters
    //    var param = new Dictionary<string, object>();
    //    param[AppEventParameterName.ContentID] = item;
    //    // log event
    //    FB.LogAppEvent(AppEventName.SpentCredits, (float)coins, param);
    //}
}