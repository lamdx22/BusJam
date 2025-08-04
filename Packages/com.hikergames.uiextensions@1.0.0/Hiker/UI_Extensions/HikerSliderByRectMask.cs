using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hiker.UI
{
    public class HikerSliderByRectMask : MonoBehaviour
    {
        [SerializeField]
        RectMask2D rectMask;
        public Slider.Direction direction;

        RectTransform rectMaskTran;

        float mValue = 0;
        private void Awake()
        {
            var rectTran = GetRectMaskTran();
        }

        RectTransform GetRectMaskTran()
        {
            if (rectMaskTran)
            {
                return rectMaskTran;
            }
            if (rectMask)
            {
                rectMaskTran = rectMask.GetComponent<RectTransform>();
            }
            return rectMaskTran;
        }

        public float value
        {
            get { return mValue; }
            set
            {
                mValue = Mathf.Clamp01(value);
                var rectTran = GetRectMaskTran();
                if (rectTran)
                {
                    var rect = rectTran.rect;
                    rectMask.padding = GetPaddingByValue(mValue, rect.width, rect.height);
                }
            }
        }

        public Vector4 GetPaddingByValue(float val, float width, float height)
        {
            /// duongrs
            /// moi chi test voi case Horizon
            return new Vector4(
                direction == Slider.Direction.RightToLeft ? (1f - val) * width : 0, // left
                direction == Slider.Direction.TopToBottom ? (1f - val) * height : 0, // bot
                direction == Slider.Direction.LeftToRight ? (1f - val) * width : 0, // right
                direction == Slider.Direction.BottomToTop ? (1f - val) * height : 0); // top
        }

        private void OnRectTransformDimensionsChange()
        {
            value = mValue;
        }
    }

}