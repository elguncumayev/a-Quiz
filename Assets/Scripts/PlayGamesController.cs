using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;
using UnityEngine.UI;

public class PlayGamesController : MonoBehaviour
{
    #region Singleton

    private static PlayGamesController instance;

    public static PlayGamesController Instance { get => instance; }

    private void Awake()
    {
        instance = this;
    }

    #endregion

    [SerializeField] Image playGamesSignInButonImage;
    [HideInInspector]public string firstNickName;
    public string currentID = "";
    void Start()
    {
        //Initialize();
    }
    void Initialize()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestServerAuthCode(false).
            EnableSavedGames().
            Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        Debug.Log("playgames initialized");
        SignInWithPlayGames();
    }
    void SignInWithPlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (success) =>
        {
            switch (success)
            {
                case SignInStatus.Success:
                    playGamesSignInButonImage.sprite = CommonData.Instance.OnSprite;
                    Debug.Log("signined in player using play games successfully");
                    ReadOrSaveToCloud(false);
                    break;
                default:
                    Debug.Log("Signin not successfull");
                    playGamesSignInButonImage.sprite = CommonData.Instance.OffSprite;
                    break;
            }
        });
    }

    public void SignInOutPlayGames()
    {
        if (Social.localUser.authenticated)
        {
            currentID = LocalUser.Instance.ID;
            SignOutFromPlayGames();
            Initialize();
        }
        else
        {
            Initialize();
        }
    }

    void SignOutFromPlayGames()
    {
        PlayGamesPlatform.Instance.SignOut();
        playGamesSignInButonImage.sprite = CommonData.Instance.OffSprite;
    }

    //cloud saving
    private bool issaving = false;
    private string SAVE_NAME = "SavedGames";
    public void ReadOrSaveToCloud(bool saving)
    {
        if (Social.localUser.authenticated)
        {
            issaving = saving;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                (SAVE_NAME, GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, OpenSavedGame);
        }
    }

    private string GetDataToStoreinCloud()//  we seting the value that we are going to store the data in cloud
    {
        string Data = "";
        Data += FBManager.Instance.lastIDString;
        return Data;
    }

    private void LoadDataFromCloudToOurGame(string savedata)
    {
        Debug.Log("-------------------------------------------SavedData:_" + savedata + "_-------------------------------------------");

        //if (currentID != FBManager.Instance.user.userID.userID) // signed in to different account
        //{
        //    FBManager.Instance.RefreshLocalUserDatas(savedata);
        //    CommonData.Instance.gameStart = true;
        //    CommonData.Instance.loadingCanvas.SetActive(true);
        //}

        FBManager.Instance.RefreshLocalUserDatas(savedata);
        CommonData.Instance.gameStart = true;
        CommonData.Instance.loadingCanvas.SetActive(true);

        firstNickName = Social.localUser.userName;
        if (savedata == "" || savedata == null) // if it is first time entering game with this account
        {
            Debug.Log("First Username: " + firstNickName);
            FBManager.Instance.GetLastID();
        }
        // if we already have an account
        else
        {
            FBManager.Instance.user.userID.userID = savedata;
            FBManager.Instance.ReadUserDatas(savedata);
        }
    }

    private void OpenSavedGame(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            if (issaving)//if is saving is true we are saving our data to cloud
            {
                byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(GetDataToStoreinCloud());
                SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder().Build();
                ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(meta, update, data, SaveCallback);
            }
            else//if is saving is false we are opening our saved data from cloud
            {
                ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(meta, ReadDataFromCloud);
            }
        }
    }

    private void ReadDataFromCloud(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            string savedata = System.Text.ASCIIEncoding.ASCII.GetString(data);
            LoadDataFromCloudToOurGame(savedata);
        }
    }

    private void SaveCallback(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        //use this to debug whether the game is uploaded to cloud
        Debug.Log ( "successfully add data to cloud" );
    }



}
