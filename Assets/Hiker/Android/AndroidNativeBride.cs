using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
public class AndroidNativeBride
{
    private static AndroidJavaObject currentActivity
    {
        get
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }
    }

    public static string GetPackageName()
    {
        return currentActivity.Call<string>("getPackageName");
    }

    public static string GetAppInfo<T>(string packageName, string info)
    {
        try
        {
            AndroidJavaObject context = currentActivity;
            AndroidJavaObject packageMngr = context.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);

            if (packageInfo == null)
            {
                return string.Empty;
            }
            else
            {
                string appInfo = packageInfo.Get<T>(info).ToString();
                return appInfo;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            return string.Empty;
        }
    }

    public static string GetPackageVersionCode(string packageName)
    {
        return GetAppInfo<int>(packageName, "versionCode");
    }

    public static string GetPackageVersionName(string packageName)
    {
        return GetAppInfo<string>(packageName, "versionName");
    }
}

#endif
