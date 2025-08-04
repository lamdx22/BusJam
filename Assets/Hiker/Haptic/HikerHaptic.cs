
using System.Collections;
using System.Collections.Generic;

#if UNITY_IPHONE
using AOT;
using Newtonsoft.Json;
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace Hiker.GUI
{
    public class HikerHaptic : MonoBehaviour
    {
        public static LogsLevel LogLevel = LogsLevel.Verbose;
        public static HikerHaptic instance = null;

        public bool HapticEnable = true;

        public static bool IsHapticSupport { get; private set; } = true;

#if UNITY_ANDROID

        private static AndroidJavaObject _androidPlugin;

        private static AndroidJavaObject androidPlugin
        {
            get
            {
                if (_androidPlugin != null)
                {
                    return _androidPlugin;
                }

                using (var javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        _androidPlugin =
                            new AndroidJavaObject("com.hikergames.hikerhaptic.HikerHaptic", currentActivity);
                    }
                }

                return _androidPlugin;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millisec">The number of milliseconds to vibrate. This must be a positive number.</param>
        /// <param name="amplitude">The strength of the vibration. This must be a value between 1 and 255</param>
        public void PlayOneShot(int millisec, int amplitude)
        {
            //Debug.Log("PlayOneShot");
            if (amplitude < 1 || amplitude > 255)
            {
                amplitude = Mathf.Clamp(amplitude, 1, 255);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            androidPlugin.Call("playOneShot", millisec, amplitude);
#endif
        }
        public void PlayPredefined(VibrationEffect effectId)
        {
            //Debug.Log("PlayPredefined");

#if UNITY_ANDROID && !UNITY_EDITOR
        androidPlugin.Call("playPredefined", (int)effectId);
#endif
        }

        public enum VibrationEffect
        {
            EFFECT_CLICK = 0,
            EFFECT_DOUBLE_CLICK = 1,
            EFFECT_TICK = 2,
            EFFECT_HEAVY_CLICK = 5,
        }

#endif

#if UNITY_IPHONE
        #region DllImport

        private delegate void HapticStoppedDelegate(int reason);

        public delegate void HapticStoppedReasonDelegate(int stopCode);

        /// <summary>
        /// Event after pattern ends. 0 - Ok; else error code
        /// </summary>
        public static event HapticStoppedReasonDelegate OnHapticStopped;

        public static event HapticStoppedReasonDelegate OnPatternStopped;

#if !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityPlayContinuous(float intensity, float sharpness, float durationInSeconds);
		[DllImport("__Internal")]
        private static extern void _hikerHapticUnityPlayTransient(float intensity, float sharpness);
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityStop();
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityStopPlayer();
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityupdateContinuousHaptics(float intensity, float sharpness);
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityplayWithDictionaryPattern(string jsonDict);
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityplayWIthAHAPFile(string filename);
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityplayWithAHAPFileFromURLAsString(string url);
        [DllImport("__Internal")]
        private static extern bool _hikerHapticUnityIsSupport();
        [DllImport("__Internal")]
        private static extern void _hikerHapticUnityRegisterCallback(HapticStoppedDelegate patternFinishedCallback, HapticStoppedDelegate engineStoppedCallback);
#else
        private static void _hikerHapticUnityPlayContinuous(float intensity, float sharpness, float durationInSeconds)
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Play Continuous with Intensity: {0} and Sharpness: {1} for {2} seconds", intensity.ToString(), sharpness.ToString(), durationInSeconds.ToString());
        }

        private static void _hikerHapticUnityPlayTransient(float intensity, float sharpness)
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Play Transient with Intensity: {0} and Sharpness: {1}", intensity.ToString(), sharpness.ToString());
        }

        private static void _hikerHapticUnityStop()
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Stop");
        }

        private static void _hikerHapticUnityStopPlayer()
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Stop Player");
        }

        private static void _hikerHapticUnityupdateContinuousHaptics(float intensity, float sharpness)
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Update Continuous Params with Intensity: {0} and Sharpness: {1}", intensity.ToString(), sharpness.ToString());
        }

        private static void _hikerHapticUnityplayWithDictionaryPattern(string jsonDict)
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Play from Pattern: {0}", jsonDict);
        }

        private static void _hikerHapticUnityplayWIthAHAPFile(string filename)
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Play from File with Name: {0}", filename);
        }

        private static void _hikerHapticUnityplayWithAHAPFileFromURLAsString(string url)
        {
            if (LogLevel > LogsLevel.None)
                Debug.LogFormat("[HikerHaptic] Play from File by URL: {0}", url);
        }

        private static bool _hikerHapticUnityIsSupport()
        {
            return false;
        }

        private static void _hikerHapticUnityRegisterCallback(HapticStoppedDelegate patternFinishedCallback, HapticStoppedDelegate engineStoppedCallback)
        {
        }
#endif

        #endregion // DllImport

        public static void PlayContinuous(float intensity, float sharpness, float durationInSeconds)
        {
            if (!IsHapticSupport) return;

            _hikerHapticUnityPlayContinuous(intensity, sharpness, durationInSeconds);
        }

        public static void PlayContinuous()
        {
            PlayContinuous(0.5f, 0.5f, 30f);
        }

        public static void PlayPattern(HapticsPattern pattern)
        {
            if (!IsHapticSupport) return;

            var json = JsonConvert.SerializeObject(pattern, Formatting.Indented);
            if (LogLevel > LogsLevel.None)
                Debug.Log($"[HikerHaptic] {json}");
            PlayPattern(json);
        }

        public static void PlayPattern(string json)
        {
            if (!IsHapticSupport) return;
            _hikerHapticUnityplayWithDictionaryPattern(json);
        }

        public static void PlayTransient(float intensity, float sharpness)
        {
            if (!IsHapticSupport) return;
            _hikerHapticUnityPlayTransient(intensity, sharpness);
        }

        public static void PlayTransient()
        {
            PlayTransient(0.5f, 0.5f);
        }

        public static void PlayFromFileWithName(string filename)
        {
            if (!IsHapticSupport) return;
            _hikerHapticUnityplayWIthAHAPFile(filename);
        }

        public static void PlayFromFileWithURL(string url)
        {
            if (!IsHapticSupport) return;
            _hikerHapticUnityplayWithAHAPFileFromURLAsString(url);
        }

        public static void Stop()
        {
            if (!IsHapticSupport) return;
            _hikerHapticUnityStop();
        }

        public static void StopPlayer()
        {
            if (!IsHapticSupport) return;
            _hikerHapticUnityStopPlayer();
        }

        public static void UpdateContinuousValues(float intensity, float sharpness)
        {
            if (!IsHapticSupport) return;
            int roundDigit = 1000;
            _hikerHapticUnityupdateContinuousHaptics(RoundToDigits(intensity, roundDigit), RoundToDigits(sharpness, roundDigit));
        }

        private static float RoundToDigits(float val, int num = 1000)
        {
            return Mathf.Round(val * num) / num;
        }

        [MonoPInvokeCallback(typeof(HapticStoppedDelegate))]
        private static void HapticStoppedCallback(int code)
        {
            OnHapticStopped?.Invoke(code);
        }

        [MonoPInvokeCallback(typeof(HapticStoppedDelegate))]
        private static void PatternStoppedCallback(int code)
        {
            OnPatternStopped?.Invoke(code);
        }
#endif

        public void PlayTapEffect()
        {
            //Debug.Log("PlayPredefined");

            if (HapticEnable == false || IsHapticSupport == false) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            androidPlugin.Call("playTapEffect");
#elif UNITY_IPHONE //&& !UNITY_EDITOR
            PlayTransient(0.4f, 1f);
#endif
        }

        public void PlayHeavyImpact()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayOneShot(500, 250);
#elif UNITY_IPHONE //&& !UNITY_EDITOR
            PlayTransient(1f, 1f);
#endif
        }

        private void Awake()
        {
        }

        public void Initialize()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            IsHapticSupport = false;
#elif UNITY_ANDROID
            if (_androidPlugin == null)
            {
                using (var javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        _androidPlugin =
                            new AndroidJavaObject("com.hikergames.hikerhaptic.HikerHaptic", currentActivity);
                    }
                }
            }

            IsHapticSupport = androidPlugin.Call<bool>("isSupportHaptic");
#elif UNITY_IPHONE
#if DEBUG
            LogLevel = LogsLevel.Verbose;
#else
            LogLevel = LogsLevel.None;
#endif
            IsHapticSupport = Application.platform == RuntimePlatform.IPhonePlayer && _hikerHapticUnityIsSupport();
            _hikerHapticUnityRegisterCallback(PatternStoppedCallback, HapticStoppedCallback);
#endif
        }

        public void LoadPrefSetting()
        {
            HapticEnable = (PlayerPrefs.GetInt("HapticEnable", 1) == 1);
        }

        public void SavePrefSetting()
        {
            PlayerPrefs.SetInt("HapticEnable", HapticEnable ? 1 : 0);
        }

        private void OnEnable()
        {
            if (instance == null)
            {
                instance = this;
            }

            Initialize();

            LoadPrefSetting();
        }
    }

    public enum LogsLevel
    {
        None,
        Verbose
    }
}