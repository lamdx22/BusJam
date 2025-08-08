using DG.Tweening;
using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class PopupThang : MonoBehaviour
    {
        public void Show(int gold)
        {
            gameObject.SetActive(true);
        }

        public void OnUserTapToStore()
        {
            GameManager.instance.GoToStore();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                GameManager.instance.GoToStore();
            }
        }
    }
}
