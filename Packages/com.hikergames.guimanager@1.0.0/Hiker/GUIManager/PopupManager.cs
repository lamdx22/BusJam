using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hiker.GUI
{
    public class PopupManager : MonoBehaviour
    {
        static PopupManager _instance = null;
        public static PopupManager instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = GameObject.Find("PopupContainer");
                    if (go != null)
                    {
                        _instance = go.GetComponent<PopupManager>();
                    }
                }

                return _instance;
            }
        }

        [System.NonSerialized]
        public List<PopupBase> activePopups = new List<PopupBase>();
        Stack<PopupBase> backStackPopups = new Stack<PopupBase>();

        Queue<PopupBase> queuePopups = new Queue<PopupBase>();

        private static Vector3 POPUP_DEFAULT_POSITION = Vector3.zero;
        private static Dictionary<string, GameObject> popupsPref = new Dictionary<string, GameObject>();
        private static Dictionary<string, GameObject> popupsCache = new Dictionary<string, GameObject>();

        public bool DisablePopupQueue { get; set; } = false;

        public bool IsHavePopupBlockControl()
        {
            if (activePopups.Count == 0) return false;
            for (int i = 0; i < activePopups.Count; ++i)
            {
                if (activePopups[i].noneBlockControl == false) return true;
            }

            return false;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            this.ClearAll();
        }

        private void Update()
        {
            if (DisablePopupQueue == false)
            {
                if (activePopups.Count == 0 && queuePopups.TryDequeue(out var popup))
                {
                    if (popup != null)
                    {
                        popup.gameObject.SetActive(true);
                    }
                }
            }
        }

        public void ClearAll()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (this.transform.GetChild(i) != null)
                {
                    Destroy(this.transform.GetChild(i).gameObject);
                }
            }

            this.activePopups.Clear();
        }

        public bool HaveBackStackPopup()
        {
            return backStackPopups.Count > 0;
        }

        public void OnBackBtnClick()
        {
            if (HaveBackStackPopup())
            {
                var popup = backStackPopups.Peek();
                popup.OnBackBtnClick();
            }
        }

        public void OnShowPopup(PopupBase popup)
        {
            if (this.activePopups.Contains(popup) == false) this.activePopups.Add(popup);
            if (popup.addToBackStack) backStackPopups.Push(popup);
        }

        public void OnHidePopup(PopupBase popup)
        {
            if (popup == null) return;
            int index = this.activePopups.IndexOf(popup);
            if (index >= 0)
            {
                this.activePopups.RemoveAt(index);
            }

            if (backStackPopups.Contains(popup))
            {
                var tempList = Hiker.Util.ListPool<PopupBase>.Claim();
                var peek = backStackPopups.Count > 0 ? backStackPopups.Peek() : null;
                while (backStackPopups.Count > 0 && peek != popup)
                {
                    tempList.Add(backStackPopups.Pop());
                    peek = backStackPopups.Count > 0 ? backStackPopups.Peek() : null;
                }

                if (peek == popup)
                {
                    backStackPopups.Pop();
                }

                for (int i = tempList.Count - 1; i >= 0; --i)
                {
                    backStackPopups.Push(tempList[i]);
                }
                Hiker.Util.ListPool<PopupBase>.Release(tempList);
            }
        }

        public void OnCreatePopup(PopupBase popup)
        {
            if (popup == null) return;
        }

        private GameObject GetPopupPref(string popupName)
        {
            GameObject pref = null;
            if (!popupsPref.ContainsKey(popupName))
            {
                pref = Resources.Load<GameObject>("Popups/" + popupName) as GameObject;

                var p = pref.GetComponent<PopupBase>();
                if (p != null && p.nonCachePref)
                {

                }
                else
                {
                    popupsPref[popupName] = pref;
                }
            }
            else
            {
                pref = popupsPref[popupName];
                if (pref == null)
                {
                    popupsPref.Remove(popupName);
                    pref = this.GetPopupPref(popupName);
                }
            }
            return pref;
        }

        private GameObject GetPopupObj(string popupName, bool isMultiInstance = false)
        {
            GameObject obj = null;
            string name = popupName;
            if (!popupsCache.ContainsKey(name) || isMultiInstance)
            {
                GameObject pref = this.GetPopupPref(popupName);
                var boolPrefState = pref.gameObject.activeSelf;
                if (boolPrefState)
                {
                    pref.gameObject.SetActive(false);
                }
                obj = Instantiate(pref, transform);
                if (boolPrefState)
                {
                    pref.gameObject.SetActive(boolPrefState);
                }
            }
            else
            {
                obj = popupsCache[name];
                if (obj == null)
                {
                    popupsCache.Remove(name);
                    obj = this.GetPopupObj(popupName, isMultiInstance);
                }
            }
            return obj;
        }

        public GameObject PreloadPopup(string popupName)
        {
            var p = GetPopupObj(popupName);
            popupsCache[popupName] = p;
            return p;
        }

        public GameObject GetPopup(string popupName, bool useDefaultPos = true, Vector3 pos = default(Vector3), bool isMultiInstance = false)
        {
            var obj = GetPopupObj(popupName, isMultiInstance);
            obj.SetActive(true);
            if (useDefaultPos) pos = POPUP_DEFAULT_POSITION;

            obj.transform.localPosition = pos;

            return obj;
        }

        public GameObject GetPopupInQueue(string popupName, bool useDefaultPos = true, Vector3 pos = default(Vector3), bool isMultiInstance = false)
        {
            GameObject obj = GetPopupObj(popupName, isMultiInstance);

            if (useDefaultPos) pos = POPUP_DEFAULT_POSITION;

            obj.transform.localPosition = pos;

            queuePopups.Enqueue(obj.GetComponent<PopupBase>());

            return obj;
        }
    }
}