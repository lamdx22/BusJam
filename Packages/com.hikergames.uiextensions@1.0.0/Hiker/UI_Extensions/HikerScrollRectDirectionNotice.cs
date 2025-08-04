using Hiker.UI;
using UnityEngine;
using UnityEngine.UI;

public class HikerScrollRectDirectionNotice : MonoBehaviour
{
    HikerScrollRect scrollRect;

    public GameObject upNotice;
    public GameObject downNotice;

    public GameObject leftNotice;
    public GameObject rightNotice;

    [Range(0f, 1f)]
    public float horizontalThresold = 0.1f;
    [Range(0f, 1f)]
    public float verticalThresold = 0.1f;

    private void OnEnable()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<HikerScrollRect>();
        }
        scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
    }
    private void OnDisable()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<HikerScrollRect>();
        }
        scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
    }

    void OnScrollRectValueChanged(Vector2 newPos)
    {
        if (leftNotice) leftNotice.SetActive(scrollRect.hNeedScrollContent && scrollRect.horizontalNormalizedPosition > horizontalThresold);
        if (rightNotice) rightNotice.SetActive(scrollRect.hNeedScrollContent && scrollRect.horizontalNormalizedPosition < 1f - horizontalThresold);
        if (downNotice) downNotice.SetActive(scrollRect.vNeedScrollContent && scrollRect.verticalNormalizedPosition > verticalThresold);
        if (upNotice) upNotice.SetActive(scrollRect.vNeedScrollContent && scrollRect.verticalNormalizedPosition < 1f - verticalThresold);

    }
}
