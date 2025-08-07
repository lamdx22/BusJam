using Dreamteck.Splines;
using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;

namespace SkyJam
{
    public class HangDoi : MonoBehaviour
    {
        [SerializeField] SplineComputer spline;
        [SerializeField] SpriteRenderer[] indicators;
        [SerializeField] TMP_Text lbNum;
        public KhoaObjVisual khoaObj;

        [SerializeField] private LayerMask xeLayer;

        private Collider2D triggerCollider;

        public QueueDirection dir;
        public ColorEnum mauKhoa = ColorEnum.None;

        public ColorManInQueue[] ColorMan;

        public int RemainMan { get { if (listMans == null) return 0; return listMans.Count; } }

        RSQueue<Man> listMans = null;
        static CauHinhGame gameCfg = null;

        Xe curXe = null;

        public int ColorIdx { get
            {
                if (listMans != null && listMans.IsEmpty == false)
                {
                    var p = listMans.Peek();
                    if (p != null) return ((int)p.Color - 1);
                }
                return -1;
            }
        }

        public int NumCurWait { get
            {
                if (listMans != null && listMans.IsEmpty == false)
                {
                    var p = listMans.Peek();
                    int c = 1;
                    for (int i = 1; i < listMans.Count; ++i)
                    {
                        if (p.Color == listMans[i].Color)
                        {
                            c++;
                        }
                        else
                        {
                            break; 
                        }
                    }
                    return c;
                }
                return 0;
            }
        }

        private void Awake()
        {
            if (listMans == null)
            {
                listMans = new RSQueue<Man>(GetComponentsInChildren<Man>());
            }

            triggerCollider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                RegenQueue();
            }
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (Application.isPlaying)
            {
                if (collision.attachedRigidbody != null)
                {
                    var xe = collision.attachedRigidbody.GetComponent<Xe>();
                    if (xe != null)
                    {
                        if (curXe != xe)
                        {
                            trigInterval = 0.1f;
                            //curTrigInterval = 0.1f;
                            curXe = xe;
                            triggPop = 0;
                        }
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (Application.isPlaying)
            {
                if (collision.attachedRigidbody != null)
                {
                    var xe = collision.attachedRigidbody.GetComponent<Xe>();
                    if (xe != null && curXe == xe)
                    {
                        //curXe = null;
                        //trigInterval = 0.1f;
                    }
                }
            }
        }


        //public void OnTriggerFromXe(Xe xe)
        //{
        //    if (curXe != xe)
        //    {
        //        trigInterval = 0.1f;
        //        curTrigInterval = 0.1f;
        //        curXe = xe;
        //        triggPop = 0;
        //    }
        //}

        //public void OnExitFromXe()
        //{
        //    curXe = null;
        //    if (xe != null && curXe == xe)
        //    {
        //        //Debug.Log("Exit2d");
        //        curXe = null;
        //    }
        //}

        public void Unlock()
        {
            if (mauKhoa > ColorEnum.None)
            {
                mauKhoa = ColorEnum.None;
            }

            if (khoaObj != null)
            {
                khoaObj.gameObject.SetActive(false);
            }

            SoundManager.instance?.PlaySound("SFX/chest_unlock");
        }

        int triggPop = 0;

        void OnTriggXe(Xe xe)
        {
            if (LevelManager.instance == null || LevelManager.instance.State != LevelStatus.Started)
                return;

            if (mauKhoa > ColorEnum.None) return;

            var c = ColorIdx;
            if (c >= 0 && c == ((int)xe.ActiveColor - 1) && listMans.IsEmpty == false)
            {
                var p = listMans.Peek();
                if (p != null && xe.PopMan(p, triggPop))
                {
                    triggPop++;
                    trigInterval *= 0.9f;
                    if (trigInterval < 0.02)
                    {
                        trigInterval = 0.02f;
                    }
                    OnDequeue();
                }
            }
        }

        void OnDequeue()
        {

            listMans.Dequeue();

            // Hiker.HikerUtils
            HikerUtils.DoAction(this, () =>
            {
                UpdateManPosInQueue();
            }, 0.15f, true);
        }

        public int GetRemainMen(List<Man> listRe)
        {
            if (listRe != null)
            {
                listRe.AddRange(listMans);
            }

            return listMans.Count;
        }

        float trigInterval = 0.1f;
        float curTrigInterval = 0f;
        // Update is called once per frame
        void Update()
        {
            
        }

        private void LateUpdate()
        {
            if (curXe != null)
            {
                if (curTrigInterval > 0f)
                {
                    curTrigInterval -= Time.deltaTime;
                }

                if (curTrigInterval <= 0f)
                {
                    curTrigInterval = trigInterval;
                    OnTriggXe(curXe);
                }
            }


        }

        public void UpdateManPosInQueue()
        {
            int remainMan = RemainMan;
            for (int i = 0; i < remainMan; ++i)
            {
                var obj = listMans[i];
                if (obj != null && obj.gameObject.activeSelf)
                {
                    obj.StartCoroutine(obj.DoTweenToOrder(i));
                }
            }

            if (listMans != null && remainMan > 0)
            {
                if (gameCfg == null)
                {
                    gameCfg = Resources.Load<CauHinhGame>("GameConfig");
                }

                var color = gameCfg.MauCoBan[(int)(listMans.Peek().Color) - 1];
                for (int i = 0; i < indicators.Length; ++i)
                {
                    if (indicators[i])
                    {
                        indicators[i].color = color;
                    }
                }
                lbNum.text = NumCurWait.ToString();
            }
            else
            {
                for (int i = 0; i < indicators.Length; ++i)
                {
                    if (indicators[i])
                    {
                        indicators[i].color = Color.white;
                    }
                }
                lbNum.text = NumCurWait.ToString();
            }
        }

        public void RegenQueue()
        {
            for (int i = 0; i < listMans.Count; ++i)
            {
                var m = listMans[i];
                if (m != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(m.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(m.gameObject);
                    }
                }
            }
            listMans.Clear();

            foreach (var m in GetComponentsInChildren<Man>(true))
            {
                if (m != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(m.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(m.gameObject);
                    }
                }
            }

            int totalMan = 0;
            for (int i = 0; i < ColorMan.Length; i++)
            {
                var c = ColorMan[i];
                if (c.Num > 0)
                {
                    totalMan += c.Num;
                }
            }

            var prefab = Resources.Load<GameObject>("Prefabs/Man");

            for (int i = 0; i < ColorMan.Length; ++i)
            {
                var c = ColorMan[i];

                if (c.Num > 0)
                {
                    for (int j = 0; j < c.Num; ++j)
                    {
                        var mObj = Instantiate(prefab, transform).GetComponent<Man>();
                        mObj.gameObject.hideFlags = HideFlags.HideAndDontSave;
                        //mObj.gameObject.hideFlags = HideFlags.None;
                        mObj.SetColor(c.Color);

                        listMans.Enqueue(mObj);
                    }
                }
            }

            for (int i = 0; i < totalMan; ++i)
            {
                var obj = listMans[i];
                if (obj != null)
                {
                    int num = i;
                    int cIdx = 0;
                    for (int j = 0; j < ColorMan.Length; ++j)
                    {
                        var c = ColorMan[j];

                        if (c.Num > 0 && num >= c.Num)
                        {
                            num -= c.Num;
                        }
                        else
                        if (c.Num > 0)
                        {
                            cIdx = j;
                            break;
                        }
                        cIdx = j;
                    }
                    obj.gameObject.SetActive(true);
                    obj.SetPositionOrder(i, spline, mauKhoa > ColorEnum.None ? 1.75f : 0.75f);
                    obj.SetColor(ColorMan[cIdx].Color);
                }
            }

            if (listMans != null && listMans.Count > 0)
            {
                if (gameCfg == null)
                {
                    gameCfg = Resources.Load<CauHinhGame>("GameConfig");
                }

                var color = gameCfg.MauCoBan[(int)(listMans.Peek().Color) - 1];
                for (int i = 0; i < indicators.Length; ++i)
                {
                    if (indicators[i])
                    {
                        indicators[i].color = color;
                    }
                }
                lbNum.text = NumCurWait.ToString();
            }
            else
            {
                for (int i = 0; i < indicators.Length; ++i)
                {
                    if (indicators[i])
                    {
                        indicators[i].color = Color.white;
                    }
                }
                lbNum.text = NumCurWait.ToString();
            }
        }

        public int RemoveManFromQueue(int idxInQueue, ColorEnum c, int num, List<Man> listResults)
        {
            if (listMans.Count == 0) return 0;
            if (c == ColorEnum.None) return 0;
            if (num <= 0) return 0;

            ColorEnum curColor = ColorEnum.None;
            int re = 0;
            for (int cIdx = -1, idx = 0; idx < listMans.Count && cIdx <= idxInQueue && re < num; ++idx)
            {
                var man = listMans[idx];
                if (man != null && man.Color != curColor)
                {
                    curColor = man.Color;
                    cIdx++;
                }
                if (cIdx == idxInQueue && man != null && man.Color == c)
                {
                    re++;
                    listMans.RemoveAt(idx);
                    idx--;
                    listResults.Add(man);
                }
            }

            return re;
        }
    }

    [System.Serializable]
    public struct ColorManInQueue
    {
        public ColorEnum Color;
        public int Num;
    }

    public enum QueueDirection
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum ColorEnum
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Orange = 4,
        Black = 5,
        LightPink = 6,
        Brown = 7,
        Purple = 8,
        Yellow = 9,
        White = 10,
        LightBlue = 11,
        DarkGreen = 12,
        LightBrown = 13,
        Pink = 14,
    }
}
