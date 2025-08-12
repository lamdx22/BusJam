using DG.Tweening;
using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class PopupThang : MonoBehaviour
    {
        public AudioClip winClip;

        public void Show(int gold)
        {
            gameObject.SetActive(true);
            LunaManager.Instance.EndGameAndGoToStore();
            //Hiker.GUI.SoundManager.instance.PlaySoundWin();
            MySoundManager.Instance.PlaySoundWin();
        }

        //public void OnUserTapToStore()
        //{
        //    GameManager.instance.GoToStore();
        //}
    }
}
