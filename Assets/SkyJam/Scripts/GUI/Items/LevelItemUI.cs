using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelItemUI : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text lvlTxt;
    [SerializeField] private Image blueBG;
    [SerializeField] private GameObject numberObj;

    public void SetInfo(int lvl, bool isBlue = false)
    {
        numberObj.gameObject.SetActive(true);
        lvlTxt.text = lvl.ToString();
        blueBG.color = isBlue ? Color.white : new Color(1, 1, 1, 0);
    }

    public void PlayFadeInBlue()
    {
        blueBG.color = new Color(1, 1, 1, 0);
        blueBG.DOFade(1, 1.5f);
    }

    public void HideNumber()
    {
        numberObj.gameObject.SetActive(false);
    }
}
