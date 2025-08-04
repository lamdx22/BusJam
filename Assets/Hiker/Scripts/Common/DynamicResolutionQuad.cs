using UnityEngine;

namespace Hiker
{
    [RequireComponent(typeof(MeshRenderer))]
    public class DynamicResolutionQuad : MonoBehaviour
    {
        public Camera cam3D;
        public Camera mainCamera;
        [Header("Performance Targeting")]
        public int targetFPS = 60;
        [Header("Resolution Scale Settings")]
        [Range(0.3f, 1.0f)] public float minScale = 0.5f;
        [Range(0.3f, 1.0f)] public float maxScale = 1.0f;
        public float currentScale = 1.0f;

        public bool isBicubicUpscale = false;

        private RenderTexture dynamicRT;
        private Material quadMaterial;

        private float targetFrameTime = 0.16667f;

        //private float[] recentFT = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //private int frameCount = 0;
        static readonly FrameTiming[] frameTimings = new FrameTiming[10];

        private float curSmoothOutDeltatime = 0f;
        private float curSmoothCPUDT = 0f;

        public static DynamicResolutionQuad instance = null;
        bool isAvailable = true;

        public bool IsCpuBotneck { get { return curSmoothCPUDT > curSmoothOutDeltatime && curSmoothCPUDT > (1.2f * targetFrameTime); } }

        void Start()
        {
            Application.targetFrameRate = targetFPS;

            //isAvailable = FrameTimingManager.IsFeatureEnabled();

            if (!mainCamera)
            {
                Debug.LogError("Assign the main camera.");
                //return;
            }

            quadMaterial = GetComponent<MeshRenderer>().material;
            if (cam3D != null)
            {
                CreateRenderTexture(currentScale);
                AssignRenderTarget();
            }
            if (mainCamera != null)
            {
                FitQuadToScreen();
            }
        }

        void Update()
        {
            if (cam3D == null) return;

            if (isAvailable)
            {
                FrameTimingManager.CaptureFrameTimings();
                var numFrame = FrameTimingManager.GetLatestTimings((uint)frameTimings.Length, frameTimings);

                if (numFrame < frameTimings.Length)
                {
                    return;
                }
            }
            else
            {
                return;
            }
            //if (frameCount < recentFT.Length)
            //{
            //    recentFT[frameCount++] = FrameTimingManager.;

            //    return;
            //}

            //float dt = Time.unscaledDeltaTime;
            //var totalDt = frameTimings[0].gpuFrameTime;
            //var cpuDt = frameTimings[0].cpuFrameTime;
            //for (int i = 1; i < frameTimings.Length; ++i)
            //{
            //    totalDt += frameTimings[i].gpuFrameTime;
            //    cpuDt += frameTimings[i].cpuFrameTime;
            //}
            //curSmoothOutDeltatime = (float)(totalDt / frameTimings.Length / 1000d);

            //curSmoothCPUDT = (float)(cpuDt / frameTimings.Length / 1000d);

            //AdjustResolutionScale();
            //UpdateRenderTextureIfNeeded();
        }
        private void OnEnable()
        {
            instance = this;
            targetFrameTime = 1f / targetFPS;
            //frameCount = 0;
        }

        public void SetCamera(Camera cam)
        {
            if (cam3D == cam) return;

            if (cam3D != null && cam3D != cam)
            {
                cam3D.targetTexture = null;
            }

            cam3D = cam;
            if (cam3D != null)
            {
                if (dynamicRT == null)
                {
                    CreateRenderTexture(currentScale);
                }
                AssignRenderTarget();
                //FitQuadToScreen();
                Update();
            }
        }

        void AdjustResolutionScale()
        {
            if (isAvailable == false)
            {
                currentScale = 1f;
                return;
            }

            var dt = curSmoothOutDeltatime;

            if (dt > Mathf.Epsilon)
            {
                if (dt > targetFrameTime * 1.25f)
                {
                    currentScale = Mathf.Max(minScale, currentScale - 0.05f);

                    //Debug.LogFormat("currentScale {0:F2}", currentScale);
                }
                else
                if (dt < targetFrameTime * 1.05f)
                {
                    currentScale = Mathf.Min(maxScale, currentScale + 0.05f);

                    //Debug.LogFormat("currentScale {0:F2}", currentScale);
                }
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
        }

        void AssignRenderTarget()
        {
            cam3D.targetTexture = dynamicRT;
            quadMaterial.mainTexture = dynamicRT;
            if (isBicubicUpscale)
            {
                Vector4 texelSize = new Vector4(1f / dynamicRT.width, 1f / dynamicRT.height, dynamicRT.width, dynamicRT.height);
                quadMaterial.SetVector("_MainTex_TexelSize", texelSize);
            }
        }

        void UpdateRenderTextureIfNeeded()
        {
            int expectedWidth = Mathf.RoundToInt(Screen.width * currentScale);
            int expectedHeight = Mathf.RoundToInt(Screen.height * currentScale);

            if (dynamicRT.width != expectedWidth || dynamicRT.height != expectedHeight)
            {
                CreateRenderTexture(currentScale);
                AssignRenderTarget();
            }
        }

        void FitQuadToScreen()
        {
            // Ensure the quad fits the full screen in camera view
            var dis = mainCamera.nearClipPlane + 0.5f;
            transform.position = mainCamera.transform.position + mainCamera.transform.forward * dis;
            transform.rotation = mainCamera.transform.rotation;
            transform.localScale = new Vector3(1f, 1f, 1f);

            float h = 10;
            if (mainCamera.orthographic)
            {
                h = mainCamera.orthographicSize;
            }
            else
            {
                h = Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * dis;
            }

            float w = h * mainCamera.aspect;
            transform.localScale = new Vector3(w * 2f, h * 2f, 1f);
        }
    }
}

