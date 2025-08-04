using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hiker.UI
{
    [RequireComponent(typeof(Slider))]
    [ExecuteInEditMode]
    public class ColorSlider : MonoBehaviour
    {
        public Image sliderFill;
        public Color colorLoading = Color.grey;
        public Color colorFull = Color.yellow;
        public bool lerpColor = false;

        Slider mSlider;
        Slider GetSlider()
        {
            if (mSlider == null)
            {
                mSlider = GetComponent<Slider>();
            }

            return mSlider;
        }
        private void OnEnable()
        {
            GetSlider().onValueChanged.AddListener(OnSliderValueChanged);
            OnSliderValueChanged(GetSlider().value);
        }
        private void OnDisable()
        {
            GetSlider().onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        void OnSliderValueChanged(float t)
        {
            if (sliderFill)
            {
                if (lerpColor)
                {
                    sliderFill.color = Color.Lerp(colorLoading, colorFull, t);
                }
                else
                {
                    sliderFill.color = t < 1 ? colorLoading : colorFull;
                }
            }
        }
    }
}

