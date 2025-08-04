using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hiker.GUI
{
    [RequireComponent(typeof(RectTransform))]
    public class ScreenBase : MonoBehaviour
    {
        [SerializeField]
        bool destroyWhenDeactive = false;

        public bool DestroyWhenDeactive { get { return destroyWhenDeactive; } set { destroyWhenDeactive = value; } }

        public virtual void OnActive()
        {

        }
        public virtual void OnDeactive()
        {

        }

        protected virtual void OnRectTransformDimensionsChange()
        {

        }

        public virtual bool OnBackBtnClick()
        {
            return false;
        }

        public virtual bool IsShouldShowBanner()
        {
            return false;
        }
    }
}