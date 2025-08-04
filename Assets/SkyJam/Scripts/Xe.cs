using DG.Tweening;
using Hiker;
using Hiker.GUI;
using Hiker.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class Xe : PickableObj
    {
        public XeVisual xeVisual;
        public XeEff[] hieuUng;

        public string XeNho;
        public Transform Tail;

        private bool isNhacLen = false;
        public bool IsGetOut { get { return getOut; } }
        public ColorEnum Color {
            get
            {
                return xeVisual.Color;
            }
        }
        public ColorEnum ActiveColor
        {
            get
            {
                for (int i = 0; i < hieuUng.Length; ++i)
                {
                    var hu = hieuUng[i];
                    if (hu != null && hu.gameObject.activeInHierarchy)
                    {
                        if (hu.MauXe() > ColorEnum.None)
                        {
                            return hu.MauXe();
                        }
                    }
                }

                return Color;
            }
        }

        bool HaveTang2()
        {
            for (int i = 0; i <= hieuUng.Length; i++)
            {
                var hu = hieuUng[i];
                if (hu != null && hu.extType == XeExtType.Tang2)
                {
                    return hu.IsActivating();
                }
            }
            return false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetColor(Color);

            UpdateLock(HaveTang2() ? -2.7f : -1.9f);
        }

        public bool OnMoveXe()
        {
            for (int i = 0; i < hieuUng.Length; ++i)
            {
                var hu = hieuUng[i];
                if (hu != null && hu.gameObject.activeInHierarchy)
                {
                    if (hu.OnMoveXe() == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void SetColor(ColorEnum c)
        {
            if (xeVisual == null) return;
            if (c <= ColorEnum.None) return;

            xeVisual.SetColor(c);
        }

        public bool PopMan(Man man, int c)
        {
            if (xeVisual.IsPopingMan) return false;

            if (xeVisual.IsFull == false)
            {
                for (int i = 0; i < hieuUng.Length; ++i)
                {
                    var hu = hieuUng[i];
                    if (hu != null && hu.gameObject.activeInHierarchy)
                    {
                        var re = hu.PopMan(man, c);
                        if (re > 0)
                        {
                            //Hiker.HikerLog.LogEditorOnly("Effect popup man 1 ");

                            return true;
                        }
                        else
                        if (re < 0)
                        {
                            //Hiker.HikerLog.LogEditorOnly("Effect popup man -1 ");

                            return false;
                        }
                        else
                        {
                            //Hiker.HikerLog.LogEditorOnly("Effect popup man skip " + name);
                        }
                    }
                }

                //Hiker.HikerLog.LogEditorOnly(string.Format("Poping {0} to {1}", man.Color, name), "Poping", "yellow");
                StartCoroutine(DoPopMan(man, c));

                LevelManager.instance?.OnPopManToXe(man);
                return true;
            }
            else
            if (IsGetOut == false)
            {
                OnXeOut();
            }

            return false;
        }

        IEnumerator DoPopMan(Man man, int c)
        {
            yield return xeVisual.StartCoroutine(xeVisual.DoPopMan(man, c));
            if (xeVisual.IsFull && IsGetOut == false)
            {
                OnXeOut();
            }
        }

        IEnumerator DoMoveOut()
        {
            mBody.bodyType = RigidbodyType2D.Kinematic;
            yield return xeVisual.StartCoroutine(xeVisual.DoMoveOut());
        }

        public void OnXeOut()
        {
            if (getOut) return;

            OnGoOutOfLevel();

            xeVisual.StartCoroutine(xeVisual.DoMoveOut());

            if (LevelManager.instance && LevelManager.instance.State == LevelStatus.Started)
            {
                LevelManager.instance.OnAnXe(this);
            }

            foreach (var t in hieuUng)
            {
                t.gameObject.SetActive(false);
            }
        }

        public void OnHaveXeGetOut(Xe veh)
        {
            for (int i = 0; i < hieuUng.Length; ++i)
            {
                var hu = hieuUng[i];
                if (hu != null && hu.gameObject.activeInHierarchy)
                {
                    hu.OnAnXe(veh);
                }
            }
        }

        public override void OnNhacXeLen()
        {
            if (isNhacLen) return;

            base.OnNhacXeLen();
            isNhacLen = true;

            for (int i = 0; i < hieuUng.Length; ++i)
            {
                if (hieuUng[i] != null && hieuUng[i].gameObject.activeInHierarchy)
                {
                    hieuUng[i].OnNhacXeLen();
                }
            }
        }

        public override void OnHaXeXuong()
        {
            if (isNhacLen == false) return;
            base.OnHaXeXuong();

            isNhacLen = false;

            for (int i = 0; i < hieuUng.Length; ++i)
            {
                if (hieuUng[i] != null && hieuUng[i].gameObject.activeInHierarchy)
                    hieuUng[i].OnHaXeXuong();
            }
        }

        public override void OnMoveDir(Vector2 d)
        {
            if (isNhacLen == false) return;

            base.OnMoveDir(d);

            for (int i = 0; i < hieuUng.Length; ++i)
            {
                if (hieuUng[i] != null && hieuUng[i].gameObject.activeInHierarchy)
                    hieuUng[i].OnMoveDir(d);
            }
        }

        public IEnumerator DoSawMulti(Vector3 touchPos)
        {
            var bua = LevelManager.instance.SpawnSawVisual(touchPos);
            yield return new WaitForSecondsRealtime(0.4f);
            SoundManager.instance?.PlaySound("SFX/BulletHitWall2");
            yield return new WaitForSecondsRealtime(0.35f);
            SoundManager.instance?.PlaySound("SFX/BulletHitWall2");
            yield return new WaitForSecondsRealtime(0.35f);
            SoundManager.instance?.PlaySound("SFX/BulletHitWall2");
            yield return new WaitForSecondsRealtime(0.5f);

            SoundManager.instance?.PlaySound("SFX/battle_pu_cua");
            gameObject.SetActive(false);

            yield return null;

            Destroy(bua.gameObject);

            //string replaceXe = XeNho;
            var listPassengers = Hiker.Util.ListPool<Man>.Claim();
            var listPassengers2 = Hiker.Util.ListPool<Man>.Claim();
            var listNewXe = Hiker.Util.ListPool<Xe>.Claim();
            var listNewT2 = Hiker.Util.ListPool<Tang2Eff>.Claim();

            var num = xeVisual.GetPassengers(listPassengers);
            //var pos = transform.position;

            //Tang2Eff newTang2 = null;
            //Xe newXe = null;
            //int newGhe = 0;

            //ColorManInQueue tang2RemoveManInqueue = new ColorManInQueue { Color = ColorEnum.None, Num = 0 };
            //ColorManInQueue removeManInQueue = new ColorManInQueue { Color = xeVisual.Color, Num = Mathf.Max(0, xeVisual.NumGhe - listPassengers.Count) };

            for (int i = 0; i < hieuUng.Length; ++i)
            {
                var h = hieuUng[i];
                if (h != null && h.gameObject.activeSelf)
                {
                    if (h.extType == XeExtType.Tang2)
                    {
                        var tang2 = h as Tang2Eff;
                        var num2 = tang2.visual.GetPassengers(listPassengers2);
                    }
                }
            }

            List<Vector2> listPosInGrid = Hiker.Util.ListPool<Vector2>.Claim();
            GetPosGridInBound(listPosInGrid);

            if (listPosInGrid.Count > 1)
            {
                var newXePrefab = Resources.Load<Xe>("Prefabs/Vehicles/Veh1I");

                if (newXePrefab != null)
                {
                    for (int i = 0; i < listPosInGrid.Count; ++i)
                    {
                        var p = listPosInGrid[i];

                        var newXe = Instantiate(newXePrefab, p, transform.rotation, transform.parent);
                        //newGhe = newXe.xeVisual.NumGhe;
                        newXe.SetColor(Color);
                        ObjectPoolManager.SpawnAutoDestroy("VFX/FX_SpawnXe", (Vector3)p + LevelManager.instance.levelCam.transform.forward * -2f, Quaternion.identity, 0.5f);
                        var halfGridSpace = LevelManager.instance.LevelBoard.GridSpace * 0.5f;

                        for (int t = 0; t < hieuUng.Length; ++t)
                        {
                            var h = hieuUng[t];
                            if (h != null && h.gameObject.activeSelf)
                            {
                                for (int j = 0; j < newXe.hieuUng.Length; ++j)
                                {
                                    var h2 = newXe.hieuUng[j];
                                    if (h2.extType == h.extType)
                                    {
                                        // chi copy full hieu ung cho xe dau tien, cac xe sau copy tat ca hieu ung tru Khoa
                                        if (i == 0 || h.extType != XeExtType.Khoa)
                                        {
                                            h2.gameObject.SetActive(true);
                                            h2.CloneState(h);

                                            if (h.extType == XeExtType.Tang2)
                                            {
                                                var tang2 = h as Tang2Eff;
                                                var newTang2 = h2 as Tang2Eff;
                                                var listT2 = Hiker.Util.ListPool<Man>.Claim();

                                                for (int k = listPassengers2.Count - 1; k >= 0; --k)
                                                {
                                                    var hanhKHach = listPassengers2[k];
                                                    Vector2 pPOs = hanhKHach.transform.position;

                                                    if (listT2.Count < 4 && Mathf.Abs(pPOs.x - p.x) <= halfGridSpace && Mathf.Abs(pPOs.y - p.y) <= halfGridSpace)
                                                    {
                                                        listT2.Add(hanhKHach);
                                                        listPassengers2.RemoveAt(k);
                                                    }
                                                }

                                                //var num2 = tang2.visual.GetPassengers(listPassengers2);
                                                if (listT2.Count > 0)
                                                {
                                                    newTang2.visual.FillPassengers(listT2);
                                                }

                                                listNewT2.Add(newTang2);
                                                //int removeMan = Mathf.Max(0, tang2.visual.NumGhe - newTang2.visual.NumGhe);
                                                //if (listPassengers2.Count > newTang2.visual.NumGhe)
                                                //{
                                                //    removeMan -= (listPassengers2.Count - newTang2.visual.NumGhe);
                                                //}

                                                //if (removeMan > 0)
                                                //{
                                                //    tang2RemoveManInqueue = new ColorManInQueue { Color = newTang2.visual.Color, Num = removeMan };
                                                //}

                                                Hiker.Util.ListPool<Man>.Release(listT2);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var listT = Hiker.Util.ListPool<Man>.Claim();
                        for (int k = listPassengers.Count - 1; k >= 0; --k)
                        {
                            var hanhKHach = listPassengers[k];
                            Vector2 pPOs = hanhKHach.transform.position;

                            if (listT.Count < 4 && Mathf.Abs(pPOs.x - p.x) <= halfGridSpace && Mathf.Abs(pPOs.y - p.y) <= halfGridSpace)
                            {
                                listT.Add(hanhKHach);
                                listPassengers.RemoveAt(k);
                            }
                        }
                        if (listT.Count > 0)
                        {
                            newXe.xeVisual.FillPassengers(listT);
                        }
                        Hiker.Util.ListPool<Man>.Release(listT);

                        listNewXe.Add(newXe);

                        //int numREmoveMan = Mathf.Max(0, xeVisual.NumGhe - newGhe);
                        //if (listPassengers.Count > newGhe)
                        //{
                        //    numREmoveMan -= (listPassengers.Count - newGhe);
                        //}

                        //if (numREmoveMan > 0)
                        //{
                        //    removeManInQueue = new ColorManInQueue { Color = newXe.xeVisual.Color, Num = numREmoveMan };
                        //}

                        //if (newXe.xeVisual.IsFull)
                        //{
                        //    newXe.OnXeOut();
                        //}
                    }
                }

                Hiker.Util.ListPool<Vector2>.Release(listPosInGrid);
            }
            else
            {
                Debug.LogError("Can not saw " + name);
                Hiker.Util.ListPool<Vector2>.Release(listPosInGrid);
                yield break;
            }

            ///
            /// neu con hanh khach chua fill het, chay lai mot luot de bao hiem
            ///
            for (int i = 0; i < listNewT2.Count; ++i)
            {
                var t2 = listNewT2[i];
                if (t2.IsActivating() && t2.IsGetOut == false && t2.visual.IsFull == false && listPassengers2.Count > 0)
                {
                    var listT = Hiker.Util.ListPool<Man>.Claim();
                    var num2 = listPassengers2.Count;
                    for (int j = 0; j < t2.visual.RemainGhe && num2 > 0; ++j)
                    {
                        listT.Add(listPassengers2[num2 - 1]);
                    }

                    t2.visual.FillPassengers(listT);
                    Hiker.Util.ListPool<Man>.Release(listT);
                }

                if (t2.IsActivating() && t2.IsGetOut == false && t2.visual.IsFull)
                {
                    t2.OnGetOut();
                }
            }

            for (int i = 0; i < listNewXe.Count; ++i)
            {
                var newX = listNewXe[i];
                if (newX.IsGetOut == false && newX.xeVisual.IsFull == false && listPassengers.Count > 0)
                {
                    var listT = Hiker.Util.ListPool<Man>.Claim();
                    var num2 = listPassengers.Count;
                    for (int j = 0; j < newX.xeVisual.RemainGhe && num2 > 0; ++j)
                    {
                        listT.Add(listPassengers[num2 - 1]);
                    }
                    if (listT.Count > 0)
                        newX.xeVisual.FillPassengers(listT);
                    Hiker.Util.ListPool<Man>.Release(listT);
                }

                if (newX.IsGetOut == false && newX.xeVisual.IsFull)
                {
                    newX.OnXeOut();
                }
            }

            Hiker.Util.ListPool<Man>.Release(listPassengers);
            Hiker.Util.ListPool<Man>.Release(listPassengers2);

            yield return null;

            Destroy(gameObject);

            yield return new WaitForSecondsRealtime(0.2f);
            for (int i = 0; i < listNewT2.Count; ++i)
            {
                var t2 = listNewT2[i];

                if (t2.IsActivating() && t2.IsGetOut == false && t2.visual.IsFull)
                {
                    t2.OnGetOut();

                    yield return new WaitForSecondsRealtime(0.2f);
                }
            }

            for (int i = 0; i < listNewXe.Count; ++i)
            {
                var newX = listNewXe[i];

                if (newX.IsGetOut == false && newX.xeVisual.IsFull)
                {
                    newX.OnXeOut();

                    yield return new WaitForSecondsRealtime(0.2f);
                }
            }


            Hiker.Util.ListPool<Xe>.Release(listNewXe);
            Hiker.Util.ListPool<Tang2Eff>.Release(listNewT2);
        }
        public IEnumerator DoShinkMinor()
        {
            //var bua = LevelManager.instance.SpawnBuaVisual(touchPos + Vector3.forward * -1f);
            //await UniTask.WaitForSeconds(0.6f, true);

            //gameObject.SetActive(false);

            //await UniTask.Yield();

            //Destroy(bua.gameObject);
            gameObject.SetActive(false);

            string replaceXe = XeNho;
            var listPassengers = Hiker.Util.ListPool<Man>.Claim();

            var num = xeVisual.GetPassengers(listPassengers);
            var pos = transform.position;

            Tang2Eff newTang2 = null;
            Xe newXe = null;
            int newGhe = 0;

            ColorManInQueue tang2RemoveManInqueue = new ColorManInQueue { Color = ColorEnum.None, Num = 0 };
            ColorManInQueue removeManInQueue = new ColorManInQueue { Color = xeVisual.Color, Num = Mathf.Max(0, xeVisual.NumGhe - listPassengers.Count) };

            for (int i = 0; i < hieuUng.Length; ++i)
            {
                var h = hieuUng[i];
                if (h != null && h.gameObject.activeSelf)
                {
                    if (h.extType == XeExtType.Tang2)
                    {
                        var tang2 = h as Tang2Eff;
                        var listPassengers2 = Hiker.Util.ListPool<Man>.Claim();
                        var num2 = tang2.visual.GetPassengers(listPassengers2);
                        
                        int removeMan = tang2.visual.NumGhe;
                        if (listPassengers2.Count > 0)
                        {
                            removeMan -= listPassengers2.Count;
                        }

                        if (removeMan > 0)
                        {
                            tang2RemoveManInqueue = new ColorManInQueue { Color = tang2.visual.Color, Num = removeMan };
                        }
                        Hiker.Util.ListPool<Man>.Release(listPassengers2);
                    }
                }
            }

            if (string.IsNullOrEmpty(replaceXe) == false)
            {
                var newXePrefab = Resources.Load<Xe>("Prefabs/Vehicles/" + replaceXe);
                
                if (newXePrefab != null)
                {
                    newXe = Instantiate(newXePrefab, transform.position, transform.rotation, transform.parent);
                    newGhe = newXe.xeVisual.NumGhe;
                    newXe.SetColor(Color);

                    for (int i = 0; i < hieuUng.Length; ++i)
                    {
                        var h = hieuUng[i];
                        if (h != null && h.gameObject.activeSelf)
                        {
                            for (int j = 0; j < newXe.hieuUng.Length; ++j)
                            {
                                var h2 = newXe.hieuUng[j];
                                if (h2.extType == h.extType)
                                {
                                    h2.gameObject.SetActive(true);
                                    h2.CloneState(h);

                                    if (h.extType == XeExtType.Tang2)
                                    {
                                        var tang2 = h as Tang2Eff;
                                        var listPassengers2 = Hiker.Util.ListPool<Man>.Claim();
                                        var num2 = tang2.visual.GetPassengers(listPassengers2);

                                        newTang2 = h2 as Tang2Eff;
                                        newTang2.visual.FillPassengers(listPassengers2);

                                        int removeMan = Mathf.Max(0, tang2.visual.NumGhe - newTang2.visual.NumGhe);
                                        if (listPassengers2.Count > newTang2.visual.NumGhe)
                                        {
                                            removeMan -= (listPassengers2.Count - newTang2.visual.NumGhe);
                                        }

                                        if (removeMan > 0)
                                        {
                                            tang2RemoveManInqueue = new ColorManInQueue { Color = newTang2.visual.Color, Num = removeMan };
                                        }

                                        Hiker.Util.ListPool<Man>.Release(listPassengers2);
                                    }
                                }
                            }
                        }
                    }

                    newXe.xeVisual.FillPassengers(listPassengers);

                    int numREmoveMan = Mathf.Max(0, xeVisual.NumGhe - newGhe);
                    if (listPassengers.Count > newGhe)
                    {
                        numREmoveMan -= (listPassengers.Count - newGhe);
                    }

                    if (numREmoveMan > 0)
                    {
                        removeManInQueue = new ColorManInQueue { Color = newXe.xeVisual.Color, Num = numREmoveMan };
                    }

                    if (newXe.xeVisual.IsFull)
                    {
                        newXe.OnXeOut();
                    }
                }
            }

            Hiker.Util.ListPool<Man>.Release(listPassengers);

            yield return null;

            listPassengers = Hiker.Util.ListPool<Man>.Claim();

            if (tang2RemoveManInqueue.Color > ColorEnum.None && tang2RemoveManInqueue.Num > 0 && LevelManager.instance != null)
            {
                LevelManager.instance.RemoveManFromQueue(tang2RemoveManInqueue.Color, tang2RemoveManInqueue.Num, listPassengers);
            }
            if (removeManInQueue.Color > ColorEnum.None && removeManInQueue.Num > 0 && LevelManager.instance != null)
            {
                LevelManager.instance.RemoveManFromQueue(removeManInQueue.Color, removeManInQueue.Num, listPassengers);
            }

            if (newTang2 != null && newTang2.gameObject.activeSelf && newTang2.visual.IsFull)
            {
                newTang2.visual.StartCoroutine(newTang2.visual.DoMoveOut());
                yield return new WaitForSecondsRealtime(0.2f);
                newTang2.gameObject.SetActive(false);
            }

            foreach (var t in listPassengers)
            {
                LevelManager.instance.OnPopManToXe(t);

                Destroy(t.gameObject);
                yield return null;
            }

            Hiker.Util.ListPool<Man>.Release(listPassengers);

            getOut = true;

            if (newXe == null && LevelManager.instance)
            {
                LevelManager.instance.OnAnXe(this);
            }
            Destroy(gameObject);

            if (LevelManager.instance)
            {
                LevelManager.instance.UpdateManInQueues();
            }
        }
        public IEnumerator DoMagnetUse()
        {
            var pos = transform.position;
            var namchamObj = LevelManager.instance.SpawnMagnetVisual(pos + Vector3.forward * -6f);
            yield return new WaitForSecondsRealtime(.3f);

            transform.SetLayer(LayerMan.NonCollideLayer, true);
            visual.transform.DOMoveZ(-1f, 0.35f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(.35f);

            int trigPop = 0;

            var listPassengers = Hiker.Util.ListPool<Man>.Claim();
            for (int i = 0; i < hieuUng.Length; ++i)
            {
                var h = hieuUng[i];
                if (h != null && h.gameObject.activeSelf)
                {
                    if (h.extType == XeExtType.Tang2)
                    {
                        var tang2 = h as Tang2Eff;
                        var num2 = tang2.visual.RemainGhe;

                        if (num2 > 0)
                        {
                            LevelManager.instance.RemoveManFromQueue(tang2.visual.Color, num2, listPassengers);
                        }
                    }
                    else
                    if (h.extType == XeExtType.Xich)
                    {
                        h.gameObject.SetActive(false);
                    }
                }
            }

            float timeBreak = 0.1f;
            if (listPassengers.Count > 0)
            {
                for (int i = 0; i < listPassengers.Count; ++i)
                {
                    var m = listPassengers[i];
                    bool popup = PopMan(m, trigPop);
                    while (popup == false)
                    {
                        yield return null;
                        popup = PopMan(m, trigPop);
                    }
                    trigPop++;
                    yield return new WaitForSecondsRealtime(timeBreak);
                    timeBreak *= 0.9f;
                    if (timeBreak < 0.02f)
                    {
                        timeBreak = 0.02f;
                    }
                }

                if (LevelManager.instance)
                {
                    LevelManager.instance.UpdateManInQueues();
                }
            }
            //await UniTask.Yield();

            listPassengers.Clear();

            var num = xeVisual.RemainGhe;

            LevelManager.instance.RemoveManFromQueue(xeVisual.Color, num, listPassengers);

            //foreach (var m in listPassengers)
            timeBreak = 0.1f;
            for (int i = 0; i < listPassengers.Count; ++i)
            {
                if (i > listPassengers.Count - 4)
                {
                    if (namchamObj != null)
                    {
                        namchamObj.SetTrigger(AnimatorHash.getOutTrigger);
                        Destroy(namchamObj.gameObject, 0.3f);
                        namchamObj = null;
                    }
                }

                var m = listPassengers[i];
                bool popup = PopMan(m, trigPop);
                while (popup == false)
                {
                    yield return null;
                    popup = PopMan(m, trigPop);
                }
                trigPop++;
                if (namchamObj != null && i >= listPassengers.Count - 2)
                {
                    namchamObj.SetTrigger(AnimatorHash.getOutTrigger);
                    Destroy(namchamObj.gameObject, 0.3f);
                    namchamObj = null;
                }
                yield return new WaitForSecondsRealtime(timeBreak);
                timeBreak *= 0.9f;
                if (timeBreak < 0.02f)
                {
                    timeBreak = 0.02f;
                }
            }
            if (listPassengers.Count == 0)
            {
                if (namchamObj != null)
                {
                    namchamObj.SetTrigger(AnimatorHash.getOutTrigger);
                    Destroy(namchamObj.gameObject, 0.3f);
                    namchamObj = null;
                }
            }
            Hiker.Util.ListPool<Man>.Release(listPassengers);

            if (LevelManager.instance)
            {
                LevelManager.instance.UpdateManInQueues();
            }
        }
    }
}

