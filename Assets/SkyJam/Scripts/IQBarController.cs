using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IQBarController : MonoBehaviour
{
    [SerializeField]
    private Image imageFill;

    [SerializeField]
    private int maxIQ = 100;

    [SerializeField]
    private float fillSpeed = 5f;

    private int currentIQ = 0;
    private float targetFill = 0f;

    // Start is called before the first frame update
    void Start()
    {
        ResetIQ();
    }

    private void Update()
    {
        imageFill.fillAmount = Mathf.Lerp(imageFill.fillAmount, targetFill, Time.deltaTime * fillSpeed);
    }

    public void ResetIQ()
    {
        currentIQ = 0;
        targetFill = 0f;
        imageFill.fillAmount = 0f;
    }

    public void Increase(int amount)
    {
        currentIQ += amount;
        currentIQ = Mathf.Clamp(currentIQ, 0, maxIQ);

        targetFill = (float)currentIQ / maxIQ;
    }
}
