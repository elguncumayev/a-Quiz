using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPrefabInfo : MonoBehaviour
{
    public TMP_Text friendName;
    public Image friendAvatar;
    public TMP_Text infoText;
    [HideInInspector] public FriendData friendData;

    [HideInInspector] public bool isRequest;

    public void OnClick_Friend()
    {
        AudioManager.Instance.Play(0);
        if (isRequest)
        {
            CommonData.Instance.frReqTextPopUp.text = string.Format("{0} sent friend request", friendName.text);
            CommonData.Instance.frRequestBack.SetActive(true);
            LeanTween.scale(CommonData.Instance.frRequestPopUp, Vector2.one, .25f).setEaseOutBack();
        }
        else
        {
            CommonData.Instance.friendPopUpAvatar.sprite = friendAvatar.sprite;
            CommonData.Instance.friendPopUpName.text = friendName.text;
            CommonData.Instance.friendPopUpTrophy.text = friendData.friendTrophy.ToString();
            CommonData.Instance.friendBack.SetActive(true);
            LeanTween.scale(CommonData.Instance.friendPopUp, Vector2.one, .25f).setEaseOutBack();
        }
        CommonData.Instance.currentSelectedFriend = friendData;
    }
}