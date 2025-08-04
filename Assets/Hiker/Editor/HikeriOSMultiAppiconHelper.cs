using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public class HikeriOSMultiAppiconHelper
{
#if UNITY_IOS

    const string iconsFolder = "iOSAppIcons";

    [PostProcessBuild(999)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string imagesPath = pathToXcode + "/Unity-iPhone/Images.xcassets";
            string projectFolder = Directory.GetParent(Application.dataPath).ToString();
            string sourceFolder = Path.Combine(projectFolder, iconsFolder);
            CopyFilesRecursively(sourceFolder, imagesPath);


            //EmbedFrameworks
            string projPath = PBXProject.GetPBXProjectPath(pathToXcode);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string targetGuid = proj.GetUnityMainTargetGuid();

            ToogleBuildSettings(proj, targetGuid, "ASSETCATALOG_COMPILER_INCLUDE_ALL_APPICON_ASSETS", true);

            File.WriteAllText(projPath, proj.WriteToString());
        }
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    private static void ToogleBuildSettings(PBXProject proj, string targetGuid, string key, bool onOrOff)
    {
        proj.SetBuildProperty(targetGuid, key, onOrOff ? "YES" : "NO");
    }
#endif
}
