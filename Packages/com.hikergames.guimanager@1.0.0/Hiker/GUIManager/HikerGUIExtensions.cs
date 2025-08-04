using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hiker.GUI
{
    public static class HikerGUIExtensions
    {
        static public void DestroyChildren(this Transform t)
        {
            bool isPlaying = Application.isPlaying;

            while (t.childCount != 0)
            {
                Transform child = t.GetChild(0);

                if (isPlaying)
                {
                    child.SetParent(null);
                    UnityEngine.GameObject.Destroy(child.gameObject);
                }
                else UnityEngine.GameObject.DestroyImmediate(child.gameObject);
            }
        }
    }
}
