using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hiker.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleFollow : MonoBehaviour
    {
        public GameObject follower;
        public float delay = 0f;
        Toggle mToggle;
        RectTransform mRect;
        public Vector2 anchorOffset = Vector2.zero;
        private void Awake()
        {
            mToggle = GetComponent<Toggle>();
            mRect = GetComponent<RectTransform>();
        }

        private void Start()
        {
            mToggle.onValueChanged.AddListener(OnTogleValueChanged);
        }

        void OnTogleValueChanged(bool isActive)
        {

            if (delay <= 0)
            {
                if (isActive)
                {
                    if (follower != null)
                    {
                        var ts = TweenPosition.Begin(follower, 0.15f, mRect.TransformPoint(anchorOffset), true);

                        ts.ignoreTimeScale = true;
                        ts.worldSpace = true;
                    }
                }
            }
            else
            {
                Hiker.HikerUtils.DoAction(this, () =>
                {
                    if (mToggle.isOn)
                    {
                        if (follower != null)
                        {
                            var ts = TweenPosition.Begin(follower, 0.15f, mRect.TransformPoint(anchorOffset), true);

                            ts.ignoreTimeScale = true;
                            ts.worldSpace = true;
                        }
                    }
                }, delay, true);
            }
        }

        public void UpdateFollower()
        {
            if (mToggle)
            {
                OnTogleValueChanged(mToggle.isOn);
            }
        }
    }
}

