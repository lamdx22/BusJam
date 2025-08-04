using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using System.Diagnostics.SymbolStore;

[RequireComponent(typeof(Canvas))]
[ExecuteInEditMode]
public class CanvasHelper : MonoBehaviour
{
    /// <summary>
    /// x is left |
    /// y is bot |
    /// z is right |
    /// w is top
    /// </summary>
    [Tooltip("(left, bot, right, top)")]
    public Vector4 offsetAnchor = Vector4.zero;

    //public static UnityEvent onOrientationChange = new UnityEvent();
    //public static UnityEvent onResolutionChange = new UnityEvent();
    public static bool isLandscape { get; private set; }

    private static List<CanvasHelper> helpers = new List<CanvasHelper>();

    private static bool screenChangeVarsInitialized = false;
    private static ScreenOrientation lastOrientation = ScreenOrientation.Portrait;
    private static Vector2 lastResolution = Vector2.zero;
    private static Rect lastSafeArea = Rect.zero;

    private Canvas canvas;
    private RectTransform rectTransform;

    private List<RectTransform> safeAreaTransform = new List<RectTransform>();

    public void AddSafeAreaHelper(RectTransform helper)
    {
        safeAreaTransform.Add(helper);
        
    }

    public void RemoveSafeAreaHelper(RectTransform helper)
    {
        safeAreaTransform.Remove(helper);
    }

    void OnEnable()
    {
        if (!helpers.Contains(this))
            helpers.Add(this);

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        rectTransform = GetComponent<RectTransform>();

        var safeAreas = transform.GetComponentsInChildren<SafeAreaHelper>();
        foreach (var s in safeAreas)
        {
            if (s)
            {
                var rectT = s.GetComponent<RectTransform>();
                if (rectT)
                {
                    AddSafeAreaHelper(rectT);
                }
            }
        }

        if (!screenChangeVarsInitialized)
        {
            lastOrientation = Screen.orientation;
            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;
            lastSafeArea = HikerSafeAreaWithBannerHelper.GetSafeAreaWithBanner();

            screenChangeVarsInitialized = true;
        }

        ApplySafeArea();
    }

    void Start()
    {
        ApplySafeArea();
    }

    void Update()
    {
        if (helpers == null || helpers.Count == 0 || helpers[0] != this)
            return;

        //if (Application.isMobilePlatform)
        {
            if (Screen.orientation != lastOrientation)
                OrientationChanged();

            var safeArea = HikerSafeAreaWithBannerHelper.GetSafeAreaWithBanner();
            if (safeArea != lastSafeArea)
                SafeAreaChanged();
        }
        //else
        {
            //resolution of mobile devices should stay the same always, right?
            // so this check should only happen everywhere else
            if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
                ResolutionChanged();
        }
    }

    static bool ValidPos(Vector2 p)
    {
        return float.IsNaN(p.x) == false && float.IsNaN(p.y) == false;
    }

    public void ApplySafeArea(RectTransform child, bool applyBannerOffset)
    {
        var safeArea = applyBannerOffset ? HikerSafeAreaWithBannerHelper.GetSafeAreaWithBanner() : Screen.safeArea;

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        var anchorMin = safeArea.min + new Vector2(offsetAnchor.x, offsetAnchor.y);
        var anchorMax = safeArea.max - new Vector2(offsetAnchor.z, offsetAnchor.w);

        var width = canvas.pixelRect.width;
        var height = canvas.pixelRect.height;

        if (width > Mathf.Epsilon && height > Mathf.Epsilon && ValidPos(anchorMin) && ValidPos(anchorMax))
        {
            anchorMin.x /= width;
            anchorMin.y /= height;
            anchorMax.x /= width;
            anchorMax.y /= height;

            child.anchorMin = anchorMin;
            child.anchorMax = anchorMax;
        }

//#if DEBUG
//        Debug.Log(
//        "ApplySafeArea:" +
//        "\n Screen.orientation: " + Screen.orientation +
//#if UNITY_IOS
//         "\n Device.generation: " + UnityEngine.iOS.Device.generation.ToString() +
//#endif
//        "\n Screen.safeArea.position: " + safeArea.position.ToString() +
//        "\n Screen.safeArea.size: " + safeArea.size.ToString() +
//        "\n Screen.width / height: (" + Screen.width.ToString() + ", " + Screen.height.ToString() + ")" +
//        "\n canvas.pixelRect.size: " + canvas.pixelRect.size.ToString() +
//        "\n anchorMin: " + anchorMin.ToString() +
//        "\n anchorMax: " + anchorMax.ToString());
//#endif
    }

    public void ApplySafeArea()
    {
        if (safeAreaTransform == null)
            return;

        //var canvasSize = GetCanvasSize();

        //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasSize.x);
        //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasSize.y);

        for (int i = safeAreaTransform.Count - 1; i >= 0; --i)
        {
            var safeAreaT = safeAreaTransform[i];
            if (safeAreaT)
            {
                var sH = safeAreaT.GetComponent<SafeAreaHelper>();
                ApplySafeArea(safeAreaT, (sH != null && sH.ignoreBannerOffset) ? false : true);
            }
        }
    }

    void OnDisable()
    {
        if (helpers != null)
        {
            helpers.Remove(this);
        }
    }

    private void OrientationChanged()
    {
        //Debug.Log("Orientation changed from " + lastOrientation + " to " + Screen.orientation + " at " + Time.time);

        lastOrientation = Screen.orientation;
        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        isLandscape = lastOrientation == ScreenOrientation.LandscapeLeft || lastOrientation == ScreenOrientation.LandscapeRight || lastOrientation == ScreenOrientation.LandscapeLeft;
        //onOrientationChange.Invoke();

        //for (int i = 0; i < helpers.Count; i++)
        //{
        //    helpers[i].ApplySafeArea();
        //}
    }

    private void ResolutionChanged()
    {
        if (lastResolution.x == Screen.width && lastResolution.y == Screen.height)
            return;

        //Debug.Log("Resolution changed from " + lastResolution + " to (" + Screen.width + ", " + Screen.height + ") at " + Time.time);

        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        isLandscape = Screen.width > Screen.height;
        //onResolutionChange.Invoke();

        //for (int i = 0; i < helpers.Count; i++)
        //{
        //    helpers[i].ApplySafeArea();
        //}
    }

    private void SafeAreaChanged()
    {
        //if (lastSafeArea == Screen.safeArea)
        //    return;

        //Debug.Log("Safe Area changed from " + lastSafeArea + " to " + Screen.safeArea.size + " at " + Time.time);

        lastSafeArea = HikerSafeAreaWithBannerHelper.GetSafeAreaWithBanner();

        for (int i = 0; i < helpers.Count; i++)
        {
            helpers[i].ApplySafeArea();
        }
    }

    public Vector2 GetCanvasSize()
    {
        return rectTransform.sizeDelta;
    }

    public static Vector2 GetSafeAreaPosition()
    {
        return  HikerSafeAreaWithBannerHelper.GetSafeAreaWithBanner().center - new Vector2(Screen.width, Screen.height) * 0.5f;
    }

    public static Vector2 GetSafeAreaSize()
    {
        return HikerSafeAreaWithBannerHelper.GetSafeAreaWithBanner().size;
    }
}