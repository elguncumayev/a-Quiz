using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    //InGamUI
    [SerializeField] TMP_Text myOverallScore;
    [SerializeField] TMP_Text questionText;
    [SerializeField] TMP_Text questionNum;
    [SerializeField] TMP_Text answersInfo;
    [SerializeField] GameObject[] answers;
    [SerializeField] TMP_Text[] answersText;
    [SerializeField] GameObject continueButton;
    //

    //Timer
    [SerializeField] RectTransform clockHand;
    [SerializeField] Image fill;
    //

    private StringBuilder sb;
    private Question[] currentQuestions;

    private int[] currentGameMyAnswers;
    private int[] currentShuffledArray;

    private int currentCorrectAnswerIndex;
    private int currentQuestionIndex;
    private int numberOfQuestions;
    private int questionCounter;
    private int trueAnswersCounter;

    //Timer
    private Coroutine timerUpdate;
    private const float time = 20f;
    private float currentTime;
    private DateTime lastPausedTime;
    private bool initialFocus = true;
    //


    private bool newGame;
    private bool friendGame = false;
    private bool answerSelected = false;
    private bool gameStart = false;
    //Data
    private CreatedRoomDatas addedCommonGame; //For General waiting games
    private WaitingGames addedWaitingGame; // for me

    private FinishedMatchInfo addedFinishedGame; // for me

    private System.Random random;
    private void Start()
    {
        sb = new StringBuilder();
        currentGameMyAnswers = new int[10];
        random = new System.Random();
    }

    public void StartNewGame(bool friendGame, int category)
    {
        gameStart = true;
        this.friendGame = friendGame;
        if (friendGame)
        {
            addedWaitingGame = new WaitingGames()
            {
                category = category,
                myID = LocalUser.Instance.ID,

                oppName = CommonData.Instance.currentSelectedFriend.friendNickName,
                oppAvatarIndex = CommonData.Instance.currentSelectedFriend.friendAvatarIndex,
                opID = CommonData.Instance.currentSelectedFriend.friendID
            };
            currentQuestions = DataManager.Instance.RandomQuestions(category);
        }
        else
        {
            addedWaitingGame = new WaitingGames()
            {
                category = LocalUser.Instance.currentCreatedGame.category,
                myID = LocalUser.Instance.ID,
            };
            addedCommonGame = new CreatedRoomDatas()
            {
                category = LocalUser.Instance.currentCreatedGame.category,
                hostID = LocalUser.Instance.ID,
                hostNickname = LocalUser.Instance.nickName,
                hostAvatarIndex = LocalUser.Instance.avatarIndex
            };
            currentQuestions = DataManager.Instance.RandomQuestions(LocalUser.Instance.currentCreatedGame.category);
        }
        newGame = true;

        ChangeCanvasToGame();
        myOverallScore.text = CommonData.Instance.myOverallScore.ToString();
        questionCounter = numberOfQuestions = 10;
        trueAnswersCounter = 0;
        currentQuestionIndex = 0;
        PlaceQuestionsAndAnswers();
    }

    public void JoinGame(bool frGame, int index)
    {
        gameStart = true;
        StartCoroutine(JoinCreatedGame(frGame, index));
    }

    public IEnumerator JoinCreatedGame(bool frGame, int index)
    {
        friendGame = frGame;
        if (frGame)
        {
            addedWaitingGame = new WaitingGames()
            {
                category = LocalUser.Instance.waitingGames[index].category,
                myID = LocalUser.Instance.ID,
                amIHost = false,

                oppScore = LocalUser.Instance.waitingGames[index].oppScore,
                oppName = LocalUser.Instance.waitingGames[index].oppName,
                oppAvatarIndex = LocalUser.Instance.waitingGames[index].oppAvatarIndex,
                opID = LocalUser.Instance.waitingGames[index].opID,
                questionIndexes = LocalUser.Instance.waitingGames[index].questionIndexes
            };
            sb.Append(LocalUser.Instance.waitingGames[index].oppName).Append("'s");
            CommonData.Instance.joiningRoomHostName.text = sb.ToString();
            sb.Clear();
            currentQuestions = DataManager.Instance.GetSpecificQuestions(LocalUser.Instance.waitingGames[index].questionIndexes);
            LocalUser.Instance.waitingGames.RemoveAt(index);
        }
        else
        {
            addedFinishedGame = new FinishedMatchInfo()
            {
                opID = LocalUser.Instance.currentCreatedGame.hostID,
                oppName = LocalUser.Instance.currentCreatedGame.hostNickname,
                oppAvatarIndex = LocalUser.Instance.currentCreatedGame.hostAvatarIndex,
                oppScore = LocalUser.Instance.currentCreatedGame.hostScore,
                category = LocalUser.Instance.currentCreatedGame.category
            };

            addedWaitingGame = new WaitingGames()
            {
                category = LocalUser.Instance.currentCreatedGame.category,
                myID = LocalUser.Instance.ID,
                oppScore = LocalUser.Instance.currentCreatedGame.hostScore,
                oppName = LocalUser.Instance.currentCreatedGame.hostNickname,
                oppAvatarIndex = LocalUser.Instance.currentCreatedGame.hostAvatarIndex,
                opID = LocalUser.Instance.currentCreatedGame.hostID
            };
            sb.Append(LocalUser.Instance.currentCreatedGame.hostNickname).Append("'s");
            CommonData.Instance.joiningRoomHostName.text = sb.ToString();
            sb.Clear();
            currentQuestions = DataManager.Instance.GetSpecificQuestions(LocalUser.Instance.currentCreatedGame.questionIndexes);
        }

        newGame = false;

        
        CommonData.Instance.joinRoomCanvas.SetActive(true);
        CommonData.Instance.categoryCanvas.SetActive(false);
        CommonData.Instance.menuCanvas.SetActive(false);
        yield return new WaitForSecondsRealtime(2);
        CommonData.Instance.joinRoomCanvas.SetActive(false);
        ChangeCanvasToGame();
        myOverallScore.text = CommonData.Instance.myOverallScore.ToString();
        questionCounter = numberOfQuestions = 10;
        trueAnswersCounter = 0;
        currentQuestionIndex = 0;
        PlaceQuestionsAndAnswers();
    }

    private IEnumerator TimerUpdate()
    {
        clockHand.rotation = Quaternion.identity;
        if(currentTime >= time)
        {
            fill.fillAmount = 1f;
            clockHand.rotation = Quaternion.identity;
        }
        while (currentTime < time)
        {
            currentTime += Time.fixedDeltaTime;
            fill.fillAmount = currentTime / time;
            clockHand.Rotate(Vector3.forward, Time.fixedDeltaTime / time * -360);
            if(currentTime >= time)
            {
                OnClick_Answer(-1);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    void OnApplicationPause(bool isGamePause)
    {
        if (isGamePause)
        {
            lastPausedTime = DateTime.Now;
        }
    }

    void OnApplicationFocus(bool isGameFocus)
    {
        //Debug.Log("OnApplicationFocus started: " + isGameFocus);
        if (isGameFocus)
        {
            if (initialFocus)
            {
                initialFocus = false;
            }
            else if(gameStart)
            {
                float gap = (float)DateTime.Now.Subtract(lastPausedTime).TotalSeconds;
                currentTime += gap;
                if (currentTime >= time)
                {
                    fill.fillAmount = 1f;
                    clockHand.rotation = Quaternion.identity;
                    OnClick_Answer(-1);
                }
                else
                {
                    clockHand.Rotate(Vector3.forward, gap / time * -360);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (gameStart)
        {
            SaveGame();
        }
    }

    private void PlaceQuestionsAndAnswers()
    {
        questionCounter--;
        sb.Append("Question : ")
            .Append(numberOfQuestions - questionCounter);
        questionNum.text = sb.ToString();
        sb.Clear();
        currentTime = 0;
        timerUpdate = StartCoroutine(TimerUpdate());
        currentShuffledArray = ShuffleCorrectAnswer();
        questionText.text = currentQuestions[currentQuestionIndex].question;
        int tempIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            if(i == currentCorrectAnswerIndex)
            {
                answersText[i].text = currentQuestions[currentQuestionIndex].correctAnswer;
            }
            else
            {
                answersText[i].text = currentQuestions[currentQuestionIndex].wrongAnswers[tempIndex];
                tempIndex++;
            }
            answers[i].GetComponent<Button>().interactable = true;
            answers[i].GetComponent<Image>().sprite = CommonData.Instance.answersSprites[0];
            answers[i].GetComponent<Image>().color = Color.white;
        }
        currentQuestionIndex++;
        answerSelected = false;
    }

    public void OnClick_Answer(int index)
    {
        if (!answerSelected)
        {
            if (index == -1)
            {
                StartCoroutine(TimeIsUp());
            }
            else
            {
                StartCoroutine(AnswerSelected(index));
            }
            answerSelected = true;
        }
    }

    private IEnumerator TimeIsUp()
    {
        for (int i = 0; i < 4; i++)
        {
            answers[i].GetComponent<Button>().interactable = false;
        }
        if(timerUpdate != null) StopCoroutine(timerUpdate);
        yield return new WaitForSeconds(1f);
        answers[currentCorrectAnswerIndex].GetComponent<Image>().sprite = CommonData.Instance.answersSprites[1];//.color = Color.green;
        currentGameMyAnswers[currentQuestionIndex - 1] = -1;

        answersInfo.text = trueAnswersCounter.ToString();

        QuestionEnd();
    }

    private IEnumerator AnswerSelected(int index)
    {
        for (int i = 0; i < 4; i++)
        {
            answers[i].GetComponent<Button>().interactable = false;
        }
        answers[index].GetComponent<Image>().color = Color.grey;
        StopCoroutine(timerUpdate);
        yield return new WaitForSeconds(1f);
        if (index == currentCorrectAnswerIndex) // True Answer
        {
            answers[index].GetComponent<Image>().sprite = CommonData.Instance.answersSprites[1];
            trueAnswersCounter++;
        }
        else  // Wrong Answer
        {
            answers[index].GetComponent<Image>().sprite = CommonData.Instance.answersSprites[2];//.color = Color.red;
            answers[currentCorrectAnswerIndex].GetComponent<Image>().sprite = CommonData.Instance.answersSprites[1];//.color = Color.green;
        }
        currentGameMyAnswers[currentQuestionIndex - 1] = currentShuffledArray[index];

        answersInfo.text = trueAnswersCounter.ToString();

        QuestionEnd();
    }

    private void QuestionEnd()
    {
        if (questionCounter == 0 && !CommonData.Instance.quizEnd)
        {
            gameStart = false;
            CommonData.Instance.quizEnd = true;
            SaveGame();
        }
        continueButton.SetActive(true);
    }

    private void SaveGame()
    {
        if (newGame)
        {
            addedWaitingGame.myScore = trueAnswersCounter;
            addedWaitingGame.questionIndexes = Array.ConvertAll(currentQuestions, question => question.id);
            LocalUser.Instance.waitingGames.Add(addedWaitingGame);
            
            if (friendGame)
            {
                addedWaitingGame.questionAnswers = new int[10];
                Array.Copy(currentGameMyAnswers, addedWaitingGame.questionAnswers, currentGameMyAnswers.Length);

                FBManager.Instance.SaveGameDatasToFriendAndMe(addedWaitingGame);
            }
            else
            {

                addedCommonGame.questionIndexes = Array.ConvertAll(currentQuestions, question => question.id);
                addedCommonGame.questionAnswers = new int[10];
                addedCommonGame.hostScore = trueAnswersCounter;
                Array.Copy(currentGameMyAnswers, addedCommonGame.questionAnswers, currentGameMyAnswers.Length);

                FBManager.Instance.CreateGame(addedCommonGame);
            }
            LocalUser.Instance.numberOfAllGames++;
            LocalUser.Instance.numberOfCorrectAnswers += trueAnswersCounter;

            FBManager.Instance.SaveUserPrivateDatas();
        }
        else
        {
            if (friendGame)
            {
                addedWaitingGame.myScore = trueAnswersCounter;

                FBManager.Instance.SaveToMyLastMatchesAndFriendsWaitingGames(addedWaitingGame);
            }
            else
            {
                addedFinishedGame.myScore = trueAnswersCounter;
                LocalUser.Instance.finishedGames.Add(addedFinishedGame);
                if (LocalUser.Instance.finishedGames.Count > 5) LocalUser.Instance.finishedGames.RemoveAt(0);

                addedWaitingGame.myScore = trueAnswersCounter;
                addedWaitingGame.questionIndexes = Array.ConvertAll(currentQuestions, question => question.id);

                FBManager.Instance.SaveFinishedGame(addedWaitingGame);
            }

            LocalUser.Instance.numberOfAllGames++;
            LocalUser.Instance.numberOfCorrectAnswers += trueAnswersCounter;

            FBManager.Instance.SaveUserPrivateDatas();
        }
    }

    public void OnClick_Continue()
    {
        if(questionCounter == 0)
        {
            if (!newGame)
            {
                if (friendGame)
                {
                    CommonData.Instance.endGameLogic.FinishedJoinedGame(trueAnswersCounter, addedWaitingGame.oppScore , addedWaitingGame.oppName);
                    friendGame = false;
                }
                else
                {
                    CommonData.Instance.endGameLogic.FinishedJoinedGame(trueAnswersCounter, addedWaitingGame.oppScore , addedWaitingGame.oppName);
                }
            }
            else
            {
                MenuLogic.Instance.WriteDataToUI();
                CommonData.Instance.gameCanvas.SetActive(false);
                CommonData.Instance.menuCanvas.SetActive(true);
            }
            answersInfo.text = 0.ToString();
            CommonData.Instance.quizEnd = false;
        }
        else
        {
            PlaceQuestionsAndAnswers();
        }
        continueButton.SetActive(false);
    }

    private int[] ShuffleCorrectAnswer() // Dont use this method another place
    {
        int[] result = { 0, 1, 2, 3 };
        int counter = 0;
        int rand;
        int temp;
        bool check = false;
        for (int i = 0; i < 4; i++)
        {
            rand = random.Next(4 - counter);
            if (rand == 0 && !check)
            {
                check = true;
                currentCorrectAnswerIndex = 3 - counter;
            }
            temp = result[rand];
            result[rand] = result[3 - counter];
            result[3 - counter] = temp;
            counter++;
        }
        return result;
    }
    
    private void ChangeCanvasToGame()
    {
        CommonData.Instance.gameCanvas.SetActive(true);
        CommonData.Instance.categoryCanvas.SetActive(false);
        CommonData.Instance.menuCanvas.SetActive(false);
    }
}
