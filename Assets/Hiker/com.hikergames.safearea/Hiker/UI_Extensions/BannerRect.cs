using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerRect : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        var v2 = HikerSafeAreaWithBannerHelper.GetBannerSizeInPixel();
        var rectTran = GetComponent<RectTransform>();

        rectTran.anchorMin = new Vector2(0, Screen.safeArea.yMin / Screen.height);
        rectTran.anchorMax = new Vector2(1, Screen.safeArea.yMin / Screen.height);

        rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, v2.x);
        rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, v2.y);
    }
}
