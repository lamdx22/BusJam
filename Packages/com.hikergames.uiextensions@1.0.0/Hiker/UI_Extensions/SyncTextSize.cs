using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hiker.UI
{
    public class SyncTextSize : MonoBehaviour
    {
        public Text[] texts;
        public float timeDelaySync = 0.1f;
        public float cycleResizeOnUpdate = 0;

        int curMinSize = 10000;
        int originMaxSize = 10000;
        private void Awake()
        {
            originMaxSize = 10000;
            for (int i = 0; i < texts.Length; ++i)
            {
                if (texts[i] != null &&
                    texts[i].resizeTextMaxSize < originMaxSize)
                {
                    originMaxSize = texts[i].resizeTextMaxSize;
                }
            }
        }

        private IEnumerator Start()
        {
            cycleCheck = cycleResizeOnUpdate;
            yield return new WaitForSecondsRealtime(timeDelaySync);
            RecalculateSize();
        }

        float cycleCheck = 0;
        private void Update()
        {
            if (cycleCheck <= 0)
            {
                if (cycleResizeOnUpdate > 0)
                {
                    cycleCheck = cycleResizeOnUpdate;
                    RecalculateSize();
                }
            }
            else
            {
                cycleCheck -= Time.unscaledDeltaTime;
            }
        }

        public void RecalculateSize()
        {
            int minSize = GetMinSize();
            if (minSize != curMinSize)
            {
                curMinSize = minSize;
                ApplySize();
            }
        }

        int GetMinSize()
        {
            int minSize = 10000;
            for (int i = 0; i < texts.Length; ++i)
            {
                if (texts[i] != null &&
                    texts[i].isActiveAndEnabled &&
                    texts[i].cachedTextGenerator != null &&
                    texts[i].cachedTextGenerator.fontSizeUsedForBestFit < minSize)
                {
                    minSize = texts[i].cachedTextGenerator.fontSizeUsedForBestFit;
                }
            }

            if (texts.Length > 0 && texts[0] != null && texts[0].canvas != null)
            {
                var textSize = Mathf.CeilToInt(minSize / texts[0].canvas.scaleFactor);
                return textSize;
            }
            return curMinSize;
        }

        void ApplySize()
        {
            if (texts != null)
            {
                for (int i = 0; i < texts.Length; ++i)
                {
                    if (texts[i] != null && texts[i].isActiveAndEnabled)
                    {
                        texts[i].resizeTextMaxSize = Mathf.Max(texts[i].resizeTextMinSize, curMinSize);
                    }
                }
            }
        }
    }
}

