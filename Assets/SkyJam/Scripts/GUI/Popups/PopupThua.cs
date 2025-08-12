using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class PopupThua : MonoBehaviour
    {
        public AudioClip loseClip;

        public void Show()
        {
            gameObject.SetActive(true);
            LunaManager.Instance.EndGameAndGoToStore();
            //Hiker.GUI.SoundManager.instance.PlaySoundLose();
            MySoundManager.Instance.PlaySoundLose();
        }

        //public void OnUserTapToStore()
        //{
        //    GameManager.instance.GoToStore();
        //}

    }
}