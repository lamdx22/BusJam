using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_ANDROID
using UnityEditor.Android;

#endif
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public class HikerPostProcessBuild
#if UNITY_ANDROID
    : IPostGenerateGradleAndroidProject
#endif
{
#if UNITY_IOS
    /// <summary>
    /// Description for IDFA request notification 
    /// [sets NSUserTrackingUsageDescription]
    /// </summary>
    const string TrackingDescription =
        "Toys Factory need to access the IDFA in order to improve personalized advertising of game, log player's behaviour and catch critical bugs.";

    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        // already called by Applovin Max framework
        //if (buildTarget == BuildTarget.iOS)
        //{
        //    AddPListValues(pathToXcode);
        //    AddFrameWorks(pathToXcode);
        //}
    }

    static void AddPListValues(string pathToXcode)
    {
        // Get Plist from Xcode project 
        string plistPath = pathToXcode + "/Info.plist";

        // Read in Plist 
        PlistDocument plistObj = new PlistDocument();
        plistObj.ReadFromString(File.ReadAllText(plistPath));

        // set values from the root obj
        PlistElementDict plistRoot = plistObj.root;

        // Set value in plist
        plistRoot.SetString("NSUserTrackingUsageDescription", TrackingDescription);

        // save
        File.WriteAllText(plistPath, plistObj.WriteToString());
    }

    static void AddFrameWorks(string pathToXcode)
    {
        //EmbedFrameworks
        string projPath = PBXProject.GetPBXProjectPath(pathToXcode);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string targetGUID = proj.GetUnityMainTargetGuid();
        proj.AddFrameworkToProject(targetGUID, "AppTrackingTransparency.framework", true);
        proj.WriteToFile(projPath);
        //EmbedFrameworks end
    }
#endif
#if UNITY_ANDROID
    public int callbackOrder => int.MaxValue;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        //Debug.Log("HikerPostProcessBuild.OnPostGenerateGradleAndroidProject at path " + path);
        //var wrapperPath = System.IO.Path.Combine(path, "../gradle/wrapper/gradle-wrapper.properties");

        //File.WriteAllText(wrapperPath, @"distributionUrl=https\://services.gradle.org/distributions/gradle-8.11.1-bin.zip");

        var gradlePropFile = System.IO.Path.Combine(path, "../gradle.properties");
        var ss = File.ReadAllLines(gradlePropFile);

        bool changed = false;
        for (int i = 0; i < ss.Length; i++)
        {
            var line = ss[i];
            if (line.StartsWith("android.enableDexingArtifactTransform"))
            {
                ss[i] = "#" + line;
                changed = true;
            }
        }

        if (changed)
        {
            Debug.Log("Turn off \'android.enableDexingArtifactTransform\'");
            File.WriteAllLines(gradlePropFile, ss);
        }
    }
#endif
}
