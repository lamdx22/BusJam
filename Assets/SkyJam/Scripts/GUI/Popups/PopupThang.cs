using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class PopupThang : MonoBehaviour
    {
        [SerializeField]
        AudioClip winClip;

        public void Show(int gold)
        {
            gameObject.SetActive(true);

            if (winClip != null)
            {
                SoundManager.instance?.PlaySound(winClip, 1f);
            }
        }

        public void OnUserTapToStore()
        {
            GameManager.instance.GoToStore();
        }
    }
}
