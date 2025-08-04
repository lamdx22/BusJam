using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerOnEnable : MonoBehaviour
{
    public float Duration;

    public bool IgnoreTimeScale = false;
    public UnityEvent onEnable;
    public UnityEvent onTimer;

    bool isActivated = false;

    float time = 0;
    private void OnEnable()
    {
        isActivated = false;
        time = 0;

        onEnable?.Invoke();
    }

    private void OnDisable()
    {
        isActivated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActivated == false)
        {
            if (time < Duration)
            {
                time += IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            }
            else
            {
                onTimer?.Invoke();
                isActivated = true;
            }
        }
    }
}
