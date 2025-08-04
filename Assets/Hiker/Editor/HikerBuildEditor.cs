using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;
using DG.Tweening.Plugins;

#if ADDRESSABLE_API
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
#endif
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public static class HikerBuildEditor
{
#if UNITY_IOS
    #region iOS Apple Config
    const string AppleDeveloperTeamID = "68ZNNV3262"; // HIKER Team
    const string DevelopProvision = "543ab77a-6680-46f8-971f-0cc1af1448ad"; // Appstore Provision
    #endregion
#endif
    static string GetProjPath()
    {
        string projPath = Application.dataPath;
        projPath = projPath.Substring(0, projPath.Length - 7);
        return projPath;
    }

#if ADDRESSABLE_API
    public static string build_script
            = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
    public static string settings_asset
        = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
    public static string profile_name = "Default";
    private static AddressableAssetSettings settings;

    static void getSettingsObject(string settingsAsset)
    {
        // This step is optional, you can also use the default settings:
        //settings = AddressableAssetSettingsDefaultObject.Settings;

        settings
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
                as AddressableAssetSettings;

        if (settings == null)
            Debug.LogError($"{settingsAsset} couldn't be found or isn't " +
                           $"a settings object.");
    }

    static void setProfile(string profile)
    {
        string profileId = settings.profileSettings.GetProfileId(profile);
        if (string.IsNullOrEmpty(profileId))
            Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                             $"using current profile instead.");
        else
            settings.activeProfileId = profileId;
    }

    static void setBuilder(IDataBuilder builder)
    {
        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

        if (index > 0)
            settings.ActivePlayerDataBuilderIndex = index;
        else
            Debug.LogWarning($"{builder} must be added to the " +
                             $"DataBuilders list before it can be made " +
                             $"active. Using last run builder instead.");
    }

    static bool buildAddressableContent()
    {
        AddressableAssetSettings
            .BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success)
        {
            Debug.LogError("Addressables build error encountered: " + result.Error);
        }
        return success;
    }

    //[MenuItem("Window/Asset Management/Addressables/Build Addressables only")]
    public static bool BuildAddressables()
    {
        getSettingsObject(settings_asset);
        setProfile(profile_name);
        IDataBuilder builderScript
          = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;

        if (builderScript == null)
        {
            Debug.LogError(build_script + " couldn't be found or isn't a build script.");
            return false;
        }

        setBuilder(builderScript);

        return buildAddressableContent();
    }

    //[MenuItem("Window/Asset Management/Addressables/Build Addressables and Player")]
    //public static void BuildAddressablesAndPlayer()
    //{
    //    bool contentBuildSucceeded = BuildAddressables();

    //    if (contentBuildSucceeded)
    //    {
    //        Build
    //    }
    //}
#endif

#if UNITY_ANDROID
    #region BuildAndroid

    const string defaultApkName = "skyjam";

    [MenuItem("Hiker/Build/Build Android Apk")]
    static public void BuildPlayerApk()
    {
        string projPath = GetProjPath();
        string sr = EditorUtility.SaveFilePanel(
            "Build Android Apk",
            projPath,
            defaultApkName,
            "apk");

        EditorUserBuildSettings.buildAppBundle = false;
        //OPS.Obfuscator.BuildPostProcessor.ManualRestore();
        BuildAndroid(projPath, sr,
            BuildOptions.StrictMode);
        //OPS.Obfuscator.BuildPostProcessor.ManualRestore();
    }

    //[MenuItem("Hiker/Build/Build Android App Bundle")]
    //static public void BuildPlayerAab()
    //{
    //    string projPath = GetProjPath();
    //    string sr = EditorUtility.SaveFilePanel(
    //        "Build Android Apk",
    //        projPath,
    //        defaultApkName,
    //        "aab");

    //    EditorUserBuildSettings.buildAppBundle = true;

    //    BuildAndroid(projPath, sr,
    //        BuildOptions.StrictMode,
    //        true);
    //}

    //[MenuItem("Hiker/Build/Build Debug Android Apk")]
    //static public void BuildDebugPlayerApk()
    //{
    //    string projPath = GetProjPath();
    //    string sr = EditorUtility.SaveFilePanel(
    //        "Build Android Apk",
    //        projPath,
    //        defaultApkName + "_dev",
    //        "apk");

    //    BuildAndroid(projPath, sr,
    //        BuildOptions.StrictMode | BuildOptions.AllowDebugging | BuildOptions.Development,
    //        false);
    //}

    static UnityEditor.Build.Reporting.BuildReport BuildAndroid(string projPath,
        string builtPath,
        BuildOptions op,
        params string[] customSymbolScript)
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        PlayerSettings.Android.keystoreName = projPath + "/rshiker.keystore";
        PlayerSettings.Android.keystorePass = "hk7554";
        PlayerSettings.Android.keyaliasName = "skyjam";
        PlayerSettings.Android.keyaliasPass = "hk7554";

        PlayerSettings.bundleVersion = HikerGameVersion.GetGameVersionString().Substring(1);

        string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenePaths.Length; ++i)
        {
            scenePaths[i] = EditorBuildSettings.scenes[i].path;
        }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            locationPathName = builtPath,
            scenes = scenePaths,
            extraScriptingDefines = customSymbolScript,
            options = op,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
        });

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            // build success
            Debug.LogFormat("Build apk success {0}", report.GetFiles()[0].path);
        }
        else
        {
            Debug.LogFormat("Build apk failed result = {0}", report.summary.result.ToString());
        }

        return report;
    }


    //[MenuItem("Window/Asset Management/Addressables/Build Addressables and Player")]
    static public void JenkinBuildAddresableAndApk()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        /*bool contentBuildSucceeded = BuildAddressables();
        if (contentBuildSucceeded)
        {
            JenkinBuildApk();
        }*/
        JenkinBuildApk();
    }

    static public void JenkinBuildApk()
    {
        string projPath = GetProjPath();

        var oldApk = System.IO.Directory.GetFiles(projPath, "*.apk");
        if (oldApk != null && oldApk.Length > 0)
        {
            foreach (var apk in oldApk)
            {
                System.IO.File.Delete(apk);
            }
        }

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        int bundleVersionCode = 0;

        if (int.TryParse(jenkinBuildNumber, out bundleVersionCode))
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }

        //EditorUserBuildSettings.development = true;
        //EditorUserBuildSettings.allowDebugging = true;
        //EditorUserBuildSettings.androidDebugMinification = AndroidMinification.Gradle;
        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
        EditorUserBuildSettings.buildAppBundle = false;

        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

        PlayerSettings.Android.buildApkPerCpuArchitecture = false;
        PlayerSettings.Android.useAPKExpansionFiles = false;
        PlayerSettings.Android.minifyRelease = true;
        PlayerSettings.Android.androidIsGame = true;

        string sr = string.Format("{0}/Build/{1}_b{2}.apk", projPath, defaultApkName, PlayerSettings.Android.bundleVersionCode);

        Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("build player " + sr);

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        var report = BuildAndroid(projPath, sr,
            BuildOptions.StrictMode | BuildOptions.CompressWithLz4HC);
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            //Debug.Log("Build failed sumary: " + report.summary.result.ToString());
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
        }
    }

    static public void JenkinBuildApkScriptOnly()
    {
        string projPath = GetProjPath();

        var oldApk = System.IO.Directory.GetFiles(projPath, "*.apk");
        if (oldApk != null && oldApk.Length > 0)
        {
            foreach (var apk in oldApk)
            {
                System.IO.File.Delete(apk);
            }
        }

        //var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        //int bundleVersionCode = 0;

        //if (int.TryParse(jenkinBuildNumber, out bundleVersionCode))
        //{
        //    PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        //}

        //EditorUserBuildSettings.development = true;
        //EditorUserBuildSettings.allowDebugging = true;
        //EditorUserBuildSettings.androidDebugMinification = AndroidMinification.Gradle;

        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;
        EditorUserBuildSettings.buildAppBundle = false;

        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

        PlayerSettings.Android.buildApkPerCpuArchitecture = false;
        PlayerSettings.Android.useAPKExpansionFiles = false;
        PlayerSettings.Android.minifyRelease = true;
        PlayerSettings.Android.androidIsGame = true;

        string sr = string.Format("{0}/Build/{1}_b{2}.apk", projPath, defaultApkName, PlayerSettings.Android.bundleVersionCode);

        //Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("build player " + sr);

        var report = BuildAndroid(projPath, sr,
            BuildOptions.StrictMode | BuildOptions.CompressWithLz4 | BuildOptions.BuildScriptsOnly);
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build failed sumary: " + report.summary.result.ToString());
        }

        //catch (System.Exception e)
        //{
        //    throw new System.Exception(e.Message);
        //}
        //finally
        //{
        //    JenkinRestoreObfuscate();
        //}
    }

    public static void JenkinBuildCheatAPK()
    {
        string projPath = GetProjPath();

        var oldApk = System.IO.Directory.GetFiles(projPath, "*.apk");
        if (oldApk != null && oldApk.Length > 0)
        {
            foreach (var apk in oldApk)
            {
                System.IO.File.Delete(apk);
            }
        }

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        int bundleVersionCode = 0;

        if (int.TryParse(jenkinBuildNumber, out bundleVersionCode))
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }

        //EditorUserBuildSettings.development = true;
        //EditorUserBuildSettings.allowDebugging = true;


        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

        PlayerSettings.Android.buildApkPerCpuArchitecture = false;
        PlayerSettings.Android.useAPKExpansionFiles = false;
        PlayerSettings.Android.minifyRelease = true;
        PlayerSettings.Android.androidIsGame = true;

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out string[] symbolScripts);

        List<string> sy = new List<string>(symbolScripts);
        bool changed = false;
        //if (sy.Remove("BUILD_HORIZONTAL"))
        //{
        //    changed = true;
        //}
        //if (sy.Remove("IRON_SOURCE"))
        //{
        //    changed = true;
        //}
        if (sy.Contains("CHEAT_APK") == false)
        {
            sy.Add("CHEAT_APK");
            changed = true;
        }
        sy.Remove("DEV_ENV");

        //if (changed)
        //{
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, sy.ToArray());
        //}

        string sr = string.Format("{0}/Build/{1}_cheat_b{2}.apk", projPath, defaultApkName, PlayerSettings.Android.bundleVersionCode);

        Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("build player " + sr);

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        var report = BuildAndroid(projPath, sr,
            BuildOptions.StrictMode | BuildOptions.CompressWithLz4HC,
            sy.ToArray());
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            //Debug.Log("Build failed sumary: " + report.summary.result.ToString());
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
        }
    }

    public static void JenkinBuildDebugAPK()
    {
        string projPath = GetProjPath();

        var oldApk = System.IO.Directory.GetFiles(projPath, "*.apk");
        if (oldApk != null && oldApk.Length > 0)
        {
            foreach (var apk in oldApk)
            {
                System.IO.File.Delete(apk);
            }
        }

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        int bundleVersionCode = 0;

        if (int.TryParse(jenkinBuildNumber, out bundleVersionCode))
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }

        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.allowDebugging = true;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
        EditorUserBuildSettings.connectProfiler = true;

        //PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.Android, true);
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Debug);

        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);


        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minifyRelease = true;
        PlayerSettings.Android.minifyDebug = true;

        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.Android.useAPKExpansionFiles = false;
        PlayerSettings.Android.buildApkPerCpuArchitecture = false;

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out string[] symbolScripts);

        List<string> sy = new List<string>(symbolScripts);
        sy.Remove("BUILD_HORIZONTAL");
        if (sy.Contains("CHEAT_APK") == false)
        {
            sy.Add("CHEAT_APK");
        }
        //sy.Remove("DEV_ENV");

        //if (changed)
        //{
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, sy.ToArray());
        //}

        string sr = string.Format("{0}/Build/{1}_debug_b{2}.apk", projPath, defaultApkName, PlayerSettings.Android.bundleVersionCode);

        Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("build player " + sr);

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        var report = BuildAndroid(projPath, sr,
            BuildOptions.StrictMode | BuildOptions.CompressWithLz4 |
            BuildOptions.Development | BuildOptions.ConnectToHost | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging,
            sy.ToArray());
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            //Debug.Log("Build failed sumary: " + report.summary.result.ToString());
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
        }
    }


    static public void JenkinBuildAab()
    {
        string projPath = GetProjPath();

        var oldApk = System.IO.Directory.GetFiles(projPath, "*.aab");
        if (oldApk != null && oldApk.Length > 0)
        {
            foreach (var apk in oldApk)
            {
                System.IO.File.Delete(apk);
            }
        }

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        int bundleVersionCode = 0;

        if (int.TryParse(jenkinBuildNumber, out bundleVersionCode))
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out string[] symbolScripts);
        List<string> sy = new List<string>();
        foreach (var s in symbolScripts)
        {
            if (s != "CHEAT_APK" &&
                s != "BUILD_HORIZONTAL" &&
                s != "SPINE_DEBUG" &&
                s != "DEV_ENV")
            {
                sy.Add(s);
            }
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, sy.ToArray());

        string sr = string.Format("{0}/Build/{1}_b{2}.aab", projPath, defaultApkName, PlayerSettings.Android.bundleVersionCode);

        Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("build player " + sr);

        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.buildAppBundle = true;

        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

        PlayerSettings.SplashScreen.show = false;

        PlayerSettings.Android.buildApkPerCpuArchitecture = true;
        PlayerSettings.Android.useAPKExpansionFiles = true;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.Android.minifyRelease = true;

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        //OPS.Obfuscator.BuildPostProcessor.ManualRestore();

        var report = BuildAndroid(projPath, sr,
            BuildOptions.StrictMode | BuildOptions.CompressWithLz4HC,
            sy.ToArray());
        //OPS.Obfuscator.BuildPostProcessor.ManualRestore();
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
        }
    }


    static public void JenkinExportGradle()
    {
        string projPath = GetProjPath();

        //var oldApk = System.IO.Directory.GetFiles(projPath, "*.aab");
        //if (oldApk != null && oldApk.Length > 0)
        //{
        //    foreach (var apk in oldApk)
        //    {
        //        System.IO.File.Delete(apk);
        //    }
        //}

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        int bundleVersionCode = 0;

        if (int.TryParse(jenkinBuildNumber, out bundleVersionCode))
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out string[] symbolScripts);
        List<string> sy = new List<string>();
        foreach (var s in symbolScripts)
        {
            if (s != "CHEAT_APK" &&
                s != "BUILD_HORIZONTAL" &&
                s != "SPINE_DEBUG" &&
                s != "DEV_ENV")
            {
                sy.Add(s);
            }
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, sy.ToArray());

        string sr = string.Format("{0}/Build/{1}_gradle", projPath, defaultApkName);

        Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("export gradle " + sr);

        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.buildAppBundle = true;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

        PlayerSettings.SplashScreen.show = false;

        PlayerSettings.Android.buildApkPerCpuArchitecture = true;
        PlayerSettings.Android.useAPKExpansionFiles = true;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.Android.minifyRelease = true;

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        //OPS.Obfuscator.BuildPostProcessor.ManualRestore();

        var report = BuildAndroid(projPath, sr,
            BuildOptions.StrictMode | BuildOptions.CompressWithLz4HC,
            sy.ToArray());
        //OPS.Obfuscator.BuildPostProcessor.ManualRestore();
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
        }
    }

    [MenuItem("Hiker/Build/Build Debug Android Apk")]
    static public void BuildDebugPlayerApk()
    {
        string projPath = GetProjPath();
        string sr = EditorUtility.SaveFilePanel(
            "Build Android Apk",
            projPath,
            defaultApkName + "_dev",
            "apk");

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        EditorUserBuildSettings.buildScriptsOnly = true;
        EditorUserBuildSettings.buildAppBundle = false;

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

        PlayerSettings.Android.minifyDebug = true;
        PlayerSettings.Android.buildApkPerCpuArchitecture = false;
        PlayerSettings.Android.useAPKExpansionFiles = false;

        BuildAndroid(projPath, sr,
            BuildOptions.StrictMode | BuildOptions.AllowDebugging | BuildOptions.Development);
    }

    #endregion
#endif

#if UNITY_IOS
    #region IOS
    const string defaultIPAName = "toysfactory";
    static UnityEditor.Build.Reporting.BuildReport BuildiOS(string projPath, string builtPath, BuildOptions op, string[] symbolScripts)
    {
        string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenePaths.Length; ++i)
        {
            scenePaths[i] = EditorBuildSettings.scenes[i].path;
        }

        PlayerSettings.iOS.appleDeveloperTeamID = AppleDeveloperTeamID;
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        PlayerSettings.iOS.iOSManualProvisioningProfileID = DevelopProvision;
        PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;

        PlayerSettings.bundleVersion = GameClient.GetGameVersionString().Substring(1);

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = builtPath,
            target = BuildTarget.iOS,
            options = op,
            extraScriptingDefines = symbolScripts,
            targetGroup = BuildTargetGroup.iOS,
        });

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            if (System.IO.File.Exists(builtPath + "/Unity-iPhone.xcodeproj"))
            {
                // build success
                Debug.LogFormat("Build ios success {0}", report.GetFiles()[0].path);
            }
        }
        else
        {
            Debug.LogFormat("Build ios failed result = {0}", report.summary.result.ToString());
        }

        return report;
    }

    [MenuItem("Hiker/Build/Build iOS")]
    static public void BuildPlayerIOS()
    {
        string projPath = GetProjPath();

        string sr = EditorUtility.SaveFilePanel(
            "Build iOS",
            projPath,
            "cubedefender_xcode",
            "");

        var op =
            BuildOptions.StrictMode | 
            BuildOptions.CompressWithLz4HC |
            BuildOptions.SymlinkSources;

        BuildiOS(projPath, sr, op, null);
    }

    //static public void JenkinBuildAddresableAndAppIOS()
    //{
    //    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
    //    {
    //        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
    //    }
    //    /*
    //    bool contentBuildSucceeded = BuildAddressables();
    //    if (contentBuildSucceeded)
    //    {
    //        JenkinBuildiOS();
    //    }
    //    */
    //}

    //[MenuItem("Hiker/Build/Test Jenkin Build iOS")]
    static public void JenkinBuildiOS()
    {
        string projPath = GetProjPath();

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        string bundleVersion = GameClient.GetGameVersionString().Substring(1);
        PlayerSettings.bundleVersion = bundleVersion;
        PlayerSettings.iOS.buildNumber = // bundleVersion + "." + 
            jenkinBuildNumber;

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        string sr = string.Format("{0}/Build/{1}_xcode", projPath, defaultIPAName);

        Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("build player " + sr);
        
        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, out string[] symbolScripts);
        List<string> sy = new List<string>();
        foreach (var s in symbolScripts)
        {
            if (s != "CHEAT_APK" &&
                s != "BUILD_HORIZONTAL" &&
                s != "SPINE_DEBUG" &&
                s != "DEV_ENV")
            {
                sy.Add(s);
            }
        }

        var op =
            BuildOptions.StrictMode | 
            BuildOptions.CompressWithLz4HC |
            //BuildOptions.AllowDebugging | BuildOptions.Development |
            BuildOptions.SymlinkSources;

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;
        EditorUserBuildSettings.buildScriptsOnly = false;

        PlayerSettings.SplashScreen.show = false;


#if UNITY_2022_3_OR_NEWER
        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.iOS, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSpeed);
#else
        EditorUserBuildSettings.il2CppCodeGeneration = UnityEditor.Build.Il2CppCodeGeneration.OptimizeSpeed;
#endif

        var report = BuildiOS(projPath, sr, op, sy.ToArray());
        
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
            //Debug.Log("Build failed sumary: " + report.summary.result.ToString());
        }
    }

    static public void JenkinBuildiOSScriptOnly()
    {
        string projPath = GetProjPath();

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        string bundleVersion = GameClient.GetGameVersionString().Substring(1);
        PlayerSettings.bundleVersion = bundleVersion;
        PlayerSettings.iOS.buildNumber = bundleVersion + "." + jenkinBuildNumber;

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        string sr = string.Format("{0}/Build/{1}_xcode", projPath, defaultIPAName);

        Debug.Log("Jenkin build number " + jenkinBuildNumber);
        Debug.Log("build player " + sr);

        var op =
            BuildOptions.BuildScriptsOnly |
            BuildOptions.StrictMode | 
            BuildOptions.CompressWithLz4HC |
            BuildOptions.SymlinkSources;

        var splashScene = EditorBuildSettings.scenes[0].path;

        var report = BuildiOS(projPath, sr,  op, null);
        
        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build failed sumary: " + report.summary.result.ToString());
        }
    }

    [PostProcessBuild(999)]
    public static void AddCapabilities(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

            var pbxProject = new PBXProject();
            pbxProject.ReadFromString(File.ReadAllText(projPath));
            string mainTarget = pbxProject.GetUnityMainTargetGuid();

            var entitlementsFileName = pbxProject.GetBuildPropertyForAnyConfig(mainTarget, "CODE_SIGN_ENTITLEMENTS");
            if (entitlementsFileName == null)
            {
                var bundleIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
                entitlementsFileName = string.Format("{0}.entitlements", bundleIdentifier.Substring(bundleIdentifier.LastIndexOf(".") + 1));
            }

            ProjectCapabilityManager capMan =
                new ProjectCapabilityManager(projPath,
                                             entitlementsFileName,
                                             "Unity-iPhone", mainTarget);
            capMan.AddGameCenter();
            capMan.AddInAppPurchase();
            capMan.AddPushNotifications(false);

            capMan.WriteToFile();

            pbxProject.AddCapability(mainTarget, PBXCapabilityType.GameCenter);
            pbxProject.AddCapability(mainTarget, PBXCapabilityType.InAppPurchase);
            pbxProject.AddCapability(mainTarget, PBXCapabilityType.PushNotifications, entitlementsFileName, true);


            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            PlistElementDict ats = null;
            if (rootDict.values.ContainsKey("NSAppTransportSecurity") == false)
            {
                ats = rootDict.CreateDict("NSAppTransportSecurity");
            }
            else
            {
                ats = plist.root.values["NSAppTransportSecurity"].AsDict();
            }

            if (ats != null)
            {
                ats.values.Remove("NSAllowsArbitraryLoadsInWebContent");
                ats.SetBoolean("NSAllowsArbitraryLoads", true);
            }

            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");

            SetAdsNeworks(rootDict, plistPath);

            rootDict.values.Remove("UIApplicationExitsOnSuspend");
            plist.WriteToFile(plistPath);


            //proj.SetTeamId(targetGuid, AppleDeveloperTeamID);
            pbxProject.SetBuildProperty(mainTarget, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks /usr/lib/swift");

            // turnoff bitcode, XCode 14.x Bitcode generation was deprecated
            var unityFrameworkTarget = pbxProject.GetUnityFrameworkTargetGuid();
            TurnOffBitcode(pbxProject, mainTarget);
            TurnOffBitcode(pbxProject, unityFrameworkTarget);

            // turnoff embed switf lib on unity framework to upload to appstore connect
            SetToggleEmbedSwitfLib(pbxProject, unityFrameworkTarget, false);
            
            File.WriteAllText(projPath, pbxProject.WriteToString());

            OnIOSBuild(buildTarget, pathToBuiltProject);
        }
    }

    // set in HikerPostProcessBuild
    //private static void SetATTPromptMessage(PlistElementDict plistRoot)
    //{
    //    // Set the description key-value in the plist:
    //    plistRoot.SetString("NSUserTrackingUsageDescription",
    //        "Cube Defender need to access the IDFA in order to improve personalized advertising of game, log player's behaviour and catch critical bugs.");
    //}

    private static void TurnOffBitcode(PBXProject proj, string targetGuid)
    {
        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
    }

    private static void SetToggleEmbedSwitfLib(PBXProject proj, string targetGuid, bool onOrOff)
    {
        proj.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", onOrOff ? "YES" : "NO");
    }

    static readonly string[] SKAdNetId = new string[]
    {
        "IronSource", // ironSource
        "Admob", // admob
        "AppLovin", // applovin
        "Facebook", // FAN
        "UnityAds", // UnityAds
        "AdColony", // AdConlony
        "InMobi", // InMobi
        "Fyber",
        "Mintegral",
        "Liftoff",
        "Pangle",
        "Smaato",
        "Yandex",
        "BidMachine"
    };
    private static void SetAdsNeworks(PlistElementDict rootDict, string plistPath)
    {
        // Check if SKAdNetworkItems already exists
        PlistElementArray SKAdNetworkItems = null;
        if (rootDict.values.ContainsKey("SKAdNetworkItems"))
        {
            try
            {
                SKAdNetworkItems = rootDict.values["SKAdNetworkItems"] as PlistElementArray;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(string.Format("Could not obtain SKAdNetworkItems PlistElementArray: {0}", e.Message));
            }
        }

        Debug.Log("Adding SKAdNeworkIDs");

        // If not exists, create it
        if (SKAdNetworkItems == null)
        {
            SKAdNetworkItems = rootDict.CreateArray("SKAdNetworkItems");
        }

        string plistContent = File.ReadAllText(plistPath);

        HashSet<string> listIds = new HashSet<string>();

        foreach (var network in SKAdNetId)
        {
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Hiker/Editor/SKAdNetworkID/" + network + ".txt");

            if (textAsset == null) continue;

            Debug.Log("Adding " + network);
            StringReader sReader = new StringReader(textAsset.text);
            var s = string.Empty;
            do
            {
                try
                {
                    s = sReader.ReadLine();
                    Debug.Log("Line " + s);
                    if (string.IsNullOrEmpty(s) == false &&
                        plistContent.Contains(s) == false &&
                        listIds.Contains(s) == false)
                    {
                        Debug.Log("Adding " + network + " id: " + s);
                        PlistElementDict SKAdNetworkIdentifierDict = SKAdNetworkItems.AddDict();
                        SKAdNetworkIdentifierDict.SetString("SKAdNetworkIdentifier", s);
                        listIds.Add(s);
                    }
                }
                catch
                {
                    s = null;
                }
            } while (s != null);

            Debug.Log("Finished " + network);
        }
    }

    private static void OnIOSBuild(BuildTarget target, string path)
    {
        //NativeLocale.AddLocalizedStringsIOS(path, Path.Combine(Application.dataPath, "NativeLocale/iOS"));
    }

    #endregion
#endif
    static public void JenkinRestoreObfuscate()
    {
        //Debug.Log("Restore obfuscator");
        //Beebyte.Obfuscator.RestoreUtils.RestoreMonobehaviourSourceFiles();
    }

    [MenuItem("Hiker/Clear Preference")]
    static void ClearPlayerPref()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Hiker/Build/Test Jenkin Build AssetBundles")]
    static void JenkinBuildAssetBundle()
    {
        //JenkinBuildAssetBundlePri(BuildAssetBundleOptions.None);
    }

    static void JenkinReBuildAssetBundle()
    {
        //JenkinBuildAssetBundlePri(BuildAssetBundleOptions.ForceRebuildAssetBundle);
    }

    //static void JenkinBuildAssetBundlePri(BuildAssetBundleOptions option)
    //{
    //    string outputPath = Path.Combine(AssetBundles.Utility.AssetBundlesOutputPath, AssetBundles.Utility.GetPlatformName());
    //    if (!Directory.Exists(outputPath))
    //        Directory.CreateDirectory(outputPath);

    //    //@TODO: use append hash... (Make sure pipeline works correctly with it.)
    //    BuildPipeline.BuildAssetBundles(outputPath, option, EditorUserBuildSettings.activeBuildTarget);

    //    AssetBundles.AssetBundlesMenuItems.BuildAssetBundles();
    //    JenkinRestoreObfuscate();
    //}
#if UNITY_STANDALONE
    static readonly string defaultWindowsAppName = "CubeDefender";

#if UNITY_STANDALONE_WIN

    public static void  JenkinsBuildWin64()
    {
        string projPath = GetProjPath();

        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");

        string sr = string.Format("{0}/Build/{1}_pc_b{2}", projPath, defaultWindowsAppName, jenkinBuildNumber);

        if (!Directory.Exists(sr))
            Directory.CreateDirectory(sr);

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] symbolScripts);
        List<string> sy = new List<string>();
        foreach (var s in symbolScripts)
        {
            if (s != "SPINE_DEBUG")
            {
                sy.Add(s);
            }
        }

        if (sy.Contains("BUILD_HORIZONTAL") == false)
        {
            sy.Add("BUILD_HORIZONTAL");
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, sy.ToArray());


        var op =
            BuildOptions.StrictMode |
            BuildOptions.CompressWithLz4HC;

        string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenePaths.Length; ++i)
        {
            scenePaths[i] = EditorBuildSettings.scenes[i].path;
        }

#if UNITY_2022_3_OR_NEWER
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
#else
        EditorUserBuildSettings.enableHeadlessMode = false;
#endif
        EditorUserBuildSettings.development = false;

        var report = BuildStandalone(BuildTarget.StandaloneWindows64, sr, defaultWindowsAppName + ".exe",
            scenePaths,
            op,
            true, sy.ToArray());

        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            //Debug.Log("Build failed sumary: " + report.summary.result.ToString());
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
        }
    }

    [MenuItem("Hiker/Build/Build Test Dev Win64")]
    public static void BuildDevWin64()
    {
        string projPath = GetProjPath();

        string sr = string.Format("{0}/Build/{1}_pc_dev", projPath, defaultWindowsAppName);

        if (!Directory.Exists(sr))
            Directory.CreateDirectory(sr);

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] symbolScripts);
        List<string> sy = new List<string>();
        foreach (var s in symbolScripts)
        {
            if (s != "SPINE_DEBUG")
            {
                sy.Add(s);
            }
        }

        //if (sy.Contains("BUILD_HORIZONTAL") == false)
        //{
        //    sy.Add("BUILD_HORIZONTAL");
        //}

        if (sy.Contains("TEST_BUILD") == false)
        {
            sy.Add("TEST_BUILD");
        }

        var op =
            BuildOptions.StrictMode |
            BuildOptions.CompressWithLz4HC;

        string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenePaths.Length; ++i)
        {
            scenePaths[i] = EditorBuildSettings.scenes[i].path;
        }

#if UNITY_2022_3_OR_NEWER
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
#else
        EditorUserBuildSettings.enableHeadlessMode = false;
#endif

        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.connectProfiler = true;
        EditorUserBuildSettings.allowDebugging = true;

        var report = BuildStandalone(BuildTarget.StandaloneWindows64, sr, defaultWindowsAppName + ".exe",
            scenePaths,
            op | BuildOptions.Development | BuildOptions.AllowDebugging,
            true, sy.ToArray());

        JenkinRestoreObfuscate();

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            //Debug.Log("Build failed sumary: " + report.summary.result.ToString());
            throw new System.Exception("Build failed sumary: " + report.summary.result.ToString());
        }
    }
#endif

    static UnityEditor.Build.Reporting.BuildReport BuildStandalone(BuildTarget buildTarget,
        string builtPath, string builtName,
        string[] scenePaths, BuildOptions buildOp, bool isIl2Cpp, string[] symbolScripts)
    {
        if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, buildTarget);
        }

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,
            isIl2Cpp ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

        string locationPathName = System.IO.Path.Combine(builtPath, builtName);

        //string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
        //for (int i = 0; i < scenePaths.Length; ++i)
        //{
        //    scenePaths[i] = EditorBuildSettings.scenes[i].path;
        //}

        var report = BuildPipeline.BuildPlayer(
            new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = locationPathName,
                target = buildTarget,
                options = buildOp,
                extraScriptingDefines = symbolScripts,
                targetGroup = BuildTargetGroup.Standalone,
            });

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            // build success
            Debug.LogFormat("Build success {0}", report.GetFiles()[0].path);
        }
        else
        {
            string mess = string.Format("Build failed result = {0}", report.summary.result.ToString());
            Debug.Log(mess);
            //throw new System.Exception(mess);
        }

        return report;
    }
#endif


    static public void RemoveDEVENVParam()
    {
        // duongrs changed: allways build asset bundle config in production release
        List<string> symbolScripts = new List<string>();
        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out string[] defines);
        symbolScripts.AddRange(defines);
        if (symbolScripts.Contains("DEV_ENV"))
        {
            symbolScripts.Remove("DEV_ENV");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbolScripts.ToArray());
        }
    }

    static public void BuildAssetBundleConfig(BuildTarget buildTarget, int configVer)
    {
        string outputPath = Path.Combine("AssetBundles", "Configs/" + buildTarget.ToString());
        outputPath = Path.Combine(GetProjPath(), outputPath);
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        AssetBundleBuild[] configs = new AssetBundleBuild[1];
        configs[0].assetBundleName = "v" + configVer + ".unity3d";

        bool b = AssetDatabase.IsValidFolder("Assets/Resources/Configs");

        if (b)
        {
            List<string> listAssets = new List<string>();
            string[] assetsGUID = AssetDatabase.FindAssets(string.Empty, new string[] {
                "Assets/Hiker/BaseGame/Resources/Configs"
            });
            listAssets.AddRange(assetsGUID);
            string[] assetsGUIDLocl = AssetDatabase.FindAssets(string.Empty, new string[] {
                "Assets/Hiker/BaseGame/Resources/Localizations"
            });
            listAssets.AddRange(assetsGUIDLocl);

            configs[0].assetNames = new string[listAssets.Count + 1];
            for (int i = 0; i < listAssets.Count; ++i)
            {
                var guid = listAssets[i];
                configs[0].assetNames[i] = AssetDatabase.GUIDToAssetPath(guid);
            }
            configs[0].assetNames[listAssets.Count] = "Assets/Hiker/BaseGame/Resources/Localization.csv";
        }

        BuildPipeline.BuildAssetBundles(outputPath, configs, BuildAssetBundleOptions.None, buildTarget);
    }

    static public void JenkinBuildAssetBundleConfig()
    {
        var jenkinBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        var ver = int.Parse(jenkinBuildNumber);
        BuildAssetBundleConfig(EditorUserBuildSettings.activeBuildTarget, ver);
    }

    //[MenuItem("Hiker/Build/Build configs")]
    //static public void BuildAssetBundleConfig()
    //{
    //    string versionConfig = HikerInputEditor.ConfirmInput("Config version");
    //    if (string.IsNullOrEmpty(versionConfig))
    //    {
    //        return;
    //    }

    //    if (int.TryParse(versionConfig, out int ver))
    //    {
    //        BuildAssetBundleConfig(EditorUserBuildSettings.activeBuildTarget, ver);
    //    }
    //    else
    //    {
    //        Debug.LogError("Please input correct num version config");
    //    }
    //}
}
