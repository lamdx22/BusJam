using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KhoaObjVisual : MonoBehaviour
{
    public Transform tamKhoa;
    [SerializeField] MeshRenderer[] renderers;

    static CauHinhGame cfg = null;

    public void SetColor(ColorEnum c)
    {
        if (c <= ColorEnum.None)
        {
            return;
        }

        if (cfg == null)
        {
            cfg = Resources.Load<CauHinhGame>("GameConfig");
        }

        foreach (var r in renderers)
        {
            r.sharedMaterial = cfg.Khoa[(int)c - 1];
        }
    }
}
