using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class PopupThua : MonoBehaviour
    {
        [SerializeField]
        AudioClip loseClip;
        public void Show()
        {
            gameObject.SetActive(true);
            if (loseClip != null)
            {
                SoundManager.instance?.PlaySound(loseClip, 1f);
            }
        }

        public void OnUserTapToStore()
        {
            GameManager.instance.GoToStore();
        }
    }
}