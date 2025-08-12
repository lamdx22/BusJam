using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunaManager : MonoBehaviour
{
    public static LunaManager Instance { get; private set; }

    public bool IsEndGame { get; private set; }

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

    private void Start()
    {
        IsEndGame = false;
    }

    public void EndGame()
    {
        if (!IsEndGame)
        {
            IsEndGame = true;
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
        if (IsEndGame && Input.GetMouseButtonDown(0))
        {
            GoToStore();
        }
    }
}
