using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hiker.UI
{
    [RequireComponent(typeof(Image))]
    [ExecuteInEditMode]
    public class SyncImageWhenEnable : MonoBehaviour
    {
        public Image target;
        Image myImage;
        private void OnEnable()
        {
            if (myImage == null)
            {
                myImage = GetComponent<Image>();
            }

            if (target != null && myImage)
            {
                myImage.sprite = target.sprite;
            }
        }

        private void Update()
        {
            if (Application.isPlaying == false)
            {
                if (myImage == null)
                {
                    myImage = GetComponent<Image>();
                }

                if (target != null && myImage)
                {
                    if (myImage.sprite != target.sprite)
                    {
                        myImage.sprite = target.sprite;
                    }

                    if (myImage.pixelsPerUnitMultiplier != target.pixelsPerUnitMultiplier)
                    {
                        myImage.pixelsPerUnitMultiplier = target.pixelsPerUnitMultiplier;
                    }
                }
            }
        }
    }
}

