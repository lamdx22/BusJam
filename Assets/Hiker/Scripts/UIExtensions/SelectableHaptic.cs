using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hiker.UI
{
    [AddComponentMenu("UI/Extensions/Selectable Haptic")]
    [RequireComponent(typeof(Selectable))]
    public class SelectableHaptic : MonoBehaviour, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (HikerHaptic.instance)
            {
                HikerHaptic.instance.PlayTapEffect();
            }
        }
    }

}
