using Hiker.GUI;
using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowScreen : MonoBehaviour
{
    public ParticleSystem snowParticle;
    public ParticleSystem snowFilter;
    public GameObject iceScreen;

    [Header("Tăng dần số lượng")]
    public float minRate = 3f;   // Ít nhất 5 hạt/giây
    public float maxRate = 15f;  // Nhiều nhất 50 hạt/giây
    public float changeSpeed = 1f; // Tốc độ chuyển đổi
    public float minRateFilter = 0f;   // Ít nhất 5 hạt/giây
    public float maxRateFilter = 2f;  // Nhiều nhất 50 hạt/giây

    [Header("Thời gian sống của hạt")]
    public float startLifetime = 4f; // thời gian sống ban đầu
    public float maxLifetime = 6f;   // tối đa để tạo hiệu ứng dày
    public float windChangeInterval = 5f; // mỗi bao lâu đổi hướng

    [Header("Gió")]
    public float windStrength = 1f; // cường độ gió ngang
    public float windChangeSpeed = 0.5f; // tốc độ đổi hướng gió

    public AudioClip iceFrostSound;

    public float timePerFrost = 0.2f;

    private ParticleSystem.EmissionModule emissionSnow;
    private ParticleSystem.MainModule mainSnow;
    private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
    private ParticleSystem.EmissionModule emissionFilter;
    private float timer = 0f;

    public Camera cam;
    public Canvas canvas;

    private float windTimer;
    private float currentWind = 0f;
    private float targetWind = 0f;
    private float lastUpdateFilter = 0;

    // Start is called before the first frame update
    void Start()
    {
        //var emission = snowParticle.emission;
        //emission.rateOverTime = 3f;
        emissionSnow = snowParticle.emission;
        emissionFilter = snowFilter.emission;
        mainSnow = snowParticle.main;
        //velocityOverLifetime = snowParticle.velocityOverLifetime;

        

        // Bật velocityOverLifetime
        //velocityOverLifetime.enabled = true;

        ShapeScaler();

        Init();
    }

    public void Init()
    {
        snowFilter.Play();
        snowParticle.Play();
        emissionSnow.rateOverTime = minRate;
        emissionFilter.rateOverTime = minRateFilter;
        snowFilter.gameObject.SetActive(false);
        iceScreen.SetActive(false);
        lastUpdateFilter = 0;
    }

    public void ShowIceScreen()
    {
        iceScreen.SetActive(true);
        //MySoundManager.Instance.PlaySfxSound(iceFrostSound);
        snowFilter.Stop();
        snowParticle.Stop();
        snowFilter.Clear();
        snowParticle.Clear();
    }

    void ShapeScaler()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!canvas) return;

        float pixelPerUnit = canvas.referencePixelsPerUnit;
        float heightWorld = (canvas.pixelRect.height / Screen.height) * (cam.orthographicSize * 2f) * pixelPerUnit;
        float widthWorld = heightWorld * cam.aspect;
        Debug.Log("width: " + widthWorld + ", height: " + heightWorld);

        // Snow
        var shape = snowParticle.shape;
        //shape.shapeType = ParticleSystemShapeType.Rectangle;
        //shape.scale = new Vector3(widthWorld, heightWorld, shape.scale.z);
        shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
        shape.radius = widthWorld/2;

        // Filter
        var main = snowFilter.main;
        main.startSize3D = true; // Bật chế độ 3D Start Size

        main.startSizeX = widthWorld;
        main.startSizeY = heightWorld;
        main.startSizeZ = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        //timer += Time.deltaTime;
        //float t = Mathf.Clamp01(timer / duration); // 0 → 1
        float t = LevelManager.instance.GetPercentageTimePassed();
        //Debug.Log("t1: " + t + " real t:" + timeLevel);

        if (t > 0.1 && snowFilter != null)
        {
            if (!snowFilter.gameObject.activeSelf)
            {
                snowFilter.gameObject.SetActive(true);
                //lastUpdateFilter = timePerFrost;
                lastUpdateFilter = t;
                MySoundManager.Instance.PlaySfxSound(iceFrostSound);
            }
            if (t - lastUpdateFilter > timePerFrost)
            {
                lastUpdateFilter = t;
                var emissionFilter = snowFilter.emission;
                float current = Mathf.Lerp(minRateFilter, maxRateFilter, t);
                emissionFilter.rateOverTime = current;
                MySoundManager.Instance.PlaySfxSound(iceFrostSound);
            }
            //if (lastUpdateFilter > 0)
            //{
            //    lastUpdateFilter -= Time.deltaTime;
            //    if (lastUpdateFilter <= 0)
            //    {
            //        lastUpdateFilter = timePerFrost;
            //        var emissionFilter = snowFilter.emission;
            //        float current = Mathf.Lerp(minRateFilter, maxRateFilter, t);
            //        emissionFilter.rateOverTime = current;
            //        MySoundManager.Instance.PlaySfxSound(iceFrostSound);
            //    }
            //}
        } 


        // Tăng dần số hạt tới giới hạn an toàn
        float currentRate = Mathf.Lerp(minRate, maxRate, t);
        emissionSnow.rateOverTime = currentRate;

        // Đồng thời tăng thời gian sống để tạo cảm giác dày hơn
        //mainSnow.startLifetime = Mathf.Lerp(startLifetime, maxLifetime, ((t < 0.3f)? 0.3f : t));

        // Đổi hướng gió theo chu kỳ
        windTimer += Time.deltaTime;
        if (windTimer >= windChangeInterval)
        {
            windTimer = 0f;
            targetWind = Random.Range(-windStrength, windStrength);
        }

        // Chuyển gió mượt mà
        currentWind = Mathf.Lerp(currentWind, targetWind, Time.deltaTime * windChangeSpeed);
        AnimationCurve flatCurve = AnimationCurve.Linear(0f, currentWind, 1f, currentWind);
        //velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(1f, flatCurve);
    }
}
