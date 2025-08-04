using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNamChamVisual : MonoBehaviour
{
    public GameObject[] activeObj;

    private void OnEnable()
    {
        if (LevelManager.instance)
        {
            foreach (var t in activeObj)
            {
                if (t) t.SetActive(LevelManager.instance.IsMagnetActive);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.instance != null && LevelManager.instance.State == LevelStatus.Started)
        {
            foreach (var t in activeObj)
            {
                if (t) t.SetActive(LevelManager.instance.IsMagnetActive);
            }
        }
    }
}
