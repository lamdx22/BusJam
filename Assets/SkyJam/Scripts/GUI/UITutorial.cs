using Hiker.GUI;
using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorial : MonoBehaviour
{
    public static UITutorial instance = null;

    public string TutName;
    public bool hideIsDestroy = true;
    public bool pauseBattle = true;
    public bool pauseTime = true;

    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            instance.Hide();
        }

        instance = this;

        if (pauseBattle && pauseTime)
        {
            if (LevelManager.instance != null)
            {
                Time.timeScale = 0f;
            }
        }
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (pauseBattle && pauseTime)
        {
            if (LevelManager.instance != null)
            {
                Time.timeScale = 1f;
            }
        }
    }

    //[GUIDelegate]
    public void Hide()
    {
        if (gameObject)
        {
            if (hideIsDestroy)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
