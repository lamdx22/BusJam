using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class SafeAreaHelper : MonoBehaviour
{
    CanvasHelper canvasHelper;
    RectTransform rectTransform;
    public bool ignoreBannerOffset = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        OnTransformParentChanged();
    }

    private void OnTransformParentChanged()
    {
        var curCanvas = GetComponentInParent<CanvasHelper>();

        if (canvasHelper && canvasHelper != curCanvas)
        {
            if (rectTransform != null)
            {
                canvasHelper.RemoveSafeAreaHelper(rectTransform);
            }
        }

        canvasHelper = curCanvas;

        if (canvasHelper)
        {
            if (rectTransform != null)
            {
                canvasHelper.AddSafeAreaHelper(rectTransform);
                canvasHelper.ApplySafeArea(rectTransform, ignoreBannerOffset == false);
            }
        }
    }

    private void OnDestroy()
    {
        if (canvasHelper != null && rectTransform)
        {
            canvasHelper.RemoveSafeAreaHelper(rectTransform);
        }
    }
}
