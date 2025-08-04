using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SkyJam
{
    public class ScreenPlayable : MonoBehaviour
    {
        [SerializeField] TMP_Text lbGold;

        [SerializeField] TMP_Text lbLevel;

        [SerializeField] Transform bang3D;
        [SerializeField] GameObject fxFrozenPrefab;
        [SerializeField] Image panelFrozenTime;
        [SerializeField] GameObject[] doKhoObjs;

        public GameObject grpPowUp;
        public ThoiGianLevel dongHo;
        public Transform timeIcon;

        public Transform btnMagnet;
        public Transform btnHammer;
        public Transform btnFreeze;
        public Transform btnChainsaw;

        private GameObject fxFrozen = null;

        public long CurGold { get; private set; }

        public static ScreenPlayable instance;
        private void OnEnable()
        {
            instance = this;
        }

        public void SyncNetworkData()
        {
            UpdateGold(GameManager.instance.PInfo.Gold);
        }
        public void UpdateGold(long val)
        {
            CurGold = val;
            lbGold.text = val.ToString();
        }
        public void InitLevel(int lvl)
        {
            //lbLevel.text = string.Format(Localization.Get("LevelShortLabel"), lvl);

            int doKho = GameManager.GetDoKhoLevel(lvl - 1);
            if (doKho > 0 && doKho >= doKhoObjs.Length)
            {
                doKho = 0;
            }

            for (int i = 0; i < doKhoObjs.Length; ++i)
            {
                var dkit = doKhoObjs[i];
                if (dkit)
                {
                    dkit.SetActive(i == doKho);
                }
            }

            dongHo.UpdateTime();
        }

        public void StartFXFrozen()
        {
            if (fxFrozen == null)
            {
                fxFrozen = Instantiate(fxFrozenPrefab, ScreenPlayable.instance.transform.position, Quaternion.identity, transform);
            }

            if (fxFrozen != null && fxFrozen.gameObject.activeSelf == false)
            {
                fxFrozen.gameObject.SetActive(true);
                SoundManager.instance?.PlaySound("SFX/battle_pu_freeze_start");
            }

            panelFrozenTime.gameObject.SetActive(true);
            var c = panelFrozenTime.color;
            c.a = 0f;
            panelFrozenTime.color = c;
            panelFrozenTime.DOKill();
            panelFrozenTime.DOFade(1f, 0.15f).SetUpdate(true);
        }

        public void TurnOffFXFrozen()
        {
            if (fxFrozen != null && fxFrozen.gameObject.activeSelf)
            {
                fxFrozen.gameObject.SetActive(false);

                //SoundManager.instance?.PlaySound("SFX/battle_pu_freeze_end");

                panelFrozenTime.DOKill();
                panelFrozenTime.DOFade(0f, 0.15f).SetUpdate(true).OnComplete(() =>
                {
                    panelFrozenTime.gameObject.SetActive(false);
                });
            }
        }

        //[GUIDelegate]
        //public void OnBtnPause()
        //{
        //    PopupPauseLevel.Create();
        //}
        //[GUIDelegate]
        public void OnBtnDungBua()
        {
            if (LevelManager.instance == null ||
                LevelManager.instance.State < LevelStatus.Inited ||
                LevelManager.instance.State >= LevelStatus.Ended)
            {
                return;
            }

            grpPowUp.gameObject.SetActive(false);
            LevelManager.instance.OnKichHoatBua();
        }
        //[GUIDelegate]
        public void OnBtnDungNC()
        {
            if (LevelManager.instance == null ||
                LevelManager.instance.State < LevelStatus.Inited ||
                LevelManager.instance.State >= LevelStatus.Ended)
            {
                return;
            }

            grpPowUp.gameObject.SetActive(false);
            LevelManager.instance.OnKichHoatNamCham();
        }


        //[GUIDelegate]
        public void OnBtnDungCua()
        {
            if (LevelManager.instance == null ||
                LevelManager.instance.State < LevelStatus.Inited ||
                LevelManager.instance.State >= LevelStatus.Ended)
            {
                return;
            }


            grpPowUp.gameObject.SetActive(false);
            LevelManager.instance.OnKichHoatCua();
        }

        //[GUIDelegate]
        public void OnBtnDungBang()
        {
            if (LevelManager.instance == null ||
                LevelManager.instance.State < LevelStatus.Inited ||
                LevelManager.instance.State >= LevelStatus.Ended)
            {
                return;
            }

            StartCoroutine(DoUseBang());
        }

        IEnumerator DoUseBang()
        {
            bang3D.gameObject.SetActive(true);
            bang3D.transform.localPosition = Vector3.zero;
            bang3D.transform.localRotation = Quaternion.identity;

            bang3D.transform.DOMove(dongHo.transform.position, .6f).SetUpdate(true);
            bang3D.transform.DORotate(Vector3.up * 1080f, .6f, RotateMode.WorldAxisAdd).SetUpdate(true).SetEase(Ease.Linear);
            bang3D.transform.DOScale(0.6f, 0.3f).SetUpdate(true);

            yield return new WaitForSecondsRealtime(0.6f);

            LevelManager.instance?.OnUseFreezePowerup();

            bang3D.gameObject.SetActive(false);
        }

        //[GUIDelegate]
        public void OnBtnHuyNamCHam()
        {
            //grpPowUp.gameObject.SetActive(true);
            LevelManager.instance?.OnHuyNamCham();
        }
        //[GUIDelegate]
        public void OnBtnHuyBua()
        {
            //grpPowUp.gameObject.SetActive(true);
            LevelManager.instance?.OnHuyBua();
        }
        //[GUIDelegate]
        public void OnBtnHuyCua()
        {
            //grpPowUp.gameObject.SetActive(true);
            LevelManager.instance?.OnHuyCua();
        }
    }
}
