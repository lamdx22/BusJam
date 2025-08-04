using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkyJam
{
    public class XeEff : MonoBehaviour
    {
        public XeExtType extType = XeExtType.None;
        public int[] Params;

        protected Xe myXe;

        protected virtual void Awake()
        {

        }
        protected virtual void OnEnable()
        {
            myXe = GetComponentInParent<Xe>();
        }
        protected virtual void OnDisable()
        {

        }
        protected virtual void Start()
        {

        }
        protected virtual void Update()
        {

        }

        public virtual void CloneState(XeEff clone)
        {
            if (clone != null && clone.extType == extType)
            {
                if (clone.Params != null && clone.Params.Length > 0)
                {
                    System.Array.Copy(clone.Params, Params, Params.Length);
                }
                else
                {
                    Params = null;
                }
            }
        }

        public virtual void OnAnXe(Xe veh)
        {
            
        }

        public virtual bool OnMoveXe()
        {
            return true;
        }
        public virtual void OnNhacXeLen()
        {
        }
        public virtual void OnHaXeXuong()
        {
        }

        public virtual void OnMoveDir(Vector2 d)
        {

        }

        public virtual ColorEnum MauXe()
        {
            return ColorEnum.None;
        }

        /// <summary>
        /// return 1 neu effect da thuc hien an linh
        /// return 0 neu effect khong thuc hien an linh
        /// return -1 neu effect ngan can thuc hien an linh
        /// </summary>
        /// <param name="man"></param>
        /// <returns></returns>
        public virtual int PopMan(Man man, int c)
        {
            return 0;
        }

        public virtual bool IsActivating()
        {
            return gameObject.activeSelf;
        }
    }

    public enum XeExtType
    {
        None = 0,
        Bang,
        Tang2,
        Khoa,
        Xich
    }
}