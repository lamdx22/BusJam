using DG.Tweening;
using Hiker.Util;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkyJam
{
    public class KhoaEff : XeEff
    {
        [SerializeField] MeshRenderer[] renderers;
        public ColorEnum color;

        static CauHinhGame cfg = null;

        protected override void Awake()
        {
            base.Awake();
            extType = XeExtType.Khoa;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateColor(color);
        }
        public override void OnAnXe(Xe veh)
        {
            base.OnAnXe(veh);
            if (veh != myXe || veh == null) return;

            if (LevelManager.instance != null)
            {
                var chiaKHoaVisual = Instantiate(transform.GetChild(0).gameObject);

                LevelManager.instance.StartCoroutine(LevelManager.instance.UnlockKhoa(chiaKHoaVisual, color));
            }
            gameObject.SetActive(false);
        }

        void UpdateColor(ColorEnum c)
        {
            color = c;

            if (cfg == null)
            {
                cfg = Resources.Load<CauHinhGame>("GameConfig");
            }
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if  (r != null)
                {
                    r.sharedMaterial = cfg.ChiaKhoa[(int)c - 1];
                }
            }
        }

        public override void CloneState(XeEff clone)
        {
            base.CloneState(clone);

            if (clone.extType == XeExtType.Khoa)
            {
                var eff = clone as KhoaEff;
                if (eff != null)
                {
                    UpdateColor(eff.color);
                }
            }
        }
    }
}