using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySoundManager : MonoBehaviour
{
    public static MySoundManager Instance { get; private set; }

    public AudioSource musicSource;
    public AudioClip mainClip;

    public AudioSource sfxSource;

    public AudioClip win_clip;
    public AudioClip lose_clip;
    public AudioClip nhaclen_clip;
    public AudioClip haxuong_clip;
    public AudioClip sfxBay_clip;
    public AudioClip sfxYay_clip;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySoundWin()
    {
        if (win_clip != null) PlaySfxSound(win_clip, 1f);
    }

    public void PlaySoundLose()
    {
        if (lose_clip != null) PlaySfxSound(lose_clip, 1f);
    }

    public void PlaySfxYay()
    {
        if (sfxYay_clip != null) PlaySfxSound(sfxYay_clip, 1f);
    }

    public void PlaySfxSound(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.clip = clip;
            sfxSource.PlayOneShot(clip, volume);
        } 
    }

    public void PlayMainMusic()
    {
        if (mainClip == null || musicSource == null) return;
        musicSource.clip = mainClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMainMusic()
    {
        musicSource?.Stop();
    }
}
