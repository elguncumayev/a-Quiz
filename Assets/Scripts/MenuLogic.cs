using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuLogic : MonoBehaviour
{
    #region Singleton
    private static MenuLogic _instance;
    public static MenuLogic Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion

    private const int randomCategoryIndex = 24;

    [SerializeField] GraphicRaycaster optionsCanvasRaycaster;

    [SerializeField] TMP_Text menuOverallScore;
    [SerializeField] TMP_Text menuName;
    [SerializeField] Image menuAvatar;

    [SerializeField] TMP_Text allGamesInfo;
    [SerializeField] TMP_Text winGamesInfo;
    [SerializeField] TMP_Text correctAnswersInfo;

    [SerializeField] RectTransform allContentHead;

    [SerializeField] RectTransform waitingGamesHead;
    [SerializeField] RectTransform waitingGamesContentParent;

    [SerializeField] RectTransform friendsHead;
    [SerializeField] RectTransform friendsContentParent;

    [SerializeField] RectTransform finishedGamesHead;
    [SerializeField] RectTransform finishedGamesContentParent;

    //Prefabs
    [SerializeField] GameObject waitingGamePrefab;
    [SerializeField] GameObject friendPanelPrefab;
    [SerializeField] GameObject finishedGamePrefab;

    //OptonsMenu
    [SerializeField] GameObject editNamePanel;
    [SerializeField] RectTransform editNamePopUp;
    [SerializeField] TMP_InputField editNameInput;
    [SerializeField] GameObject editNameInfo;
    [SerializeField] Button editNameApply;

    [SerializeField] GameObject editAvatarPanel;
    [SerializeField] RectTransform editAvatarPopUp;
    [SerializeField] RectTransform editAvatarContent;
    [SerializeField] RectTransform editAvatarScroll;
    [SerializeField] GameObject avatarPrefab;

    [SerializeField] TMP_InputField friendSearchInput;
    [SerializeField] GameObject friendSearchPanel;
    [SerializeField] RectTransform friendSearchPopUp;
    [SerializeField] GameObject searchFriendInfo;
    [SerializeField] Button searchFriendSendButton;

    [SerializeField] TMP_Text optionsName;
    [SerializeField] TMP_Text optionsID;
    [SerializeField] Image optionsAvatar;

    [SerializeField] Image soundOnOffButonImage;

    // First nick name variables
    [SerializeField] TMP_InputField firstNickInput;
    [SerializeField] GameObject createNickNameInfo;
    [SerializeField] Button createNickNameApply;

    private bool friendGame = false;
    private bool ready = false;
    private const int infoPanelsDefaultSize = 220;
    private const int allContentDefaultSize = 2167;
    const string savedSoundVolume = "sv";

    private void Start()
    {
        if (!PlayerPrefs.HasKey(savedSoundVolume)) // if it is first time,
        {
            // turn sound on
            soundOnOffButonImage.sprite = CommonData.Instance.OnSprite;
            PlayerPrefs.SetInt(savedSoundVolume, 1);
        }
        Application.runInBackground = true;
        CommonData.Instance.gameStart = true;
        CommonData.Instance.menuCanvas.SetActive(false);
        CommonData.Instance.optionsCanvas.SetActive(false);
        CommonData.Instance.categoryCanvas.SetActive(false);
        CommonData.Instance.joinRoomCanvas.SetActive(false);
        CommonData.Instance.gameCanvas.SetActive(false);
        CommonData.Instance.endGameCanvas.SetActive(false);
        CommonData.Instance.firstNicknameCanvas.SetActive(false);
    }

    public void LoadingComplete()
    {
        CommonData.Instance.loadingCanvas.SetActive(false);
        CommonData.Instance.menuCanvas.SetActive(true);
    }

    public void WriteDataToUI()
    {
        menuOverallScore.text = LocalUser.Instance.trophy.ToString();
        menuName.text = LocalUser.Instance.nickName;
        optionsName.text = LocalUser.Instance.nickName;
        optionsID.text = LocalUser.Instance.ID;
        menuAvatar.sprite = CommonData.Instance.avatarsRes[LocalUser.Instance.avatarIndex];
        optionsAvatar.sprite = CommonData.Instance.avatarsRes[LocalUser.Instance.avatarIndex];


        allGamesInfo.text = LocalUser.Instance.numberOfAllGames.ToString();
        winGamesInfo.text = LocalUser.Instance.numberOfWonGames.ToString();
        correctAnswersInfo.text = LocalUser.Instance.numberOfCorrectAnswers.ToString();

        float addHeightToHeadContent = 0;

        float addHeight = LocalUser.Instance.waitingGames.Count * 230;
        waitingGamesHead.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, infoPanelsDefaultSize + addHeight);
        addHeightToHeadContent += addHeight;

        foreach(Transform gO in waitingGamesContentParent.transform)
        {
            Destroy(gO.gameObject);
        }

        for (int i = 0; i < LocalUser.Instance.waitingGames.Count; i++)
        {
            GameObject wGNew = Instantiate(waitingGamePrefab, waitingGamesContentParent);
            if (!LocalUser.Instance.waitingGames[i].finished)
            {
                if (LocalUser.Instance.waitingGames[i].gameWithFriend)
                {
                    if (LocalUser.Instance.waitingGames[i].amIHost)
                    {
                        wGNew.GetComponent<Button>().interactable = false;
                        wGNew.GetComponent<WGPrefabInfo>().category.text = CommonData.Instance.categoryNames[LocalUser.Instance.waitingGames[i].category];
                        wGNew.GetComponent<WGPrefabInfo>().infoText.text = string.Format("You {0} - 0 {1}", LocalUser.Instance.waitingGames[i].myScore, LocalUser.Instance.waitingGames[i].oppName);
                        wGNew.GetComponent<WGPrefabInfo>().oppAvatar.sprite = CommonData.Instance.avatarsRes[LocalUser.Instance.waitingGames[i].oppAvatarIndex];
                        wGNew.GetComponent<WGPrefabInfo>().getResultText.SetActive(false);
                    }
                    else
                    {
                        wGNew.GetComponent<Button>().interactable = true;
                        wGNew.GetComponent<WGPrefabInfo>().category.text = CommonData.Instance.categoryNames[LocalUser.Instance.waitingGames[i].category];
                        wGNew.GetComponent<WGPrefabInfo>().infoText.text = string.Format("You - {1}", LocalUser.Instance.waitingGames[i].myScore, LocalUser.Instance.waitingGames[i].oppName);
                        wGNew.GetComponent<WGPrefabInfo>().oppAvatar.sprite = CommonData.Instance.avatarsRes[LocalUser.Instance.waitingGames[i].oppAvatarIndex];
                        wGNew.GetComponent<WGPrefabInfo>().getResultText.GetComponent<TMP_Text>().text = "it is your turn";
                        wGNew.GetComponent<WGPrefabInfo>().getResultText.SetActive(true);
                    }
                }
                else
                {
                    wGNew.GetComponent<Button>().interactable = false;
                    wGNew.GetComponent<WGPrefabInfo>().category.text = CommonData.Instance.categoryNames[LocalUser.Instance.waitingGames[i].category];
                    wGNew.GetComponent<WGPrefabInfo>().infoText.text = string.Format("You {0} - 0 New Player", LocalUser.Instance.waitingGames[i].myScore);
                    wGNew.GetComponent<WGPrefabInfo>().oppAvatar.sprite = CommonData.Instance.unknownPlayer;
                    wGNew.GetComponent<WGPrefabInfo>().getResultText.SetActive(false);
                }
            }
            else
            {
                wGNew.GetComponent<Button>().interactable = true;
                wGNew.GetComponent<WGPrefabInfo>().category.text = CommonData.Instance.categoryNames[LocalUser.Instance.waitingGames[i].category];
                wGNew.GetComponent<WGPrefabInfo>().infoText.text = string.Format("You - {0}", LocalUser.Instance.waitingGames[i].oppName);
                wGNew.GetComponent<WGPrefabInfo>().oppAvatar.sprite = CommonData.Instance.avatarsRes[LocalUser.Instance.waitingGames[i].oppAvatarIndex];
                wGNew.GetComponent<WGPrefabInfo>().getResultText.SetActive(true);
            }
            wGNew.GetComponent<WGPrefabInfo>().index = i;
        }

        addHeight = (LocalUser.Instance.friends.Count + LocalUser.Instance.friendRequests.Count) * 230;
        friendsHead.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, infoPanelsDefaultSize + addHeight);
        addHeightToHeadContent += addHeight;

        foreach (Transform gO in friendsContentParent.transform)
        {
            Destroy(gO.gameObject);
        }

        foreach (FriendData fRequest in LocalUser.Instance.friendRequests)
        {
            GameObject fNew = Instantiate(friendPanelPrefab, friendsContentParent);
            fNew.GetComponent<FPrefabInfo>().infoText.text = "friend request";
            fNew.GetComponent<FPrefabInfo>().friendData = fRequest;
            fNew.GetComponent<FPrefabInfo>().friendName.text = fRequest.friendNickName;
            fNew.GetComponent<FPrefabInfo>().friendAvatar.sprite = CommonData.Instance.requstIcon;
            fNew.GetComponent<FPrefabInfo>().isRequest = true;
        }

        foreach (FriendData fr in LocalUser.Instance.friends)
        {
            GameObject fNew = Instantiate(friendPanelPrefab, friendsContentParent);
            fNew.GetComponent<FPrefabInfo>().friendData = fr;
            fNew.GetComponent<FPrefabInfo>().friendName.text = fr.friendNickName;
            fNew.GetComponent<FPrefabInfo>().friendAvatar.sprite = CommonData.Instance.avatarsRes[fr.friendAvatarIndex];
            fNew.GetComponent<FPrefabInfo>().isRequest = false;
        }
        
        addHeight = LocalUser.Instance.finishedGames.Count * 230;
        finishedGamesHead.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, infoPanelsDefaultSize + addHeight);
        addHeightToHeadContent += addHeight;

        foreach (Transform gO in finishedGamesContentParent.transform)
        {
            Destroy(gO.gameObject);
        }

        foreach (FinishedMatchInfo fG in LocalUser.Instance.finishedGames)
        {
            GameObject fGNew = Instantiate(finishedGamePrefab, finishedGamesContentParent);
            Debug.Log("fg.category: " + fG.category);
            Debug.Log("CommonData.Instance.categoryNames[0]: " + CommonData.Instance.categoryNames[0]);
            fGNew.GetComponent<FGPrefabInfo>().category.text = CommonData.Instance.categoryNames[fG.category];
            fGNew.GetComponent<FGPrefabInfo>().infoText.text = string.Format("You {0} - {1} {2}",fG.myScore,fG.oppScore, fG.oppName);
            fGNew.GetComponent<FGPrefabInfo>().oppAvatar.sprite = CommonData.Instance.avatarsRes[fG.oppAvatarIndex];
        }

        CommonData.Instance.UILoaded = true;
        if (CommonData.Instance.dataLoaded && CommonData.Instance.gameStart)
        {
            CommonData.Instance.gameStart = false;
            LoadingComplete();
        }

        allContentHead.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, allContentDefaultSize + addHeightToHeadContent);
    }

    public void OnClick_StartRandom()
    {
        CommonData.Instance.categoryCanvas.SetActive(true);
        CommonData.Instance.menuCanvas.SetActive(false);
    }

    public void OnClick_StartWithFriend()
    {
        friendGame = true;
        OnClick_FriendPanelClose();
        CommonData.Instance.categoryCanvas.SetActive(true);
        CommonData.Instance.menuCanvas.SetActive(false);
    }

    public void OnClick_CategoryClose()
    {
        friendGame = false;
        CommonData.Instance.menuCanvas.SetActive(true);
        CommonData.Instance.categoryCanvas.SetActive(false);
    }

    public void OnClick_StartWithCategory(int category)
    {
        int lastCategoryChoice;
        lastCategoryChoice = (category == randomCategoryIndex) ? new System.Random().Next(24) : category;
        Debug.Log("Last category Index sent : " + lastCategoryChoice);
        if (friendGame)
        {
            CommonData.Instance.gameLogic.StartNewGame(friendGame, category);
            friendGame = false;
        }
        else FBManager.Instance.JoinRandomRoom(lastCategoryChoice);
    }

    public void OnClick_OptionMenuOpen()
    {
        CommonData.Instance.optionsCanvas.SetActive(true);
        CommonData.Instance.menuCanvas.SetActive(false);
    }
    
    public void OnClick_OptionMenuClose()
    {
        CommonData.Instance.optionsCanvas.SetActive(false);
        CommonData.Instance.menuCanvas.SetActive(true);
    }

    public void OnClick_FriendRequestPanelDecline()
    {
        FBManager.Instance.DeclineFriendRequest(CommonData.Instance.currentSelectedFriend.friendID);
        LocalUser.Instance.friendRequests.RemoveAll(req => req.friendID.Equals(CommonData.Instance.currentSelectedFriend.friendID));
        WriteDataToUI();
        LeanTween.scale(CommonData.Instance.frRequestPopUp, Vector2.zero, .25f).setEaseInBack().setOnComplete(() => 
        {
            CommonData.Instance.frRequestBack.SetActive(false);
        });
    }
    
    public void OnClick_FriendRequestPanelAccept()
    {
        FBManager.Instance.AcceptFriendRequest(CommonData.Instance.currentSelectedFriend.friendID);
        FriendData fD = LocalUser.Instance.friendRequests.Find(req => req.friendID.Equals(CommonData.Instance.currentSelectedFriend.friendID));
        LocalUser.Instance.friendRequests.RemoveAll(req => req.friendID.Equals(CommonData.Instance.currentSelectedFriend.friendID));
        LocalUser.Instance.friends.Add(fD);
        WriteDataToUI();
        LeanTween.scale(CommonData.Instance.frRequestPopUp, Vector2.zero, .25f).setEaseInBack().setOnComplete(() => 
        {
            CommonData.Instance.frRequestBack.SetActive(false);
        });
    }
    
    public void OnClick_RemoveFriendQueryOpen()
    {
        CommonData.Instance.friendRemoveBack.SetActive(true);
        LeanTween.scale(CommonData.Instance.friendRemoveAreYouSure, Vector2.one, .25f).setEaseOutBack();
    }
    
    public void OnClick_RemoveFriendQueryClose()
    {
        LeanTween.scale(CommonData.Instance.friendRemoveAreYouSure, Vector2.zero, .25f).setEaseInBack().setOnComplete(() =>
        {
            CommonData.Instance.friendRemoveBack.SetActive(false);
        });
    }
    
    public void OnClick_RemoveFriend()
    {
        FBManager.Instance.RemoveFriend(CommonData.Instance.currentSelectedFriend.friendID);
        LocalUser.Instance.friends.RemoveAll(req => req.friendID.Equals(CommonData.Instance.currentSelectedFriend.friendID));
        LeanTween.scale(CommonData.Instance.friendRemoveAreYouSure, Vector2.zero, .25f).setEaseInBack().setOnComplete(() =>
        {
            CommonData.Instance.friendRemoveBack.SetActive(false);
        });
        OnClick_FriendPanelClose();
        WriteDataToUI();
    }

    public void OnClick_FriendPanelClose()
    {
        LeanTween.scale(CommonData.Instance.friendPopUp, Vector2.zero, .25f).setEaseInBack().setOnComplete(() => 
        {
            CommonData.Instance.friendBack.SetActive(false);
        });
    }

    public void OnClick_EditNamePanelOpen()
    {
        editNamePanel.SetActive(true);
        editNameInfo.SetActive(true);
        editNameApply.interactable = false;
        LeanTween.scale(editNamePopUp, Vector2.one, .25f).setEaseOutBack();
    }

    public void OnClick_ApplyNewName()
    {
        LocalUser.Instance.nickName = editNameInput.text;
        optionsName.text = editNameInput.text;
        menuName.text = editNameInput.text;
        editNameApply.interactable = false;
        FBManager.Instance.SaveUserPrivateDatas();
        OnClick_CloseEditNamePanel();
    }

    public void OnEditNameValueChange()
    {
        string text = editNameInput.text;
        if(text.Length > 2)
        {
            editNameInfo.SetActive(false);
            editNameApply.interactable = true;
        }
        else
        {
            editNameInfo.SetActive(true);
            editNameApply.interactable = false;
        }
    }

    public void OnClick_CloseEditNamePanel()
    {
        editNameInput.text = string.Empty;
        editNameInfo.SetActive(false);
        editNameApply.interactable = false;
        LeanTween.scale(editNamePopUp, Vector2.zero, .25f).setEaseInBack().setOnComplete(()=>
        {
            editNamePanel.SetActive(false);
        });
    }

    public void OnClick_EditAvatarPanelOpen()
    {
        editAvatarPanel.SetActive(true);
        if (!ready)
        {
            for (int i = 0; i < CommonData.Instance.avatarsRes.Length; i++)
            {
                GameObject avatar = Instantiate(avatarPrefab, editAvatarContent);
                avatar.GetComponent<Image>().sprite = CommonData.Instance.avatarsRes[i];
                avatar.GetComponent<AvatarIndex>().index = i;
            }
            ready = true;
        }
        LeanTween.scale(editAvatarPopUp, Vector2.one, .25f).setEaseOutBack();
    }

    public void OnClick_ApplyNewAvatar()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = editAvatarScroll.position
        };
        List<RaycastResult> results = new List<RaycastResult>();
        optionsCanvasRaycaster.Raycast(pointerData, results);

        if (results.Count != 0 && results[0].gameObject.CompareTag("a"))
        {
            LocalUser.Instance.avatarIndex = results[0].gameObject.GetComponent<AvatarIndex>().index;
            optionsAvatar.sprite = CommonData.Instance.avatarsRes[LocalUser.Instance.avatarIndex];
            menuAvatar.sprite = CommonData.Instance.avatarsRes[LocalUser.Instance.avatarIndex];
            FBManager.Instance.SaveUserPrivateDatas();
        }

        LeanTween.scale(editAvatarPopUp, Vector2.zero, .25f).setEaseInBack().setOnComplete(() =>
        {
            editAvatarPanel.SetActive(false);
        });
    }

    public void OnClick_FriendSearchOpen()
    {
        searchFriendSendButton.interactable = false;
        friendSearchPanel.SetActive(true);
        LeanTween.scale(friendSearchPopUp, Vector2.one, .25f).setEaseOutBack();
    }

    public void OnSearchFriendValueChange()
    {
        string text = friendSearchInput.text;
        if(text.Length == 7 && Regex.IsMatch(text, @"^[a-zA-Z]+$"))
        {
            searchFriendInfo.SetActive(false);
            searchFriendSendButton.interactable = true;
        }
        else
        {
            searchFriendInfo.SetActive(true);
            searchFriendSendButton.interactable = false;
        }
    }
    
    public void OnClick_FriendSearchClose()
    {
        friendSearchInput.text = string.Empty;
        LeanTween.scale(friendSearchPopUp, Vector2.zero, .25f).setEaseInBack().setOnComplete(() =>
        {
            friendSearchPanel.SetActive(false);
        });
    }

    public void OnClick_SendFriendrequest()
    {
        string friendID = friendSearchInput.text.ToUpper();
        if (!friendID.Equals(LocalUser.Instance.ID))
        {
            FBManager.Instance.SendFriendRequest(friendID);
        }
        friendSearchInput.text = string.Empty;
        LeanTween.scale(friendSearchPopUp, Vector2.zero, .25f).setEaseInBack().setOnComplete(() =>
        {
            friendSearchPanel.SetActive(false);
        });
    }

    public void OnClick_Refresh()
    {
        CommonData.Instance.gameStart = true;
        CommonData.Instance.UILoaded = false;
        CommonData.Instance.loadingCanvas.SetActive(true);
        CommonData.Instance.optionsCanvas.SetActive(false);
        FBManager.Instance.ReadUserDatas(LocalUser.Instance.ID);
    }

    public void OnClick_SoundOnOff()
    {
        if (PlayerPrefs.GetInt(savedSoundVolume) == 0)
        {
            // turn sound on
            soundOnOffButonImage.sprite = CommonData.Instance.OnSprite;
            PlayerPrefs.SetInt(savedSoundVolume, 1);
        }
        else
        {
            // turn sound off
            soundOnOffButonImage.sprite = CommonData.Instance.OffSprite;
            PlayerPrefs.SetInt(savedSoundVolume, 0);
        }
    }

    public void OnClick_SetFirstNickName()
    {
        LocalUser.Instance.nickName = firstNickInput.text;
        FBManager.Instance.GetLastID_LocallySaved();
    }

    public IEnumerator OpenFirstNicknamePanel()
    {
        Debug.Log("-------------OpenFirstNicknamePanel----------------");
        yield return null;
        CommonData.Instance.loadingCanvas.SetActive(false);
        CommonData.Instance.firstNicknameCanvas.SetActive(true);
    }

    public void OnSetFirstNickNameValueChange()
    {
        string text = firstNickInput.text;
        if (text.Length > 2)
        {
            createNickNameInfo.SetActive(false);
            createNickNameApply.interactable = true;
        }
        else
        {
            createNickNameInfo.SetActive(true);
            createNickNameApply.interactable = false;
        }
    }


}
