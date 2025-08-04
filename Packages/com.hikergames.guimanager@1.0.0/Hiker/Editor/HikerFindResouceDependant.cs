using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;

public class HikerFindResouceDependant : EditorWindow
{
    Object dependantTarget;
    string folders = findInFolder;
    string[] dependants;

    const string findInFolder = "Assets/Resources";

    static private string[] FindResourcesDependant(Object target, string[] searchInFolders)
    {
        EditorUtility.DisplayProgressBar("Find...", "Checking...", 0);
        //var objs = Selection.GetFiltered<Object>(SelectionMode.Assets);
        string[] assetsInRes = AssetDatabase.FindAssets(string.Empty, searchInFolders);
        List<string> dependants = new List<string>();

        var setGUIDs = new HashSet<string>();
        foreach (var s in assetsInRes)
        {
            setGUIDs.Add(s);
        }
        //for (int i = 0; i < objs.Length; ++i)
        {
            var o = target;

            var objPath = AssetDatabase.GetAssetPath(o);

            //float per = (float)i / objs.Length;
            EditorUtility.DisplayProgressBar("Find dependants from " + o.name, "Checking...", 0);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Found dependants to {0}:\n", objPath);

            int k = 0;
            foreach (var assetGUIID in setGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUIID);

                float per2 = (float)k / setGUIDs.Count;// / objs.Length;
                EditorUtility.DisplayProgressBar("Find dependants from " + o.name, "Checking " + assetGUIID, per2);

                if (assetPath == objPath)
                    continue;

                var deps = AssetDatabase.GetDependencies(assetPath);

                if (deps.Contains(objPath))
                {
                    dependants.Add(assetPath);

                    sb.AppendLine(assetPath);
                }
                k++;
            }

            Debug.Log(sb.ToString());
        }

        EditorUtility.ClearProgressBar();

        return dependants.ToArray();
    }

    Vector2 scrollPos = Vector2.zero;
    private void OnGUI()
    {
        EditorGUILayout.PrefixLabel("Search In Folders");
        folders = EditorGUILayout.TextArea(folders);

        if (dependantTarget == null)
        {
            EditorGUILayout.LabelField("Please select dependants target asset");

            var newObj = EditorGUILayout.ObjectField("Target:", dependantTarget, typeof(Object), false);

            if (newObj != null)
            {
                dependantTarget = newObj;
            }

            var curSelect = Selection.activeObject;
            if (curSelect != null)
            {
                if (GUI.Button(EditorGUILayout.GetControlRect(), "Select " + curSelect.name))
                {
                    dependantTarget = curSelect;
                }
            }
        }
        else
        {
            var newObj = EditorGUILayout.ObjectField("Target:", dependantTarget, typeof(Object), false);
            //if (GUI.Button(EditorGUILayout.GetControlRect(), dependantTarget.name))
            //{
            //    Selection.SetActiveObjectWithContext(dependantTarget, this);
            //}
            if (newObj != dependantTarget)
            {
                dependantTarget = newObj;
                dependants = null;
            }

            if (dependants == null)
            {
                if (GUI.Button(EditorGUILayout.GetControlRect(), "Find dependants"))
                {
                    var searchInFolders = folders.Split(',');
                    dependants = FindResourcesDependant(dependantTarget, searchInFolders);
                }
            }
            else
            {
                if (GUI.Button(EditorGUILayout.GetControlRect(false, GUILayout.MaxWidth(100)), "Re scan"))
                {
                    var searchInFolders = folders.Split(',');
                    dependants = FindResourcesDependant(dependantTarget, searchInFolders);
                }

                EditorGUILayout.Space();

                EditorGUI.LabelField(EditorGUILayout.GetControlRect(false, GUILayout.MaxWidth(100)), "Dependants " + dependants.Length);

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var dep in dependants)
                {
                    var subPath = dep.Substring(dep.LastIndexOf(findInFolder) + findInFolder.Length);

                    if (GUI.Button(EditorGUILayout.GetControlRect(GUILayout.MinWidth(200), GUILayout.MaxWidth(300)), subPath))
                    {
                        Selection.SetActiveObjectWithContext(AssetDatabase.LoadMainAssetAtPath(dep), Selection.activeContext);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }

    [MenuItem("Hiker/Find Resources Dependants")]
    private static void ShowWindow()
    {
        var window = EditorWindow.CreateInstance<HikerFindResouceDependant>();
        window.titleContent = new GUIContent("Resource Dependant");
        window.Show();
    }

    [MenuItem("Assets/Move to Old Resources")]
    private static void MoveToOldRes()
    {
        var curSelects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets | SelectionMode.TopLevel);
        if (curSelects != null)
        {
            foreach (var obj in curSelects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var subPath = path.Substring(findInFolder.Length + 1);

                var folders = subPath.Split('/');
                string fPath = "Assets/OldResources";
                int count = 0;
                foreach (var f in folders)
                {
                    if (count == folders.Length - 1)
                        continue;

                    string nextPath = fPath + "/" + f;
                    if (AssetDatabase.IsValidFolder(nextPath) == false)
                    {
                        Debug.Log("create folder " + nextPath);
                        AssetDatabase.CreateFolder(fPath, f);
                    }

                    count++;
                    fPath = nextPath;
                }
                string newPath = fPath + "/" + folders[folders.Length - 1];
                string check = AssetDatabase.ValidateMoveAsset(path, newPath);
                if (string.IsNullOrEmpty(check))
                {
                    AssetDatabase.MoveAsset(path, newPath);
                }
                else
                {
                    Debug.Log(check);
                }
            }
        }
    }
}
