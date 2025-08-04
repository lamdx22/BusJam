using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hiker.GUI
{
    public class PopupBase : MonoBehaviour
    {
        public bool isModal = false;
        public bool addToBackStack = true;
        public bool hideIsDestroy = true;
        public float timeDelayDestroy = 0.25f;
        public bool noneBlockControl = false;
        [Tooltip("Use to exclude prefab popup out of cache pool in PopupManager")]
        public bool nonCachePref = false;
        [SerializeField]
        protected Animator appearAnimator;

        [SerializeField]
        protected TweenAlpha appearTweenAlpha;

        protected GameObject myGO = null;

        protected bool isClosed = false;

        public UnityEngine.Events.UnityEvent onPopupClosed;
        private static readonly int closeTrigger = Animator.StringToHash("close");
        private static readonly int openTrigger = Animator.StringToHash("open");


        protected virtual void Awake()
        {
            PopupManager.instance.OnCreatePopup(this);
            myGO = gameObject;
        }

        protected virtual void OnEnable()
        {
            if (appearAnimator != null)
            {
                appearAnimator.SetTrigger(openTrigger);
            }
            PopupManager.instance.OnShowPopup(this);
        }

        protected virtual void OnDisable()
        {
            PopupManager.instance.OnHidePopup(this);
            onPopupClosed?.Invoke();
        }

        public void MakePopupToTop()
        {
            transform.SetAsLastSibling();
        }

        [GUIDelegate]
        protected virtual void OnCleanUp()
        {
        }

        [GUIDelegate]
        public virtual void OnBackBtnClick()
        {
            if (isModal == false && addToBackStack)
            {
                OnCloseBtnClick();
            }
        }

        [GUIDelegate]
        public virtual void OnCloseBtnClick()
        {
            if (isClosed)
            {
                return;
            }

            isClosed = true;
            OnCleanUp();

            if (appearAnimator != null)
            {
                appearAnimator.SetTrigger(closeTrigger);

                if (appearTweenAlpha)
                    appearTweenAlpha.PlayReverse();

                Hiker.HikerUtils.DoAction(PopupManager.instance, Hide, timeDelayDestroy, true);
            }
            else
            {
                Hide();
            }
        }

        protected virtual void Hide()
        {
            if (myGO == null) return;

            if (hideIsDestroy)
            {
                Destroy(myGO);
            }
            else
            {
                myGO.SetActive(false);
            }
        }
    }
}