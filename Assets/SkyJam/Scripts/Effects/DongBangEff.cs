using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkyJam
{
    public class DongBangEff : XeEff
    {
        public GameObject vfx;
        public TMP_Text lbNum;

        int curNum = 0;
        static readonly string[] numTexts = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        protected override void Awake()
        {
            base.Awake();
            extType = XeExtType.Bang;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Params != null && Params.Length > 0)
            {
                var n = Params[0];
                UpdateNum(n, n);
            }
            lbNum.transform.rotation = Quaternion.identity;
        }

        protected override void Update()
        {
            base.Update();

            //if (Application.isPlaying == false)
//            {
//                lbNum.transform.rotation = Quaternion.identity;
//            }
//#if UNITY_EDITOR
//            if (Params.Length > 0 && LevelDesignUI.instance && LevelDesignUI.instance.editMode.isOn)
//            {
//                UpdateNum(Params[0]);
//            }
//#endif
        }
        public override bool IsActivating()
        {
            if (base.IsActivating() && Params != null && Params[0] > 0)
            {
                return curNum > 0;
            }
            return false;
        }
        public override void OnAnXe(Xe veh)
        {
            base.OnAnXe(veh);
            if (veh == this) return;

            if (curNum > 0)
            {
                UpdateNum(curNum - 1, Params[0]);
            }

            if (curNum <= 0)
            {
                DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
            }
        }

        void UpdateNum(int num, int maxNum)
        {
            curNum = num;

            var initPos = -0.18f;
            var endPos = 0.2f;
            var underground = 1.6f;

            var p = vfx.transform.localPosition;
            var h = p.z;

            if (maxNum > 1)
            {
                if (curNum > 0)
                {
                    float rate = 1f - Mathf.Clamp01((float)(curNum - 1) / (maxNum - 1));
                    h = initPos + (endPos - initPos) * rate;
                }
                else
                {
                    h = underground;
                }
                p.z = h;
            }
            else
            {
                if (curNum > 0)
                {
                    h = initPos;
                }
                else
                {
                    h = underground;
                }
                p.z = h;
            }

            vfx.transform.DOLocalMoveZ(h, 0.5f).SetUpdate(true);

            if (num < numTexts.Length)
            {
                lbNum.text = numTexts[num];
            }
            else
            {
                lbNum.text = num.ToString();
            }
        }
        public override bool OnMoveXe()
        {
//#if UNITY_EDITOR
//            if (LevelDesignUI.instance && LevelDesignUI.instance.editMode.isOn)
//            {
//                return true;
//            }
//#endif
            if (curNum <= 0)
            {
                return true;
            }
            return false;
        }

        public override void CloneState(XeEff clone)
        {
            base.CloneState(clone);

            if (clone.extType == XeExtType.Bang)
            {
                var eff = clone as DongBangEff;
                if (eff != null)
                {
                    UpdateNum(eff.curNum, eff.Params[0]);
                }
            }
        }
    }
}