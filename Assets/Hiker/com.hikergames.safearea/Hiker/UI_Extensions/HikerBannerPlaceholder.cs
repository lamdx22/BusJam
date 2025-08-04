using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HikerBannerPlaceholder : MonoBehaviour
{
    public static HikerBannerPlaceholder instance = null;

    [SerializeField]
    Image imgBkg;
    [SerializeField]
    RectTransform bannerRectTran;

    bool isDirt = true;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnEnable()
    {
        HikerSafeAreaWithBannerHelper.hasBanner = true;
        isDirt = true;
        //UpdateSize();
    }

    private void OnDisable()
    {
        HikerSafeAreaWithBannerHelper.hasBanner = false;
        isDirt = true;
        //UpdateSize();
    }

    // Update is called once per frame
    void Update()
    {
//#if UNITY_EDITOR
        UpdateSize();
//#endif
    }

    private void OnTransformParentChanged()
    {
        isDirt = true;
    }

    void UpdateSize()
    {
        if (isDirt == false) return;
        var background = GetImage();
        var safeArea = Screen.safeArea;

        var bannerSize = HikerSafeAreaWithBannerHelper.hasBanner ? HikerSafeAreaWithBannerHelper.GetBannerSizeInPixel() : Vector2.zero;
        var rootCanvas = background.canvas.rootCanvas;
        var canvasSize = rootCanvas.GetComponent<RectTransform>().sizeDelta;

        var rectTran = GetRectTransform();
        var screenSize = new Vector2(Screen.width, Screen.height);
        var anchorPoint = new Vector2(0.5f, 0f);
        rectTran.anchorMin = anchorPoint;
        rectTran.anchorMax = anchorPoint;
        rectTran.pivot = anchorPoint;

        rectTran.anchoredPosition = new Vector3(0f, safeArea.yMin / screenSize.y * canvasSize.y);
        var rateScale = Mathf.Min(canvasSize.x / screenSize.x, canvasSize.y / screenSize.y);
        //var rateScale = 1f / rootCanvas.scaleFactor;
        //var rateScale = 1f;

        rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bannerSize.x * rateScale);
        rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bannerSize.y * rateScale);

        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bannerSize.y * rateScale + rectTran.anchoredPosition.y);
        //Debug.LogFormat("Update Banner Size {0}x{1} - dpi={2}, rateScale={3}, ScreenSize={4}x{5}", bannerSize.x * rateScale, bannerSize.y * rateScale,
        //    Screen.dpi, rateScale, Screen.width, Screen.height);
        isDirt = false;
    }

    RectTransform GetRectTransform()
    {
        if (bannerRectTran == null)
        {
            bannerRectTran = GetComponent<RectTransform>();
        }
        return bannerRectTran;
    }

    Image GetImage()
    {
        if (imgBkg == null)
        {
            imgBkg = GetComponent<Image>();
        }
        return imgBkg;
    }
}
