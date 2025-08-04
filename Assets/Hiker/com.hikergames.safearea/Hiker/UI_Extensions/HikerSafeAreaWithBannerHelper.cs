using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HikerSafeAreaWithBannerHelper
{
    public static bool hasBanner = false;
    public static readonly Vector2 BannerSizeInDP = new Vector2(320, 50);
    public static readonly Vector2 BannerSizeInDP_Tablet = new Vector2(728, 90);
    public const float FallbackScreenDPI = 320f;

    public static Vector2 GetBannerSizeInDP()
    {
#if APPLOVIN_MAX
        if (MaxSdkUtils.IsTablet())
        {
            return BannerSizeInDP_Tablet;
        }
#endif

        return BannerSizeInDP;
    }

    public static float DPToPixel()
    {
        var currentDpi = Screen.dpi;
        if (currentDpi == 0)
        {
            currentDpi = FallbackScreenDPI;
        }
        return currentDpi / 160f;
        //return Mathf.Min(Screen.width / 320f, Screen.height / 320f);
    }

    public static Rect GetSafeAreaWithBanner()
    {
        var safeArea = Screen.safeArea;
        if (hasBanner)
        {
            safeArea.yMin += GetBannerHeightInPixel();
        }
        return safeArea;
    }

    public static Vector2 GetBannerSizeInPixel()
    {
        var s = GetBannerSizeInDP();

        var re = new Vector2(Mathf.Round(s.x * DPToPixel()), Mathf.Round(s.y * DPToPixel()));

        if (re.x < Screen.width)
        {
            re.x = Screen.width;
            re.y = s.y * Screen.width / s.x;
        }

        return re;

        //return new Vector2(1080, Mathf.FloorToInt(s.y * 1080 / s.x));
        //return new Vector2(Screen.width, 168);
    }

    static float GetBannerHeightInPixel()
    {
        var s = GetBannerSizeInPixel();
        return s.y;

        //return Mathf.FloorToInt(s.y * 1080 / s.x);
        //return 168;
    }
}
