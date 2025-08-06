using DG.Tweening;
using Hiker.GUI;
using SkyJam;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTip : MonoBehaviour
{
    public Transform targetPoint;
    public float moveTime = 0.2f;
    public float fadeTime = 0.2f;

    private bool isFirstTouch = false;
    private SpriteRenderer sr;
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        isFirstTouch = false;

        //transform.DOMove(targetPoint.position, duration).SetLoops(-1, LoopType.Restart);
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPoint.position, moveTime))
           .Append(sr.DOFade(0f, fadeTime)) // Mờ dần
           .AppendCallback(() =>
           {
               transform.position = startPos; // về lại chỗ cũ
           })
           .Append(sr.DOFade(1f, fadeTime)) // Hiện lại
           .SetLoops(-1); // Lặp vô hạn
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isFirstTouch = true;
            SoundManager.instance?.StartMainMusic();
        }

        if (isFirstTouch)
        {
            Destroy(gameObject);
        }
    }
}
