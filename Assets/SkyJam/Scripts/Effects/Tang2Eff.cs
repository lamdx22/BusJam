using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class Tang2Eff : XeEff
    {
        public XeVisual visual;
        public bool IsGetOut { get { return isGetout; } }

        bool isPoppingMan = false;
        bool isGetout = false;

        public override int PopMan(Man man, int c)
        {
            if (isPoppingMan) return -1;

            if (visual.IsFull)
            {
                return 0;
            }

            if (visual.Color == man.Color)
            {
                StartCoroutine(DoPopMan(man, c));

                return 1;
            }

            return -1;
        }
        public override ColorEnum MauXe()
        {
            if (visual.IsFull) return ColorEnum.None;

            return visual.Color;
        }

        IEnumerator DoPopMan(Man man, int c)
        {
            isPoppingMan = true;
            visual.StartCoroutine(visual.DoPopMan(man, c));
            yield return null;

            if (visual.IsFull && IsGetOut == false)
            {
                OnGetOut();
                yield return new WaitForSecondsRealtime(0.2f);
            }
            isPoppingMan = false;
        }

        public void OnGetOut()
        {
            if (IsGetOut == false)
            {
                isGetout = true;
                visual.StartCoroutine(visual.DoMoveOut());
            }
        }

        public override void CloneState(XeEff clone)
        {
            base.CloneState(clone);
            
            if (clone.extType == XeExtType.Tang2)
            {
                var eff = clone as Tang2Eff;
                if (eff != null)
                {
                    visual.SetColor(eff.visual.Color);
                }
            }
        }
    }
}
