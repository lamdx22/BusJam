using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hiker.GUI
{
    public class GUIDelegateAttribute : System.Attribute {}

    public class GUIManager : MonoBehaviour
    {
        public static GUIManager Instance { get { if (instance == null) instance = FindObjectOfType<GUIManager>(); return instance; } }
        static GUIManager instance = null;

        public Transform ScreenContainer;
        public Transform PopupManagerTransform;
        public Transform TopCanvasTransform;

        //public TopCover ApplicationPauseGrp;
        //public float delayUnPauseUI = 0f;

        public string LastScreen { get; set; }
        public string CurrentScreen { get; set; }
        [System.NonSerialized]
        public Dictionary<string, ScreenBase> screens = new Dictionary<string, ScreenBase>();

#if UNITY_EDITOR
        [SerializeField] private bool simulateHasBanner = false;
#endif

        ScreenBase LoadScreen(string screen_name)
        {
            GameObject go = null;

            if (go == null)
            {
                go = Instantiate(Resources.Load<GameObject>("Screens/Screen" + screen_name), ScreenContainer);
            }

            if (go == null)
            {
                return null;
            }

            go.transform.localScale = Vector3.one;
            //go.SetActive(false);

            ScreenBase screen_base = go.GetComponent<ScreenBase>();

            if (screen_base == null)
            {
                return null;
            }

            if (screens.ContainsKey(screen_name))
            {
                screens[screen_name] = screen_base;
            }
            else
            {
                screens.Add(screen_name, screen_base);
            }

            return screen_base;
        }

        private void OnEnable()
        {
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnPreInit()
        {
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
#endif
        }

        void Awake()
        {
            //this.ProcessResolution();

            //if (ApplicationPauseGrp != null)
            //{
            //    ApplicationPauseGrp.gameObject.SetActive(false);
            //}

            if (instance != null && instance != this)
            {
                GameObject.Destroy(gameObject);
                return;
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

            this.ClearScreen();

#if !BANNER_ADS
            HikerSafeAreaWithBannerHelper.hasBanner = false;
#endif
        }

        private void OnApplicationPause(bool pause)
        {
//            if (pause)
//            {
//#if DEBUG
//                Debug.Log("App Paused");
//#endif
//                if (ApplicationPauseGrp != null)
//                {
//                    ApplicationPauseGrp.gameObject.SetActive(true);
//                }
//                delayUnPauseUI = 0f;
//            }
//            else
//            {
//#if DEBUG
//                Debug.Log("App Unpaused");
//#endif
//                if (ApplicationPauseGrp != null)
//                {
//                    if (delayUnPauseUI > 0)
//                    {
//                        if (ApplicationPauseGrp != null)
//                        {
//                            ApplicationPauseGrp.gameObject.SetActive(true);
//                        }
//                        ApplicationPauseGrp.GoDeactive(delayUnPauseUI);
//                        delayUnPauseUI = 0f;
//                    }
//                    else
//                    {
//                        ApplicationPauseGrp.gameObject.SetActive(false);
//                    }
//                }
//            }
        }

        public void ClearScreen()
        {
            ScreenContainer.DestroyChildren();
            this.screens.Clear();
        }

        public ScreenBase SetScreen(string screen_name, System.Action onScreenActive = null)
        {
            if (screens.ContainsKey(screen_name) == false)
            {
                ScreenBase screen = LoadScreen(screen_name);
            }

            if (CurrentScreen == screen_name)
            {
                var screen = this.screens[CurrentScreen];
                screen.OnActive();
                if (onScreenActive != null) onScreenActive();

                return screen;
            }
#if DEBUG
            Debug.Log("Screen: " + screen_name);
#endif
            LastScreen = CurrentScreen;

            ScreenBase curScreen = null;

            if (string.IsNullOrEmpty(CurrentScreen) == false &&
                screens.ContainsKey(CurrentScreen))
            {
                curScreen = screens[CurrentScreen];
                curScreen.OnDeactive();
                if (curScreen.DestroyWhenDeactive)
                {
                    screens.Remove(CurrentScreen);
                    curScreen.gameObject.SetActive(false);
                    Destroy(curScreen.gameObject);
                }
                else
                {
                    curScreen.gameObject.SetActive(false);
                }
            }

            curScreen = screens[screen_name];
            CurrentScreen = screen_name;

            if (!curScreen.gameObject.activeSelf)
                curScreen.gameObject.SetActive(true);

            curScreen.OnActive();

            if (onScreenActive != null) onScreenActive();

            if (TopCanvasTransform != null)
            {
                TopCanvasTransform.GetComponent<CanvasHelper>().ApplySafeArea();
            }

            return curScreen;
        }

        public ScreenBase GetCurrentScreen()
        {
            if (screens != null && string.IsNullOrEmpty(CurrentScreen) == false &&
                screens.ContainsKey(CurrentScreen))
            {
                return screens[CurrentScreen];
            }
            return null;
        }
    }

}

