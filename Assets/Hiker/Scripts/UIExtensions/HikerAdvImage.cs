using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using Sprites = UnityEngine.Sprites;
using UnityEngine.U2D;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class HikerAdvImage : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
{
    [SerializeField]
    private Sprite m_Sprite;

    public Sprite sprite
    {
        get { return m_Sprite; }
        set
        {
            if (m_Sprite != null)
            {
                if (m_Sprite != value)
                {
                    m_SkipLayoutUpdate = m_Sprite.rect.size.Equals(value ? value.rect.size : Vector2.zero);
                    m_SkipMaterialUpdate = m_Sprite.texture == (value ? value.texture : null);
                    m_Sprite = value;

                    ResetAlphaHitThresholdIfNeeded();
                    SetAllDirty();
                    TrackSprite();
                }
            }
            else if (value != null)
            {
                m_SkipLayoutUpdate = value.rect.size == Vector2.zero;
                m_SkipMaterialUpdate = value.texture == null;
                m_Sprite = value;

                ResetAlphaHitThresholdIfNeeded();
                SetAllDirty();
                TrackSprite();
            }

            void ResetAlphaHitThresholdIfNeeded()
            {
                if (!SpriteSupportsAlphaHitTest() && m_AlphaHitTestMinimumThreshold > 0)
                {
                    Debug.LogWarning("Sprite was changed for one not readable or with Crunch Compression. Resetting the AlphaHitThreshold to 0.", this);
                    m_AlphaHitTestMinimumThreshold = 0;
                }
            }

            bool SpriteSupportsAlphaHitTest()
            {
                return m_Sprite != null && m_Sprite.texture != null && !GraphicsFormatUtility.IsCrunchFormat(m_Sprite.texture.format) && m_Sprite.texture.isReadable;
            }
        }
    }


    /// <summary>
    /// Disable all automatic sprite optimizations.
    /// </summary>
    /// <remarks>
    /// When a new Sprite is assigned update optimizations are automatically applied.
    /// </remarks>

    public void DisableSpriteOptimizations()
    {
        m_SkipLayoutUpdate = false;
        m_SkipMaterialUpdate = false;
    }

    // Not serialized until we support read-enabled sprites better.
    private float m_AlphaHitTestMinimumThreshold = 0;

    // Whether this is being tracked for Atlas Binding.
    private bool m_Tracked = false;
    public float alphaHitTestMinimumThreshold
    {
        get { return m_AlphaHitTestMinimumThreshold; }
        set
        {
            if (sprite != null && (GraphicsFormatUtility.IsCrunchFormat(sprite.texture.format) || !sprite.texture.isReadable))
                throw new System.InvalidOperationException("alphaHitTestMinimumThreshold should not be modified on a texture not readeable or not using Crunch Compression.");

            m_AlphaHitTestMinimumThreshold = value;
        }
    }

    protected HikerAdvImage()
    {
        useLegacyMeshGeneration = false;
    }
    /// <summary>
    /// Image's texture comes from the UnityEngine.Image.
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            if (sprite == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return sprite.texture;
        }
    }

    /// <summary>
    /// Whether the Sprite of the image has a border to work with.
    /// </summary>

    public bool hasBorder
    {
        get
        {
            if (sprite != null)
            {
                Vector4 v = sprite.border;
                return v.sqrMagnitude > 0f;
            }
            return false;
        }
    }


    [SerializeField]
    private float m_PixelsPerUnitMultiplier = 1.0f;

    /// <summary>
    /// Pixel per unit modifier to change how sliced sprites are generated.
    /// </summary>
    public float pixelsPerUnitMultiplier
    {
        get { return m_PixelsPerUnitMultiplier; }
        set
        {
            m_PixelsPerUnitMultiplier = Mathf.Max(0.01f, value);
            SetVerticesDirty();
        }
    }

    // case 1066689 cache referencePixelsPerUnit when canvas parent is disabled;
    private float m_CachedReferencePixelsPerUnit = 100;

    public float pixelsPerUnit
    {
        get
        {
            float spritePixelsPerUnit = 100;
            if (sprite)
                spritePixelsPerUnit = sprite.pixelsPerUnit;

            if (canvas)
                m_CachedReferencePixelsPerUnit = canvas.referencePixelsPerUnit;

            return spritePixelsPerUnit / m_CachedReferencePixelsPerUnit;
        }
    }

    protected float multipliedPixelsPerUnit
    {
        get { return pixelsPerUnit * m_PixelsPerUnitMultiplier; }
    }


    /// <summary>
    /// The specified Material used by this Image. The default Material is used instead if one wasn't specified.
    /// </summary>
    public override Material material
    {
        get
        {
            if (m_Material != null)
                return m_Material;

            //Edit and Runtime should use Split Alpha Shader if EditorSettings.spritePackerMode = Sprite Atlas V2
#if UNITY_EDITOR
            if ((Application.isPlaying || EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2) &&
                sprite && sprite.associatedAlphaSplitTexture != null)
            {
                return Image.defaultETC1GraphicMaterial;
            }
#else

                if (sprite && sprite.associatedAlphaSplitTexture != null)
                    //return Image.defaultETC1GraphicMaterial;
                    return new Material(Shader.Find("UI/ETC1 Supported"));
#endif

            return defaultMaterial;
        }

        set
        {
            base.material = value;
        }
    }
    private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
    {
        var spriteRatio = spriteSize.x / spriteSize.y;
        var rectRatio = rect.width / rect.height;

        if (spriteRatio > rectRatio)
        {
            var oldHeight = rect.height;
            rect.height = rect.width * (1.0f / spriteRatio);
            rect.y += (oldHeight - rect.height) * rectTransform.pivot.y;
        }
        else
        {
            var oldWidth = rect.width;
            rect.width = rect.height * spriteRatio;
            rect.x += (oldWidth - rect.width) * rectTransform.pivot.x;
        }
    }

    /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
    private Vector4 GetDrawingDimensions()
    {
        var padding = sprite == null ? Vector4.zero : Sprites.DataUtility.GetPadding(sprite);
        var size = sprite == null ? Vector2.zero : new Vector2(sprite.rect.width, sprite.rect.height);

        Rect r = GetPixelAdjustedRect();
        // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

        int spriteW = Mathf.RoundToInt(size.x);
        int spriteH = Mathf.RoundToInt(size.y);

        var v = new Vector4(
            padding.x / spriteW,
            padding.y / spriteH,
            (spriteW - padding.z) / spriteW,
            (spriteH - padding.w) / spriteH);

        if (size.sqrMagnitude > 0.0f)
        {
            PreserveSpriteAspectRatio(ref r, size);
        }

        v = new Vector4(
            r.x + r.width * v.x,
            r.y + r.height * v.y,
            r.x + r.width * v.z,
            r.y + r.height * v.w
        );

        return v;
    }

    /// <summary>
    /// Adjusts the image size to make it pixel-perfect.
    /// </summary>
    /// <remarks>
    /// This means setting the Images RectTransform.sizeDelta to be equal to the Sprite dimensions.
    /// </remarks>
    public override void SetNativeSize()
    {
        if (sprite != null)
        {
            float w = sprite.rect.width / pixelsPerUnit;
            float h = sprite.rect.height / pixelsPerUnit;
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.sizeDelta = new Vector2(w, h);
            SetAllDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (sprite == null)
        {
            base.OnPopulateMesh(toFill);
            return;
        }

        GenerateSlicedSprite(toFill);
    }
    private void TrackSprite()
    {
        if (sprite != null && sprite.texture == null)
        {
            TrackImage(this);
            m_Tracked = true;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        TrackSprite();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (m_Tracked)
            UnTrackImage(this);
    }
    // <summary>
    /// Update the renderer's material.
    /// </summary>

    protected override void UpdateMaterial()
    {
        base.UpdateMaterial();

        // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)

        if (sprite == null)
        {
            //canvasRenderer.SetAlphaTexture(null);
            return;
        }

        Texture2D alphaTex = sprite.associatedAlphaSplitTexture;

        if (alphaTex != null)
        {
            //canvasRenderer.SetAlphaTexture(alphaTex);
        }
    }

    protected override void OnCanvasHierarchyChanged()
    {
        base.OnCanvasHierarchyChanged();
        if (canvas == null)
        {
            m_CachedReferencePixelsPerUnit = 100;
        }
        else if (canvas.referencePixelsPerUnit != m_CachedReferencePixelsPerUnit)
        {
            m_CachedReferencePixelsPerUnit = canvas.referencePixelsPerUnit;

            SetVerticesDirty();
            SetLayoutDirty();
        }
    }

    /// <summary>
    /// Generate vertices for a simple Image.
    /// </summary>
    void GenerateSimpleSprite(VertexHelper vh)
    {
        Vector4 v = GetDrawingDimensions();
        var uv = (sprite != null) ? Sprites.DataUtility.GetOuterUV(sprite) : Vector4.zero;

        var color32 = color;
        vh.Clear();
        vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
        vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
        vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
        vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    static readonly Vector2[] s_VertScratch = new Vector2[4];
    static readonly Vector2[] s_UVScratch = new Vector2[4];
    /// <summary>
    /// Generate vertices for a 9-sliced Image.
    /// </summary>
    private void GenerateSlicedSprite(VertexHelper toFill)
    {
        if (!hasBorder)
        {
            GenerateSimpleSprite(toFill);
            return;
        }

        Vector4 outer, inner, padding, border;
        Vector2 spriteSize;
        Vector2 borderSize;

        if (sprite != null)
        {
            outer = Sprites.DataUtility.GetOuterUV(sprite);
            inner = Sprites.DataUtility.GetInnerUV(sprite);
            padding = Sprites.DataUtility.GetPadding(sprite);
            border = sprite.border;
            spriteSize = sprite.rect.size;
            borderSize = new Vector2(border.x + border.z, border.y + border.w);
        }
        else
        {
            outer = Vector4.zero;
            inner = Vector4.zero;
            padding = Vector4.zero;
            border = Vector4.zero;
            spriteSize = Vector2.zero;
            borderSize = Vector2.zero;
        }

        Rect rect = GetPixelAdjustedRect();

        var width = (spriteSize.x - borderSize.x) / multipliedPixelsPerUnit;
        var height = (spriteSize.y - borderSize.y) / multipliedPixelsPerUnit;

        border.x = (rect.width - width) * border.x / borderSize.x;
        border.z = (rect.width - width) * border.z / borderSize.x;
        border.y = (rect.height - height) * border.y / borderSize.y;
        border.w = (rect.height - height) * border.w / borderSize.y;

        Vector4 adjustedBorders = GetAdjustedBorders(border, rect);
        padding = padding / multipliedPixelsPerUnit;

        s_VertScratch[0] = new Vector2(padding.x, padding.y);
        s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

        s_VertScratch[1].x = adjustedBorders.x;
        s_VertScratch[1].y = adjustedBorders.y;

        s_VertScratch[2].x = rect.width - adjustedBorders.z;
        s_VertScratch[2].y = rect.height - adjustedBorders.w; 

        for (int i = 0; i < 4; ++i)
        {
            s_VertScratch[i].x += rect.x;
            s_VertScratch[i].y += rect.y;
        }

        s_UVScratch[0] = new Vector2(outer.x, outer.y);
        s_UVScratch[1] = new Vector2(inner.x, inner.y);
        s_UVScratch[2] = new Vector2(inner.z, inner.w);
        s_UVScratch[3] = new Vector2(outer.z, outer.w);

        toFill.Clear();

        for (int x = 0; x < 3; ++x)
        {
            int x2 = x + 1;

            for (int y = 0; y < 3; ++y)
            {
                int y2 = y + 1;

                // Check for zero or negative dimensions to prevent invalid quads (UUM-71372)
                if ((s_VertScratch[x2].x - s_VertScratch[x].x <= 0) || (s_VertScratch[y2].y - s_VertScratch[y].y <= 0))
                    continue;

                AddQuad(toFill,
                    new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                    new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                    color,
                    new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                    new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
            }
        }
    }

    static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
    {
        int startIndex = vertexHelper.currentVertCount;

        vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
        vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
    {
        Rect originalRect = rectTransform.rect;

        for (int axis = 0; axis <= 1; axis++)
        {
            float borderScaleRatio;

            // The adjusted rect (adjusted for pixel correctness)
            // may be slightly larger than the original rect.
            // Adjust the border to match the adjustedRect to avoid
            // small gaps between borders (case 833201).
            if (originalRect.size[axis] != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }

            // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
            // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
            float combinedBorders = border[axis] + border[axis + 2];
            if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }
        return border;
    }

    /// <summary>
    /// See ILayoutElement.CalculateLayoutInputHorizontal.
    /// </summary>
    public virtual void CalculateLayoutInputHorizontal() { }

    /// <summary>
    /// See ILayoutElement.CalculateLayoutInputVertical.
    /// </summary>
    public virtual void CalculateLayoutInputVertical() { }

    /// <summary>
    /// See ILayoutElement.minWidth.
    /// </summary>
    public virtual float minWidth { get { return 0; } }

    /// <summary>
    /// If there is a sprite being rendered returns the size of that sprite.
    /// In the case of a slided or tiled sprite will return the calculated minimum size possible
    /// </summary>
    public virtual float preferredWidth
    {
        get
        {
            if (sprite == null)
                return 0;

            return Sprites.DataUtility.GetMinSize(sprite).x / pixelsPerUnit;
        }
    }

    /// <summary>
    /// See ILayoutElement.flexibleWidth.
    /// </summary>
    public virtual float flexibleWidth { get { return -1; } }

    /// <summary>
    /// See ILayoutElement.minHeight.
    /// </summary>
    public virtual float minHeight { get { return 0; } }

    /// <summary>
    /// If there is a sprite being rendered returns the size of that sprite.
    /// In the case of a slided or tiled sprite will return the calculated minimum size possible
    /// </summary>
    public virtual float preferredHeight
    {
        get
        {
            if (sprite == null)
                return 0;

            return Sprites.DataUtility.GetMinSize(sprite).y / pixelsPerUnit;
        }
    }

    /// <summary>
    /// See ILayoutElement.flexibleHeight.
    /// </summary>
    public virtual float flexibleHeight { get { return -1; } }

    /// <summary>
    /// See ILayoutElement.layoutPriority.
    /// </summary>
    public virtual int layoutPriority { get { return 0; } }

    /// <summary>
    /// Calculate if the ray location for this image is a valid hit location. Takes into account a Alpha test threshold.
    /// </summary>
    /// <param name="screenPoint">The screen point to check against</param>
    /// <param name="eventCamera">The camera in which to use to calculate the coordinating position</param>
    /// <returns>If the location is a valid hit or not.</returns>
    /// <remarks> Also see See:ICanvasRaycastFilter.</remarks>
    public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (alphaHitTestMinimumThreshold <= 0)
            return true;

        if (alphaHitTestMinimumThreshold > 1)
            return false;

        if (sprite == null)
            return true;

        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local))
            return false;

        Rect rect = GetPixelAdjustedRect();

        // Convert to have lower left corner as reference point.
        local.x += rectTransform.pivot.x * rect.width;
        local.y += rectTransform.pivot.y * rect.height;

        local = MapCoordinate(local, rect);

        // Convert local coordinates to texture space.
        float x = local.x / sprite.texture.width;
        float y = local.y / sprite.texture.height;

        try
        {
            return sprite.texture.GetPixelBilinear(x, y).a >= alphaHitTestMinimumThreshold;
        }
        catch (UnityException e)
        {
            Debug.LogError("Using alphaHitTestMinimumThreshold greater than 0 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite.", this);
            return true;
        }
    }
    private Vector2 MapCoordinate(Vector2 local, Rect rect)
    {
        Rect spriteRect = sprite.rect;

        Vector4 border = sprite.border;
        Vector4 adjustedBorder = GetAdjustedBorders(border / pixelsPerUnit, rect);

        for (int i = 0; i < 2; i++)
        {
            if (local[i] <= adjustedBorder[i])
                continue;

            if (rect.size[i] - local[i] <= adjustedBorder[i + 2])
            {
                local[i] -= (rect.size[i] - spriteRect.size[i]);
                continue;
            }

            float lerp = Mathf.InverseLerp(adjustedBorder[i], rect.size[i] - adjustedBorder[i + 2], local[i]);
            local[i] = Mathf.Lerp(border[i], spriteRect.size[i] - border[i + 2], lerp);
        }

        return local + spriteRect.position;
    }

    // To track textureless images, which will be rebuild if sprite atlas manager registered a Sprite Atlas that will give this image new texture
    static List<HikerAdvImage> m_TrackedTexturelessImages = new List<HikerAdvImage>();
    static bool s_Initialized;
    static void RebuildImage(SpriteAtlas spriteAtlas)
    {
        for (var i = m_TrackedTexturelessImages.Count - 1; i >= 0; i--)
        {
            var g = m_TrackedTexturelessImages[i];
            if (null != g.sprite && spriteAtlas.CanBindTo(g.sprite))
            {
                g.SetAllDirty();
                m_TrackedTexturelessImages.RemoveAt(i);
            }
        }
    }

    private static void TrackImage(HikerAdvImage g)
    {
        if (!s_Initialized)
        {
            SpriteAtlasManager.atlasRegistered += RebuildImage;
            s_Initialized = true;
        }

        m_TrackedTexturelessImages.Add(g);
    }

    private static void UnTrackImage(HikerAdvImage g)
    {
        m_TrackedTexturelessImages.Remove(g);
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetMaterialDirty();
        SetVerticesDirty();
        SetRaycastDirty();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        m_PixelsPerUnitMultiplier = Mathf.Max(0.01f, m_PixelsPerUnitMultiplier);
    }

#endif
}
