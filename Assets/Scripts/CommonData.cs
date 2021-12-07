using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonData : MonoBehaviour
{
    #region Singleton
    private static CommonData _instance;
    public static CommonData Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
        colors = new Color32[]{
            new Color32(0, 255, 0,255),
            new Color32(42, 255, 0,255),
            new Color32(84, 253, 0,255),
            new Color32(126, 254, 0,255),
            new Color32(168, 255, 0,255),
            new Color32(210, 255, 0,255),
            new Color32(255, 255, 0,255),
            new Color32(0, 255, 0,255) };
    }
    #endregion

    //LoadindScreen
    public Image[] circles;
    public TMP_Text joiningRoomHostName;
    //

    //Friend Pop-Up
    public GameObject friendBack;
    public RectTransform friendPopUp;
    public Image friendPopUpAvatar;
    public TMP_Text friendPopUpName;
    public TMP_Text friendPopUpTrophy;
    public GameObject friendRemoveBack;
    public RectTransform friendRemoveAreYouSure;
    public FriendData currentSelectedFriend;
    //

    //Friend Request Pop-Up
    public GameObject frRequestBack;
    public RectTransform frRequestPopUp;
    public TMP_Text frReqTextPopUp;
    //

    //Sprite Resources
    public Sprite OnSprite;
    public Sprite OffSprite;
    public Sprite[] avatarsRes;
    public Sprite requstIcon;
    public Sprite unknownPlayer;
    public Sprite[] answersSprites;//0 - common ; 1 - true ; 2 - false
    //

    //Canvasaes And Logic Scripts
    public GameObject loadingCanvas;
    public GameObject menuCanvas;
    public GameObject optionsCanvas;
    public GameObject categoryCanvas;
    public GameObject joinRoomCanvas;
    public GameObject gameCanvas;
    public GameObject endGameCanvas;
    public GameObject firstNicknameCanvas;
    public GameLogic gameLogic;
    public EndGameLogic endGameLogic;
    //

    //EndScreen
    public GameObject[] myPS;
    public GameObject[] oppPS;
    //

    //LoadingScreenCircles
    [HideInInspector] public Color32[] colors;
    //

    //Loadings
    [HideInInspector] public bool UILoaded = false;
    [HideInInspector] public bool dataLoaded = false;
    [HideInInspector] public bool gameStart;
    [HideInInspector] public bool quizEnd = false;
    //

    [HideInInspector] public int myOverallScore;

    //Constants
    [HideInInspector] public readonly int numberOfAllQuestions = 1214;
    [HideInInspector] public readonly int numberOfCategories = 24;
    [HideInInspector] public string[] categoryNames = { "General Knowledge",
                                                        "Books",
                                                        "Film",
                                                        "Music",
                                                        "Musicals & Theatres",
                                                        "Television",
                                                        "Video Games",
                                                        "Board Games",
                                                        "Science & Nature",
                                                        "Science: Computers",
                                                        "Science: Mathematics",
                                                        "Mythology",
                                                        "Sports",
                                                        "Geography",
                                                        "History",
                                                        "Politics",
                                                        "Art",
                                                        "Celebrities",
                                                        "Animals",
                                                        "Vehicles",
                                                        "Comics",
                                                        "Science: Gadgets",
                                                        "Japanese Anime & Manga",
                                                        "Cartoon & Animations" };
}
