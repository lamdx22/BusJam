using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunaManager : MonoBehaviour
{
    public static LunaManager Instance { get; private set; }

    private bool isEndGame = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EndGame()
    {
        if (!isEndGame)
        {
            isEndGame = true;
            //SoundManager.instance.StartFadeOutMusic();
            MySoundManager.Instance.StopMainMusic();
            Luna.Unity.LifeCycle.GameEnded();
        }
    }

    public void GoToStore()
    {
        Luna.Unity.Playable.InstallFullGame();
    }

    public void EndGameAndGoToStore()
    {
        EndGame();
        GoToStore();
    }

    private void Update()
    {
        if (isEndGame && Input.GetMouseButtonDown(0))
        {
            GoToStore();
        }
    }
}
