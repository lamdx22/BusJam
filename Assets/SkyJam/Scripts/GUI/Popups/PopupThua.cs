using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class PopupThua : MonoBehaviour
    {
        public void Show()
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