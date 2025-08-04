using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HikerPerspectiveImage : BaseMeshEffect
{
    [Range(0, 1f)]
    [SerializeField]
    float displacement = 0.2f;

    RectTransform rectTran;
    Image image;

    protected override void OnEnable()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (rectTran == null)
        {
            rectTran = GetComponent<RectTransform>();
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        var listVertices = Hiker.Util.ListPool<UIVertex>.Claim();
        vh.GetUIVertexStream(listVertices);

        var size = rectTran.rect.size;
        var min = rectTran.rect.min;
        var max = rectTran.rect.max;

        var d = displacement * size.x * 0.5f;

        for (int i = 0; i < listVertices.Count; i++)
        {
            var v = listVertices[i];

            Vector3 t = v.position;
            var tl = Mathf.Clamp01((t.y - min.y) / size.y);
            var l = Mathf.Lerp(0, d, tl);
            var th = Mathf.Clamp01((t.x - min.x) / size.x);
            t.x = Mathf.Lerp(min.x + l, max.x - l, th);

            v.position = t;

            //Debug.LogFormat("V{0}: {1} -> {2}", i, listVertices[i].position, v.position);

            listVertices[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(listVertices);

        Hiker.Util.ListPool<UIVertex>.Release(listVertices);
    }
}
