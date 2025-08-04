using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Hiker
{
    public static class HikerLog
    {
        static void InternalLogFormat(string msg, string chanel, string htmlColor)
        {
            UnityEngine.Debug.LogFormat("<color=white>[HIKER{3}]</color> {0}{2}{1}",
                string.IsNullOrWhiteSpace(htmlColor) ? string.Empty : "<color="+htmlColor+">",
                string.IsNullOrWhiteSpace(htmlColor) ? string.Empty : "</color>",
                msg,
                string.IsNullOrWhiteSpace(chanel) ? string.Empty : "-" + chanel);
        }
        static void InternalWarnFormat(string msg, string chanel, string htmlColor)
        {
            UnityEngine.Debug.LogWarningFormat("<color=white>[HIKER{3}]</color> {0}{2}{1}",
                string.IsNullOrWhiteSpace(htmlColor) ? string.Empty : "<color=" + htmlColor + ">",
                string.IsNullOrWhiteSpace(htmlColor) ? string.Empty : "</color>",
                msg,
                string.IsNullOrWhiteSpace(chanel) ? string.Empty : "-" + chanel);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogEditorOnly(string msg, string chanel = "", string htmlColor = "white")
        {
            InternalLogFormat(msg, chanel, htmlColor);
        }

        [Conditional("DEBUG")]
        public static void LogDebugOnly(string msg, string chanel = "", string htmlColor = "white")
        {
            InternalLogFormat(msg, chanel, htmlColor);
        }

        [Conditional("UNITY_EDITOR")]
        public static void WarnEditorOnly(string msg, string chanel = "", string htmlColor = "white")
        {
            InternalWarnFormat(msg, chanel, htmlColor);
        }

        [Conditional("DEBUG")]
        public static void WarnDebugOnly(string msg, string chanel = "", string htmlColor = "white")
        {
            InternalWarnFormat(msg, chanel, htmlColor);
        }
    }
}

