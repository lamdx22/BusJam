using DG.Tweening;
using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    [ExecuteInEditMode]
    public class Man : MonoBehaviour
    {
        [SerializeField] Transform visual;
        [SerializeField] Transform visualOnSeat;
        [SerializeField] Renderer[] renderers;
        public GameObject muObj;
        public ColorEnum Color = ColorEnum.None;

        public bool IsVIPPerson { get; set; } = false;
        public bool IsOnSeat { get { return visualOnSeat.gameObject.activeSelf; } }

        static CauHinhGame gameCfg = null;
        SplinePositioner positioner = null;
        int curOrder = -1;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            if (positioner == null)
            {
                positioner = GetComponent<SplinePositioner>();
            }

            if (positioner != null)
            {
                positioner.spline = GetComponentInParent<SplineComputer>();
            }

            SetColor(Color);
            SetStateOnSeat(false);
        }

        public void SetColor(ColorEnum c)
        {
            var cIdx = (int)c - 1;
            if (c != Color && c != ColorEnum.None)
            {
                //Debug.Log("Man set color " + c);
            }
            else
            {
                return;
            }

            Color = c;
            if (gameCfg == null)
            {
                gameCfg = Resources.Load<CauHinhGame>("GameConfig");
            }
            foreach (var renderer in renderers)
            {
                if (renderer) renderer.sharedMaterial = gameCfg.VatLieuMan[(int)c - 1];
            }
        }

        public void SetPositionOrder(int order, SplineComputer spline, float offset = 0.75f)
        {
            if (positioner != null)
            {
                positioner.spline = spline;
            }

            curOrder = order;

            if (positioner && positioner.spline != null)
            {
                positioner.SetDistance(order * 1.2f + offset);
            }
        }
        public void OnPopOut()
        {
            if (positioner != null)
            {
                positioner.spline = null;
            }
        }

        public IEnumerator DoTweenToOrder(int order)
        {
            if (curOrder >= 0)
            {
                int step = 0;
                if (curOrder < order)
                {
                    step = 1;
                }
                else if (curOrder > order)
                {
                    step = -1;
                }

                var t = curOrder;
                if (step != 0)
                {
                    while (t != order)
                    {
                        t += step;
                        curOrder = t;
                        if (positioner && positioner.spline != null)
                        {
                            positioner.SetDistance(t * 1.2f + 0.75f);
                        }
                        yield return new WaitForSecondsRealtime(0.03333f);
                    } 
                }
            }
            else
            {
                curOrder = order;
                if (positioner && positioner.spline != null)
                {
                    positioner.SetDistance(order * 1.2f + 0.75f);
                }
            }
        }

        public void SetStateOnSeat(bool isOnSeat)
        {
            if (visualOnSeat)
            {
                visualOnSeat.gameObject.SetActive(isOnSeat);
            }
            if (visual)
            {
                visual.gameObject.SetActive(!isOnSeat);
            }

            if (muObj != null)
            {
                muObj.gameObject.SetActive(isOnSeat == false && IsVIPPerson);
            }
        }
    }
}

