using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WGPrefabInfo : MonoBehaviour
{
    public TMP_Text category;
    public TMP_Text infoText;
    public Image oppAvatar;
    public GameObject getResultText;
    [HideInInspector] public int index;

    public void OnClick_FinishedWaitingGame()
    {
        AudioManager.Instance.Play(0);
        if (LocalUser.Instance.waitingGames[index].gameWithFriend)
        {
            if (LocalUser.Instance.waitingGames[index].amIHost)
            {
                CommonData.Instance.endGameLogic.FinishWaitingGame(LocalUser.Instance.waitingGames[index].myScore, LocalUser.Instance.waitingGames[index].oppScore, LocalUser.Instance.waitingGames[index].oppName, index);
            }
            else
            {
                CommonData.Instance.gameLogic.JoinGame(true, index);
            }
        }
        else
        {
            CommonData.Instance.endGameLogic.FinishWaitingGame(LocalUser.Instance.waitingGames[index].myScore, LocalUser.Instance.waitingGames[index].oppScore, LocalUser.Instance.waitingGames[index].oppName, index);
        }
    }
}
