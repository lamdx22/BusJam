using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerUpButton : MonoBehaviour
{
    public string POName;
    public GameObject grpLock;
    public GameObject btnFree;
    public GameObject grpNum;
    public TMP_Text lbLockLabel;
    public GameObject btnPlus;

    protected void OnEnable()
    {
        SyncNetworkData();
    }

    public void SyncNetworkData()
    {
        //if (GameManager.instance != null)
        //{
        //    if (GameManager.instance.IsUnlockFeature(POName) == false)
        //    {
        //        grpLock.gameObject.SetActive(true);

        //        lbLockLabel.text = string.Format(Localization.Get("LevelShortLabel"), GameManager.instance.GetLevelUnlock(POName));

        //        btnFree.gameObject.SetActive(false);
        //        grpNum.gameObject.SetActive(false);
        //        btnPlus.gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        grpLock.gameObject.SetActive(false);
        //        var r = GameManager.instance.GetGamerCounter(POName);

        //        btnFree.gameObject.SetActive(r <= 0);
        //        //grpNum.gameObject.SetActive(r > 0);
        //        if (r > 0)
        //        {
        //            var c = GameManager.instance.GetCurrentMaterial(POName);
        //            grpNum.gameObject.SetActive(c > 0);
        //            btnPlus.gameObject.SetActive(c <= 0);
        //        }
        //    }
        //}
        //else
        {
            grpLock.gameObject.SetActive(false);
            btnFree.gameObject.SetActive(true);
            grpNum.gameObject.SetActive(false);
            btnPlus.gameObject.SetActive(false);
        }
    }
}
