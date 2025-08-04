using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteAnimation : MonoBehaviour
{
    [SerializeField] Sprite[] frames;
    [SerializeField] float[] frameTimes;
    [SerializeField] bool ignoreScaleTime = true;

    Image img;
    float frameT = 0f;
    int frameIdx = 0;
    private void OnEnable()
    {
        if (img == null)
        {
            img = GetComponent<Image>();
        }
    }

    private void Start()
    {
        frameIdx = -1;
        NextFrame();
    }

    void NextFrame()
    {
        frameIdx++;
        if (frameIdx >= frames.Length)
        {
            frameIdx -= frames.Length;
        }

        if (img != null)
        {
            img.sprite = frames[frameIdx];
        }

        if (frameIdx < frameTimes.Length)
        {
            frameT += frameTimes[frameIdx];
        }
        else if (frameTimes.Length > 0)
        {
            frameT += frameTimes[frameTimes.Length - 1];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (frameT <= 0)
        {
            NextFrame();
        }

        frameT -= ignoreScaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}
