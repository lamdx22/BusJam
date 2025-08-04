using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class HikerLoopImage : MaskableGraphic, ILayoutElement
    , ICanvasRaycastFilter
{
    [SerializeField]
    private Sprite m_Sprite;

    [SerializeField]
    private Vector2 m_Spacing = Vector2.zero;

    RectTransform rectTran;
    private float m_CachedReferencePixelsPerUnit = 100f;

    protected static Material s_ETC1DefaultUI = null;
    public static Material defaultETC1GraphicMaterial
    {
        get
        {
            if (s_ETC1DefaultUI == null)
            {
                s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
            }

            return s_ETC1DefaultUI;
        }
    }

    public override Texture mainTexture
    {
        get
        {
            if (m_Sprite == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }

                return Graphic.s_WhiteTexture;
            }

            return m_Sprite.texture;
            //return Graphic.s_WhiteTexture;
        }
    }

    public float pixelsPerUnit
    {
        get
        {
            float num = 100f;
            if (m_Sprite != null)
            {
                num = m_Sprite.pixelsPerUnit;
            }

            if (base.canvas != null)
            {
                m_CachedReferencePixelsPerUnit = base.canvas.referencePixelsPerUnit;
            }

            return num / m_CachedReferencePixelsPerUnit;
        }
    }
    public float minWidth => 0f;

    public float preferredWidth {
        get
        {
            if  (m_Sprite == null)
            {
                return 0f;
            }

            return m_Sprite.rect.size.x / pixelsPerUnit;
        }
    }

    public float flexibleWidth => -1f;

    public float minHeight => 0f;

    public float preferredHeight
    {
        get
        {
            if (m_Sprite == null)
            {
                return 0f;
            }

            return m_Sprite.rect.size.y / pixelsPerUnit;
        }
    }
    public float flexibleHeight => -1f;

    public int layoutPriority => 0;

    public void CalculateLayoutInputHorizontal()
    {

    }

    public void CalculateLayoutInputVertical()
    {

    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // not support for now (use only for decor)
        return false;
        //if (alphaHitTestMinimumThreshold <= 0f)
        //{
        //    return true;
        //}

        //if (alphaHitTestMinimumThreshold > 1f)
        //{
        //    return false;
        //}

        //if (m_Sprite == null)
        //{
        //    return true;
        //}

        //if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out var localPoint))
        //{
        //    return false;
        //}

        //Rect rect = GetPixelAdjustedRect();

        //localPoint.x += base.rectTransform.pivot.x * rect.width;
        //localPoint.y += base.rectTransform.pivot.y * rect.height;
        //localPoint = MapCoordinate(localPoint, rect);
        //float u = localPoint.x / (float)activeSprite.texture.width;
        //float v = localPoint.y / (float)activeSprite.texture.height;
        //try
        //{
        //    return activeSprite.texture.GetPixelBilinear(u, v).a >= alphaHitTestMinimumThreshold;
        //}
        //catch (UnityException ex)
        //{
        //    Debug.LogError("Using alphaHitTestMinimumThreshold greater than 0 on Image whose sprite texture cannot be read. " + ex.Message + " Also make sure to disable sprite packing for this sprite.", this);
        //    return true;
        //}
    }

    protected override void OnEnable()
    {
        if (rectTran == null)
        {
            rectTran = GetComponent<RectTransform>();
        }
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (m_Sprite == null)
        {
            base.OnPopulateMesh(vh);
            return;
        }

        //var listVertices = Hiker.Util.ListPool<UIVertex>.Claim();
        //vh.GetUIVertexStream(listVertices);

        //vh.Clear();
        //vh.AddUIVertexTriangleStream(listVertices);

        //Hiker.Util.ListPool<UIVertex>.Release(listVertices);
        GenerateSimpleSprite(vh);

    }

    private void GenerateSimpleSprite(VertexHelper vh)
    {
        Vector4 vPad = ((m_Sprite == null) ? Vector4.zero : DataUtility.GetPadding(m_Sprite));
        Vector2 spriteSize = ((m_Sprite == null) ? Vector2.zero : new Vector2(m_Sprite.rect.width, m_Sprite.rect.height));
        Rect rect = GetPixelAdjustedRect();
        int num = Mathf.RoundToInt(spriteSize.x);
        int num2 = Mathf.RoundToInt(spriteSize.y);
        Vector4 vPad2 = new Vector4(vPad.x / (float)num, vPad.y / (float)num2, ((float)num - vPad.z) / (float)num, ((float)num2 - vPad.w) / (float)num2);

        Vector4 drawingDimensions = new Vector4(rect.x + rect.width * vPad2.x, rect.y + rect.height * vPad2.y, rect.x + rect.width * vPad2.z, rect.y + rect.height * vPad2.w);

        Vector4 vector = ((m_Sprite != null) ? DataUtility.GetOuterUV(m_Sprite) : Vector4.zero);
        Color color = this.color;

        float w = drawingDimensions.z - drawingDimensions.x;
        float h = drawingDimensions.w - drawingDimensions.y;

        float cX = num - vPad.x - vPad.z;
        float cY = num2 - vPad.y - vPad.w;

        int x = Mathf.CeilToInt(w / (cX + m_Spacing.x));
        int y = Mathf.CeilToInt(h / (cY + m_Spacing.y));

        float w2 = x * cX + (x - 1) * m_Spacing.x;
        float h2 = y * cY + (y - 1) * m_Spacing.y;

        if (w2 > w)
        {
            x--;
        }

        if (h2 > h)
        {
            y--;
        }

        w2 = x * cX + (x - 1) * m_Spacing.x;
        h2 = y * cY + (y - 1) * m_Spacing.y;

        vh.Clear();

        int c = 0;
        for (int i = 0; i < x; ++i)
        {
            for (int j = 0; j < y; ++j)
            {
                var r = new Vector4(
                    i * (cX + m_Spacing.x) - w2 * rectTran.pivot.x,
                    j * (cY + m_Spacing.y) - h2 * rectTran.pivot.y,
                    i * (cX + m_Spacing.x) - w2 * rectTran.pivot.x + cX,
                    j * (cY + m_Spacing.y) - h2 * rectTran.pivot.y + cY);

                vh.AddVert(new Vector3(r.x, r.y), color, new Vector2(vector.x, vector.y));
                vh.AddVert(new Vector3(r.x, r.w), color, new Vector2(vector.x, vector.w));
                vh.AddVert(new Vector3(r.z, r.w), color, new Vector2(vector.z, vector.w));
                vh.AddVert(new Vector3(r.z, r.y), color, new Vector2(vector.z, vector.y));

                vh.AddTriangle(c + 0, c + 1, c + 2);
                vh.AddTriangle(c + 2, c + 3, c + 0);
                c += 4;
            }
        }
    }

    //private Vector4 GetDrawingDimensions()
    //{
    //    Vector4 vector = ((m_Sprite == null) ? Vector4.zero : DataUtility.GetPadding(m_Sprite));
    //    Vector2 spriteSize = ((m_Sprite == null) ? Vector2.zero : new Vector2(m_Sprite.rect.width, m_Sprite.rect.height));
    //    Rect rect = GetPixelAdjustedRect();
    //    int num = Mathf.RoundToInt(spriteSize.x);
    //    int num2 = Mathf.RoundToInt(spriteSize.y);
    //    Vector4 vector2 = new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2);
    //    //if (spriteSize.sqrMagnitude > 0f)
    //    //{
    //    //    PreserveSpriteAspectRatio(ref rect, spriteSize);
    //    //}

    //    return new Vector4(rect.x + rect.width * vector2.x, rect.y + rect.height * vector2.y, rect.x + rect.width * vector2.z, rect.y + rect.height * vector2.w);
    //}

    //private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
    //{
    //    float num = spriteSize.x / spriteSize.y;
    //    float num2 = rect.width / rect.height;
    //    if (num > num2)
    //    {
    //        float height = rect.height;
    //        rect.height = rect.width * (1f / num);
    //        rect.y += (height - rect.height) * base.rectTransform.pivot.y;
    //    }
    //    else
    //    {
    //        float width = rect.width;
    //        rect.width = rect.height * num;
    //        rect.x += (width - rect.width) * base.rectTransform.pivot.x;
    //    }
    //}
}
