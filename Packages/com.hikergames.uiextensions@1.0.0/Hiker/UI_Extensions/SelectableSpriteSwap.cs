using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hiker.UI
{
    [AddComponentMenu("UI/Extensions/Selectable Sprite Swap")]
    [RequireComponent(typeof(Selectable))]
    [ExecuteInEditMode]
    public class SelectableSpriteSwap : MonoBehaviour
    {
        public Sprite DisableSprite;
        public Sprite NormalSprite;

        public Image Target;

        Selectable _selectable;

        private void OnEnable()
        {
            if (_selectable == null) _selectable = GetComponent<Selectable>();
            if (NormalSprite == null)
            {
                if ( _selectable != null && Target != null && _selectable.interactable)
                {
                    NormalSprite = Target.sprite;
                }
            }
            if (DisableSprite == null)
            {
                if (_selectable != null && Target != null && _selectable.interactable == false)
                {
                    DisableSprite = Target.sprite;
                }
            }
        }

        private void Update()
        {
            if (_selectable && Target)
            {
                if (_selectable.IsInteractable())
                {
                    if (Target.sprite != NormalSprite && NormalSprite != null)
                    {
                        Target.sprite = NormalSprite;
                    }
                }
                else
                {
                    if (Target.sprite != DisableSprite && DisableSprite != null)
                    {
                        Target.sprite = DisableSprite;
                    }
                }
            }
        }
    }

}
