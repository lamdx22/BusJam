using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hiker.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleAnimation : MonoBehaviour
    {
        public Animator iconAnimator;
        Toggle mToggle;
        private void Awake()
        {
            mToggle = GetComponent<Toggle>();
        }
        private void Start()
        {
            mToggle.onValueChanged.AddListener(OnTogleValueChanged);
            if (iconAnimator && mToggle.isOn) iconAnimator.SetBool("IsOn", true);
        }

        public void OnTogleValueChanged(bool isActive)
        {
            if (iconAnimator) iconAnimator.SetBool("IsOn", isActive);
        }
    }
}

