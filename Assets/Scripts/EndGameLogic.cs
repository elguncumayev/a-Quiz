using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameLogic : MonoBehaviour
{
    [SerializeField] Image myPP;
    [SerializeField] TMP_Text myName;
    [SerializeField] TMP_Text myScore;
    [SerializeField] TMP_Text myResult;
    [SerializeField] TMP_Text myTrophyChange;
    [SerializeField] Image oppPP;
    [SerializeField] TMP_Text oppName;
    [SerializeField] TMP_Text oppScore;
    [SerializeField] TMP_Text oppResult;
    [SerializeField] TMP_Text oppTrophyChange;

    public void FinishWaitingGame(int myScore, int oppTrueAnswers,string oppName, int index)
    {
        CommonData.Instance.menuCanvas.SetActive(false);
        FBManager.Instance.FromWaitingGamesToLastMatches(LocalUser.Instance.waitingGames[index]);
        GameEnd(myScore, oppTrueAnswers,oppName);
        LocalUser.Instance.waitingGames.RemoveAt(index);
        MenuLogic.Instance.WriteDataToUI();
    }

    public void FinishedJoinedGame(int myScore, int oppTrueAnswers, string oppName)
    {
        CommonData.Instance.gameCanvas.SetActive(false);
        GameEnd(myScore, oppTrueAnswers,oppName);
        MenuLogic.Instance.WriteDataToUI();
    }

    public void GameEnd(int score, int oppTrueAnswers , string oppName)
    {
        if (score > oppTrueAnswers)
        {
            AudioManager.Instance.Play(1);
            myTrophyChange.text = string.Format("+ {0}", score);
            oppTrophyChange.text = string.Format("- {0}", 10 - oppTrueAnswers);
            LocalUser.Instance.numberOfWonGames++;
            LocalUser.Instance.trophy += score;
        }
        else if (score < oppTrueAnswers)
        {
            AudioManager.Instance.Play(2);
            myTrophyChange.text = string.Format("- {0}", 10 - score);
            oppTrophyChange.text = string.Format("+ {0}", oppTrueAnswers);
            LocalUser.Instance.trophy -= (10 - score);
            if (LocalUser.Instance.trophy < 0) LocalUser.Instance.trophy = 0;
        }
        else
        {
            AudioManager.Instance.Play(1);
            myTrophyChange.text = string.Format("+ {0}", score);
            oppTrophyChange.text = string.Format("+ {0}", oppTrueAnswers);
            LocalUser.Instance.trophy += score;
        }
        FBManager.Instance.SaveUserPrivateDatas();

        CommonData.Instance.endGameCanvas.SetActive(true);
        myName.text = LocalUser.Instance.nickName;
        myScore.text = score.ToString();
        this.oppName.text = oppName;
        oppScore.text = oppTrueAnswers.ToString();
        if(score > oppTrueAnswers)
        {
            myResult.text = "won";
            myResult.color = Color.green;
            oppResult.text = "lost";
            oppResult.color = Color.red;
            StartCoroutine(MyParticles());
        }
        else if(score < oppTrueAnswers)
        {
            myResult.text = "lost";
            myResult.color = Color.red;
            oppResult.text = "win";
            oppResult.color = Color.green;
            StartCoroutine(OppParticles());
        }
        else
        {
            myResult.text = "draw";
            myResult.color = Color.yellow;
            oppResult.text = "draw";
            oppResult.color = Color.yellow;
            StartCoroutine(MyParticles());
            StartCoroutine(OppParticles());
        }
    }

    IEnumerator MyParticles()
    {
        CommonData.Instance.myPS[0].SetActive(true);
        yield return new WaitForSeconds(.4f);
        CommonData.Instance.myPS[1].SetActive(true);
    }
    IEnumerator OppParticles()
    {
        CommonData.Instance.oppPS[0].SetActive(true);
        yield return new WaitForSeconds(.4f);
        CommonData.Instance.oppPS[1].SetActive(true);
    }

    public void OnClick_Continue()
    {
        CommonData.Instance.menuCanvas.SetActive(true);
        CommonData.Instance.endGameCanvas.SetActive(false);
        CommonData.Instance.myPS[0].SetActive(false);
        CommonData.Instance.myPS[1].SetActive(false);
        CommonData.Instance.oppPS[0].SetActive(false);
        CommonData.Instance.oppPS[1].SetActive(false);
    }
}
