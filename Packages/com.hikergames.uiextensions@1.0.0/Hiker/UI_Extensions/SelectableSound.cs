using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hiker.UI
{
    [AddComponentMenu("UI/Extensions/Selectable Sound")]
    [RequireComponent(typeof(Selectable))]
    public class SelectableSound : MonoBehaviour, IPointerClickHandler
    {
        public AudioClip sound;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (sound != null && Hiker.GUI.SoundManager.instance)
            {
                Hiker.GUI.SoundManager.instance.PlaySound(sound);
            }
        }
    }

}
