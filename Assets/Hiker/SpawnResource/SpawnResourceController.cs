using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hiker.GUI;
using Hiker.Util;
using TMPro;
using DG.Tweening;
using SkyJam;

public class SpawnResourceController : MonoBehaviour
{
    [SerializeField] private FlyResource flyGold;
    [SerializeField] private FlyResource flyTheLuc;

    [SerializeField] private List<FlyResource> flyTools;

    [SerializeField] private Vector2 spawnRange;
    [SerializeField] private Vector2 startScale;

    public static SpawnResourceController instance;

    public static void SyncRectTransformPosWithOtherInScreenSpace(Transform tran, Transform refTran, Camera cam)
    {
        var rectParent = tran.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, refTran.position);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectParent, screenPos, cam, out Vector3 wPos);
        tran.position = wPos;
    }

    private Vector3 VerifyPosOnScreen(Transform target)
    {
        Vector3 pos = target.position;

        var canvas = target.GetComponentInParent<Canvas>();
        if (canvas && canvas.rootCanvas.worldCamera && canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            var screenPos = RectTransformUtility.WorldToScreenPoint(canvas.rootCanvas.worldCamera, pos);
            var rect = this.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPos,
                this.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                out pos);
        }

        return pos;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            return;
        }
        instance = this;
    }

    public GameObject SpawnObject(FlyResource prefab, Transform spawnPos = null, Transform target = null, bool keepScale = true)
    {
        var pos = new Vector3(
            Random.Range(spawnRange.x, spawnRange.x),
            Random.Range(spawnRange.y, spawnRange.y), 
            0);

        if(spawnPos != null)
        {
            pos = transform.InverseTransformPoint(VerifyPosOnScreen(spawnPos));
        }

        var flyObj = ObjectPoolManager.Spawn(prefab.gameObject, pos, Quaternion.identity, transform);

        if(target != null)
        {
            flyObj.GetComponent<FlyResource>().enabled = false;
            flyObj.GetComponent<FlyResource>().SetTarget(target);
            flyObj.GetComponent<FlyResource>().enabled = true;
        }

        if (keepScale)
        {
            flyObj.transform.localScale = Vector3.one;
        }
        else
        {
            flyObj.transform.localScale = Vector3.one * Random.Range(startScale.x, startScale.y);
        }
        
        return flyObj;
    }

    int flyGoldNum = 0;
    int curFlyGold = 0;
    public void SpawnGold(long money)
    {
        //ResourceBar.instance?.goldDisplay?.DelaySync(true);
        flyGoldNum = flyGold.FlyItemMaxCount;
        curFlyGold = 0;
        if (money <= flyGoldNum)
        {
            flyGoldNum = (int)money;
            for (int i = 0; i < money; ++i)
            {
                var gold = SpawnObject(flyGold, null, null, false);
                gold.gameObject.SetActive(true);
                gold.GetComponent<FlyResource>().Value = 1;
            }
        }
        else
        {
            var oneGold = money / flyGoldNum;
            for (int i = 0; i < flyGoldNum; ++i)
            {
                var gold = SpawnObject(flyGold, null, null, false);
                gold.gameObject.SetActive(true);

                if (i == 0)
                {
                    var pad = money % flyGoldNum;
                    gold.GetComponent<FlyResource>().Value = oneGold + pad;
                }
                else
                {
                    gold.GetComponent<FlyResource>().Value = oneGold;
                }
            }
        }
    }

    //[GUIDelegate]
    public void PlusGold(long add)
    {
        curFlyGold++;
        if (curFlyGold >= flyGoldNum)
        {
            ScreenPlayable.instance.SyncNetworkData();
        }
        else
        {
            ScreenPlayable.instance.UpdateGold(ScreenPlayable.instance.CurGold + add);
        }
    }
}
