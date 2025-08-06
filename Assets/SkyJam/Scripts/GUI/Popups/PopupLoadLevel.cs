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

        //if (fadeIn)
        //{
        //    canvasGroup.alpha = 0f;
        //    canvasGroup.DOFade(1f, 0.6f);
        //}
        //else
        //{
        //    canvasGroup.alpha = 1f;
        //    canvasGroup.DOFade(0f, 0.1f);
        //}

        //yield return new WaitForSecondsRealtime(0.6f);

        //if (LevelManager.instance)
        //{
        //    LevelManager.instance.SpawnLevel(prefab, lvlNum);
        //    yield return new WaitUntil(() => LevelManager.instance.State >= LevelStatus.Inited);
        //}

        //canvasGroup.DOFade(0f, 0.5f);
        //yield return new WaitForSecondsRealtime(0.5f);
        //gameObject.SetActive(false);
    }
}
