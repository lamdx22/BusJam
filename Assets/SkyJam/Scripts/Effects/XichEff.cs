using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkyJam
{
    public class XichEff : XeEff
    {
        public Xe otherXe;

        [SerializeField]
        GameObject visualXich;
        FixedJoint2D fixedJoint = null;

        public bool IsMasterVisual
        {
            get
            {
                if (otherXe == null) return false;
                if (otherXe.transform.GetSiblingIndex() < myXe.transform.GetSiblingIndex())
                    return false;
                return true;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            extType = XeExtType.Xich;
        }

        public Vector2Int GetXichDirection(out Vector2 centerPos)
        {
            centerPos = Vector2.zero;
            if (myXe == null || otherXe == null) return Vector2Int.zero;
            if (LevelManager.instance == null) return Vector2Int.zero;
            var levelBoard = LevelManager.instance.LevelBoard;
            if (levelBoard == null)
            {
                levelBoard = GetComponentInParent<BoardController>();
            }
            if (levelBoard == null) return Vector2Int.zero;

            List<Vector2Int> listPosInGrid = Hiker.Util.ListPool<Vector2Int>.Claim();
            myXe.GetPosGridInBound(listPosInGrid);

            List<Vector2Int> listPosInGrid2 = Hiker.Util.ListPool<Vector2Int>.Claim();
            otherXe.GetPosGridInBound(listPosInGrid2);

            int h = 0;
            int v = 0;
            for (int i = 0; i < listPosInGrid.Count; ++i)
            {
                var p1 = listPosInGrid[i];
                for (int j = 0; j < listPosInGrid2.Count; ++j)
                {
                    var p2 = listPosInGrid2[j];
                    var d = p1 - p2;
                    if (d.x == 0 && Mathf.Abs(d.y) == 1)
                    {
                        v++;
                        centerPos = (levelBoard.GetWorldPosByGridPos(p1.x, p1.y) + levelBoard.GetWorldPosByGridPos(p2.x, p2.y)) * 0.5f;
                        break;
                    }
                    else
                    if (d.y == 0 && Mathf.Abs(d.x) == 1)
                    {
                        h++;
                        centerPos = (levelBoard.GetWorldPosByGridPos(p1.x, p1.y) + levelBoard.GetWorldPosByGridPos(p2.x, p2.y)) * 0.5f;
                        break;
                    }
                }
            }

            Hiker.Util.ListPool<Vector2Int>.Release(listPosInGrid);
            Hiker.Util.ListPool<Vector2Int>.Release(listPosInGrid2);

            if (h > 0 && h > v)
            {
                return Vector2Int.right;
            }
            else if (v > 0 && v > h)
            {
                return Vector2Int.up;
            }
            else
            {
                return Vector2Int.zero;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (otherXe != null)
            {
                if (otherXe.IsGetOut == false)
                {
                    for (int i = 0; i < otherXe.hieuUng.Length; ++i)
                    {
                        if (otherXe.hieuUng[i].extType == XeExtType.Xich)
                        {
                            if (otherXe.hieuUng[i].gameObject.activeSelf == false)
                                otherXe.hieuUng[i].gameObject.SetActive(true);

                            var xichEff = otherXe.hieuUng[i] as XichEff;
                            if (xichEff != null && xichEff.otherXe != myXe)
                            {
                                xichEff.otherXe = myXe;
                            }
                        }
                    }

                    if (IsMasterVisual)
                    {
                        var d = GetXichDirection(out Vector2 centerPos);
                        GameObject prefab = null;
                        if (d.x > 0) // chain dir in horizon so visual chain in vertical
                        {
                            prefab = Resources.Load<GameObject>("Prefabs/Chain1V");
                            //Hiker.HikerLog.LogDebugOnly("Horizon chain");
                        }
                        else
                        if (d.y > 0)
                        {
                            prefab = Resources.Load<GameObject>("Prefabs/Chain1H");
                            //Hiker.HikerLog.LogDebugOnly("Vertical chain");
                        }

                        if (visualXich != null && (prefab == null || visualXich.name.StartsWith(prefab.name) == false))
                        {
                            GameObject.DestroyImmediate(visualXich.gameObject);
                            visualXich = null;
                        }

                        if (visualXich == null && prefab != null)
                        {
//#if UNITY_EDITOR
//                            if (Application.isPlaying == false ||
//                                LevelDesignUI.instance != null && LevelDesignUI.instance.editMode.isOn)
//                            {
//                                visualXich = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
//                                visualXich.name = prefab.name;
//                                visualXich.transform.position = centerPos;
//                                visualXich.transform.rotation = Quaternion.identity;
//                            }
//                            else
//#endif
                            {
                                visualXich = Instantiate(prefab, transform);
                                visualXich.name = prefab.name;
                                visualXich.transform.position = centerPos;
                                visualXich.transform.rotation = Quaternion.identity;
                            }
                        }
                        //else if (visualXich != null)
                        //{
                        //    visualXich.transform.position = centerPos;
                        //    visualXich.transform.rotation = Quaternion.identity;
                        //}
                    }
                    else
                    {
                        if (visualXich != null)
                        {
                            visualXich.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (otherXe != null && otherXe.IsGetOut == false)
            {
                for (int i = 0; i < otherXe.hieuUng.Length; ++i)
                {
                    if (otherXe.hieuUng[i].extType == XeExtType.Xich)
                    {
                        if (otherXe.hieuUng[i].gameObject.activeSelf)
                            otherXe.hieuUng[i].gameObject.SetActive(false);
                    }
                }
            }

            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
                fixedJoint = null;
            }
        }

        protected override void Update()
        {
            base.Update();

//#if UNITY_EDITOR
            
//#endif
        }

        public override void OnNhacXeLen()
        {
            base.OnNhacXeLen();

            fixedJoint = myXe.gameObject.AddComponent<FixedJoint2D>();
            fixedJoint.connectedBody = otherXe.GetComponent<Rigidbody2D>();

            if (otherXe != null && otherXe.IsGetOut == false)
            {
                otherXe.OnNhacXeLen();
            }
        }
        public override void OnHaXeXuong()
        {
            base.OnNhacXeLen();

            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
                fixedJoint = null;
                otherXe.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }

            if (otherXe != null && otherXe.IsGetOut == false)
            {
                otherXe.OnHaXeXuong();
                otherXe.DoMovePos(LevelManager.instance.LevelBoard.SnapPositionToGrid(otherXe.BodyPos), 0.15f);
            }
        }

        public override void CloneState(XeEff clone)
        {
            base.CloneState(clone);

            if (clone.extType == XeExtType.Xich)
            {
                var eff = clone as XichEff;
                if (eff != null)
                {
                    otherXe = eff.otherXe;
                }
            }
        }
    }
}