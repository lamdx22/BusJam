using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hiker.GUIManager.Editor
{
    using UnityEditor;
    public static class Hiker_EditorUtility
    {
        /// <summary>
        /// Returns the fully qualified path of the package.
        /// </summary>
        public static string packageFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_PackageFullPath))
                    m_PackageFullPath = GetPackageFullPath();

                return m_PackageFullPath;
            }
        }
        [SerializeField]
        private static string m_PackageFullPath;



        private static string GetPackageFullPath()
        {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.hikergames.guimanager");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.hikergames.guimanager/Hiker"))
                {
                    return packagePath + "/Assets/Packages/com.hikergames.guimanager";
                }

                //// Search for default location of normal proj copy files
                //if (Directory.Exists(packagePath + "/Assets/Hiker/GUIManager"))
                //{
                //    return packagePath + "/Assets/Hiker/GUIManager";
                //}

                //// Search for potential alternative locations in the user project
                //string[] matchingPaths = Directory.GetDirectories(packagePath, "TextMesh Pro", SearchOption.AllDirectories);
                //string path = ValidateLocation(matchingPaths, packagePath);
                //if (path != null) return packagePath + path;
            }

            return null;
        }
    }
    public class GUIManager_PackageUtilities : Editor
    {
        /// <summary>
        ///
        /// </summary>
        [MenuItem("Window/Hiker - GUIManager/Import Base Game", false, 2050)]
        public static void ImportProjectResourcesMenu()
        {
            ImportBaseGameAssets();
        }

        /// <summary>
        ///
        /// </summary>
        private static void ImportBaseGameAssets()
        {
            //// Check if the TMP Settings asset is already present in the project.
            //string[] settings = AssetDatabase.FindAssets("t:TMP_Settings");

            //if (settings.Length > 0)
            //{
            //    // Save assets just in case the TMP Setting were modified before import.
            //    AssetDatabase.SaveAssets();

            //    // Copy existing TMP Settings asset to a byte[]
            //    k_SettingsFilePath = AssetDatabase.GUIDToAssetPath(settings[0]);
            //    k_SettingsBackup = File.ReadAllBytes(k_SettingsFilePath);

            //    RegisterResourceImportCallback();
            //}

            string packageFullPath = Hiker_EditorUtility.packageFullPath;

            AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/HikerBaseGame.unitypackage", true);
        }
    }
}
