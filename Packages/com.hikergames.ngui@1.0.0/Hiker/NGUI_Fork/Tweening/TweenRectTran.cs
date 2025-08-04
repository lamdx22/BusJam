using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Tween/Tween RectTransform")]
[RequireComponent(typeof(RectTransform))]
public class TweenRectTran : UITweener
{
    public Vector2 from;
    public Vector2 to;

    RectTransform mTrans;

    public RectTransform cachedTransform { get { if (mTrans == null) mTrans = GetComponent<RectTransform>(); return mTrans; } }

    /// <summary>
    /// Tween's current value.
    /// </summary>

    public Vector2 value
    {
        get
        {
            return cachedTransform.sizeDelta;
        }
        set
        {
            cachedTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
            cachedTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(cachedTransform.parent as RectTransform);
        }
    }

    void Awake()
    {
        //mRect = GetComponent<UIRect>();
    }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished) { value = from * (1f - factor) + to * factor; }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenRectTran Begin(GameObject go, float duration, Vector3 pos)
    {
        TweenRectTran comp = UITweener.Begin<TweenRectTran>(go, duration);
        comp.from = comp.value;
        comp.to = pos;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenRectTran Begin(GameObject go, float duration, Vector3 pos, bool worldSpace)
    {
        TweenRectTran comp = UITweener.Begin<TweenRectTran>(go, duration);
        comp.from = comp.value;
        comp.to = pos;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }

    [ContextMenu("Set 'From' to current value")]
    public override void SetStartToCurrentValue() { from = value; }

    [ContextMenu("Set 'To' to current value")]
    public override void SetEndToCurrentValue() { to = value; }

    [ContextMenu("Assume value of 'From'")]
    void SetCurrentValueToStart() { value = from; }

    [ContextMenu("Assume value of 'To'")]
    void SetCurrentValueToEnd() { value = to; }
}
