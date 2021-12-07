using UnityEngine;

public class LoadinCircleScaler : MonoBehaviour
{
    [SerializeField] int index;
    RectTransform rect;
    private const float min = .45f;
    private readonly Vector3 maxScale = new Vector3(1f, 1f, 0);

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        if (rect.localScale.x <= min)
        {
            rect.localScale = maxScale;
            ChangeColors();
        }
        else rect.localScale -= new Vector3(.02f, .02f, 0);
    }
    
    private void ChangeColors()
    {
        int temp = index;
        for (int i = 0; i < 8; i++)
        {
            CommonData.Instance.circles[temp].color = CommonData.Instance.colors[i];
            if (temp == 7) temp = 0;
            else temp++;
        }
    }
}
