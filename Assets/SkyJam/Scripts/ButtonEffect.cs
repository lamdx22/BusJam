using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEffect : MonoBehaviour
{
    void Start()
    {
        // Bắt đầu scale lặp lại
        transform.DOScale(1.1f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo) // Lặp vô hạn, kiểu Yoyo (to → nhỏ → to...)
            .SetEase(Ease.InOutSine);   // Làm mềm chuyển động
    }
}
