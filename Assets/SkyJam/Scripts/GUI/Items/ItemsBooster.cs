using Hiker.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsBooster : MonoBehaviour
{
    [SerializeField] private string codeName;
    [SerializeField] private string codeNameTime;
    [SerializeField] private Image icon;

    [SerializeField] private GameObject activeObj;
    [SerializeField] private GameObject unActiveObj;
    [SerializeField] private GameObject unlimitedObj;
    [SerializeField] private TMPro.TMP_Text countTxt;
    [SerializeField] private TMPro.TMP_Text timeTxt;

    [SerializeField] private GameObject objLock;
    [SerializeField] private TMPro.TMP_Text txtUnlock;

    private int curActive = 0;
    long curCount = 0;

    DateTime unlimitEndTime = new DateTime();

    public int CurActive { get { return curActive; } }
    public bool IsLocked { get; private set; }

    public void UpdateInfo()
    {
        if (GameManager.instance == null) return;

        bool isUnlock = GameManager.instance.IsUnlockFeature(codeName);
        IsLocked = (isUnlock == false);
        if (isUnlock)
        {
            var usedCount = GameManager.instance.GetGamerCounter(codeName, 0);

            long count = 0;
            var matNum = GameManager.instance.GetCurrentMaterial(codeName);

            long seconds = 0;
            var matTime = GameManager.instance.GetCurrentMaterial(codeNameTime);
            if (matTime > 0) seconds = matTime;

            curCount = count;

            if (seconds > 0 && usedCount > 0)
            {
                curActive = 2;
                unlimitEndTime = TimeUtils.Now.AddSeconds(seconds);
            }
            else
            {
                if (curActive == 2) curActive = 0;
                countTxt.text = count.ToString();
            }

            activeObj.SetActive(curActive == 1 && seconds <= 0);
            unActiveObj.SetActive(curActive == 0 && seconds <= 0 && usedCount > 0);
            unlimitedObj.SetActive(curActive == 2 && seconds > 0);

            objLock.gameObject.SetActive(false);
        }
        else
        {
            curActive = 0;
            int lvlUnlock = GameManager.instance.GetLevelUnlock(codeName);
            //txtUnlock.text = string.Format(Localization.Get("LevelShortLabel"), lvlUnlock);
            objLock.gameObject.SetActive(true);

            unlimitedObj.SetActive(false);
            activeObj.SetActive(false);
            unActiveObj.SetActive(false);
        }
        
    }
    float interval = 1f;
    float lastCheck = -1f;
    float timeTime = 0f;

    private void Update()
    {
        if (IsLocked) return;

        if (timeTxt.gameObject.activeInHierarchy)
        {
            timeTime = Time.time;
            if (lastCheck < timeTime - interval)
            {
                lastCheck = timeTime;
                timeTxt.text = GameManager.GetTimeRemainStringNoDate(TimeUtils.Now, unlimitEndTime);
                if(TimeUtils.Now > unlimitEndTime)
                {
                    UpdateInfo();
                }
            }
        }
    }

    //[GUIDelegate]
    public void OnItemPress()
    {
        if (IsLocked)
        {
            curActive = 0;
            //PopupMessage.Create(string.Format(Localization.Get("UnlockAtLevel"), GameManager.instance.GetLevelUnlock(codeName)));
            return;
        }

        if (GameManager.instance.IsUnlockFeature(codeName) &&
            GameManager.instance.GetGamerCounter(codeName, 0) == 0)
        {
            curActive = 1;
            UpdateInfo();
            return;
        }

        if (unlimitedObj.activeInHierarchy == false) 
        {
            if(curCount > 0)
            {
                if (curActive == 0)
                {
                    curActive = 1;
                    UpdateInfo();
                }
                else if (curActive == 1)
                {
                    curActive = 0;
                    UpdateInfo();
                }
            }
            else
            {
                //ShowPopupMuaBooster();
            }
        }
        else
        {
            //Khong thay doi trang thai
        }
    }
}
