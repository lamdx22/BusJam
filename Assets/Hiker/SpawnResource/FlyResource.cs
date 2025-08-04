using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Hiker.Util;
using UnityEngine.Events;

public class FlyResource : MonoBehaviour
{
    [Header("Not Nullable")]
    public int FlyItemMaxCount = 10;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 randomPosRange;
    [SerializeField] private Vector2 firstScaleTimeRange;
    [SerializeField] private Vector2 moveBackTimeRange;

    [Header("Nullable")]
    [SerializeField] private Transform spawnControllerTransform;
    [SerializeField] private SpawnResourceController spawnResourceController;
    [SerializeField] private List<Ease> randCurve;

    public long Value { get; set; }
    public bool IsLastItem { get; set; }
    public UnityEvent<long> onFlyingFinished;

    private void OnEnable()
    {
        OnSpawn();
        if(randCurve == null) randCurve = new List<Ease>();
        if(randCurve.Count == 0) randCurve.Add(Ease.InCubic);
    }

    private void OnDisable()
    {
        DOTween.Kill(transform);
    }

    private void OnSpawn()
    {
        if (spawnControllerTransform == null)
        {
            spawnControllerTransform = SpawnResourceController.instance.transform;
        }

        if (spawnResourceController == null)
        {
            spawnResourceController = SpawnResourceController.instance;
        }

        float firstScaleTime = Random.Range(firstScaleTimeRange.x, firstScaleTimeRange.y);

        var targetPos = transform.localPosition + new Vector3(Random.Range(-randomPosRange.x, randomPosRange.x)
                                   , Random.Range(-randomPosRange.y, randomPosRange.y)
                                   , 0);
        transform.DOLocalMove(targetPos, firstScaleTime).SetEase(Ease.OutCubic).SetUpdate(true);
        transform.DOScale(Vector3.one, firstScaleTime).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            float moveBackTime = Random.Range(moveBackTimeRange.x, moveBackTimeRange.y);
            transform.DOScale(Vector3.one * 0.7f, moveBackTime).SetEase(Ease.Linear).SetUpdate(true);
            CheckTypeResource(moveBackTime);
        }).SetUpdate(true);
    }

    private Vector3 VerifyPosOnScreen(Transform target)
    {
        Vector3 pos = target.position;
        
        var canvas = target.GetComponentInParent<Canvas>();

        if (canvas && canvas.rootCanvas.worldCamera && canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            var rect = spawnControllerTransform.GetComponent<RectTransform>();
            var screenPos = RectTransformUtility.WorldToScreenPoint(canvas.rootCanvas.worldCamera, pos);

            RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPos,
                spawnControllerTransform.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                out pos);
        }

        return pos;
    }

    private void CheckTypeResource(float time)
    {
        if (target != null)
        {
            Vector3 pos = VerifyPosOnScreen(target);
            transform.DOMoveX(pos.x, time).SetEase(randCurve[Random.Range(0, randCurve.Count)]).SetUpdate(true);
            transform.DOMoveY(pos.y, time).SetEase(Ease.InCubic).SetUpdate(true).OnComplete(() =>
            {
                transform.DOScale(Vector3.zero, 0.3f).SetUpdate(true);
                onFlyingFinished?.Invoke(Value);
                DOVirtual.DelayedCall(0.3f, () => ObjectPoolManager.Unspawn(gameObject));
            }).SetUpdate(true);
        }
        else
        {
            onFlyingFinished?.Invoke(Value);
            DOVirtual.DelayedCall(0.3f, () => ObjectPoolManager.Unspawn(gameObject));
        }
    }

    public void SetTarget(Transform tg)
    {
        target = tg;
    }
}
