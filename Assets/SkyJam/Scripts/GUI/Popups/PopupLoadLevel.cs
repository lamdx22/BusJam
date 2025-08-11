using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopupLoadLevel : MonoBehaviour
{
    [SerializeField]
    TweenAlpha tween;

    [SerializeField] private CanvasGroup canvasGroup;


    public void LoadLevel(BoardController prefab, int lvlNum, bool fadeIn)
    {
        gameObject.SetActive(true);
        StartCoroutine(CoLoadLevel(prefab, lvlNum, fadeIn));
    }

    IEnumerator CoLoadLevel(BoardController prefab, int lvlNum, bool fadeIn)
    {
        tween.PlayForward();
        tween.ResetToBeginning();
        if (fadeIn == false)
        {
            tween.Sample(1f, true);
            tween.enabled = false;
        }
        yield return new WaitForSecondsRealtime(0.6f);
        if (LevelManager.instance)
        {
            LevelManager.instance.SpawnLevel(prefab, lvlNum);

            yield return new WaitUntil(() => LevelManager.instance != null && LevelManager.instance.State >= LevelStatus.Inited);
        }
        tween.PlayReverse();
        yield return new WaitForSecondsRealtime(0.5f);
        gameObject.SetActive(false);
    }
}
