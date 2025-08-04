using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class BuiltInDynamicResolution : MonoBehaviour
{
    [Header("Camera and UI")]
    public Camera mainCamera;
    //public Canvas uiCanvas;
    public RawImage outputImage;

    [Header("Resolution Scale Settings")]
    [Range(0.3f, 1.0f)] public float minScale = 0.5f;
    [Range(0.3f, 1.0f)] public float maxScale = 1.0f;
    public float currentScale = 1.0f;

    [Header("Performance Targeting")]
    public int targetFPS = 60;

    public static BuiltInDynamicResolution instance = null;

    private RenderTexture dynamicRT;
    private float targetFrameTime = 0.1333f;

    private float[] recentFT = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private int frameCount = 0;

    private void OnEnable()
    {
        instance = this;
        targetFrameTime = 1f / targetFPS;
        frameCount = 0;
    }

    void Start()
    {
        if (!mainCamera || !outputImage)
        {
            Debug.LogError("Assign all required references.");
            //return;
        }

        Application.targetFrameRate = targetFPS;

        if (mainCamera != null && outputImage != null)
        {
            CreateRenderTexture(currentScale);
            AssignRenderTarget();
        }

        if (outputImage != null)
        {
            outputImage.color = Color.white;
        }
    }

    public void SetCamera(Camera cam)
    {
        if (mainCamera == cam) return;

        if (mainCamera != null && mainCamera != cam)
        {
            mainCamera.targetTexture = null;
        }

        mainCamera = cam;
        if (mainCamera != null && outputImage != null)
        {
            if (dynamicRT == null)
            {
                CreateRenderTexture(currentScale);
            }
            AssignRenderTarget();
            Update();
        }
    }

    float curSmoothOutDeltatime = 0f;

    void Update()
    {
        if (!mainCamera || !outputImage)
        {
            return;
        }

        if (frameCount < recentFT.Length)
        {
            recentFT[frameCount++] = Time.unscaledDeltaTime;

            return;
        }

        float dt = Time.unscaledDeltaTime;
        float totalDt = dt;
        for (int i = 1; i < recentFT.Length; ++i)
        {
            totalDt += recentFT[i];
            recentFT[i - 1] = recentFT[i];
        }
        recentFT[recentFT.Length - 1] = dt;
        curSmoothOutDeltatime = totalDt / recentFT.Length;

        AdjustScaleBasedOnPerformance();
        UpdateRenderTextureIfNeeded();
    }

    void AdjustScaleBasedOnPerformance()
    {
        var dt = curSmoothOutDeltatime;

        if (dt > Mathf.Epsilon)
        {
            if (dt > targetFrameTime * 1.15f)
            {
                currentScale = Mathf.Max(minScale, currentScale - 0.1f);
            }
            else
            if (dt < targetFrameTime * 1.05f)
            {
                currentScale = Mathf.Min(maxScale, currentScale + 0.1f);
            }

            Debug.LogFormat("currentScale {0:F1}", currentScale);
        }
    }

    void CreateRenderTexture(float scale)
    {
        int width = Mathf.RoundToInt(Screen.width * scale);
        int height = Mathf.RoundToInt(Screen.height * scale);

        if (dynamicRT != null)
            dynamicRT.Release();

        dynamicRT = new RenderTexture(width, height, 24);
        dynamicRT.name = $"DynamicRT_{width}x{height}";
        dynamicRT.filterMode = FilterMode.Bilinear;
        dynamicRT.useMipMap = false;
        dynamicRT.autoGenerateMips = false;
    }

    void AssignRenderTarget()
    {
        if (mainCamera != null)
        {
            mainCamera.targetTexture = dynamicRT;
            outputImage.texture = dynamicRT;
        }
    }

    void UpdateRenderTextureIfNeeded()
    {
        if (dynamicRT.width != Mathf.RoundToInt(Screen.width * currentScale) ||
            dynamicRT.height != Mathf.RoundToInt(Screen.height * currentScale))
        {
            CreateRenderTexture(currentScale);
            AssignRenderTarget();
        }
    }
}

