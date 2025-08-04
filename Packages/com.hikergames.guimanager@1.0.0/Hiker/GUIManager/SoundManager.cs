using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hiker.GUI
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance = null;

        public AudioSource musicSource;
        public AudioClip mainClip;
        public AudioClip battleClip;
        public bool MusicEnable = true;
        public bool SoundEnable = true;

        private void OnEnable()
        {
            instance = this;
            LoadSoundAndMusicSetting();
        }

        public void LoadSoundAndMusicSetting()
        {
            MusicEnable = (PlayerPrefs.GetInt("MusicEnable", 1) == 1);
            SoundEnable = (PlayerPrefs.GetInt("SoundEnable", 1) == 1);

            ApplySoundAndMusicSetting();
        }

        public void SaveSoundAndMusicSetting()
        {
            if (MusicEnable)
                PlayerPrefs.SetInt("MusicEnable", 1);
            else
                PlayerPrefs.SetInt("MusicEnable", 0);

            if (SoundEnable)
                PlayerPrefs.SetInt("SoundEnable", 1);
            else
                PlayerPrefs.SetInt("SoundEnable", 0);

        }

        public void MuteAudio()
        {
            AudioListener.volume = 0;
        }

        public void UnmuteAudio()
        {
            AudioListener.volume = SoundEnable ? 1f : 0f;
        }

        public void ApplySoundAndMusicSetting()
        {
            if (musicSource != null)
            {
                if (MusicEnable == false)
                {
                    musicSource.Stop();
                    musicSource.volume = 0;
                }
                else
                if (GUIManager.Instance && GUIManager.Instance.CurrentScreen == "Main")
                {
                    if (musicSource.isPlaying == false || musicSource.volume <= 0f)
                    {
                        StartMainMusic();
                    }
                }
            }

            AudioListener.volume = SoundEnable ? 1f : 0f;
        }

        public void StartMainMusic()
        {
            if (MusicEnable)
            {
                //musicSource.volume = 0f;
                this.StopCoroutine("FadeInBattle");
                this.StopCoroutine("FadeInMain");
                this.StopCoroutine("FadeOutMusic");
                this.StartCoroutine("FadeInMain", 1f);
            }
        }

        public void StartFadeOutMusic()
        {
            if (MusicEnable)
            {
                //musicSource.volume = 0f;
                this.StopCoroutine("FadeInBattle");
                this.StopCoroutine("FadeInMain");
                this.StopCoroutine("FadeOutMusic");
                this.StartCoroutine("FadeOutMusic", 0.5f);
            }
        }

        public void StartBattleMusic()
        {
            if (MusicEnable)
            {
                this.StopCoroutine("FadeInBattle");
                this.StopCoroutine("FadeInMain");
                this.StopCoroutine("FadeOutMusic");
                this.StartCoroutine("FadeInBattle", 2f);
            }
        }

        public IEnumerator FadeInBattle(float FadeTime)
        {
            AudioSource audioSource = musicSource;
                        
            //Destroy(audioSource);

            float maxVolumn = 0.75f;
            audioSource.clip = battleClip;

            float upVolumnInFrame = maxVolumn * Time.unscaledDeltaTime / FadeTime;

            if (audioSource.isPlaying == false)
                audioSource.Play();

            while (audioSource.volume < maxVolumn)
            {
                audioSource.volume += upVolumnInFrame;
                yield return null;
            }
            audioSource.volume = maxVolumn;
        }

        public IEnumerator FadeOutMusic(float FadeTime)
        {
            AudioSource audioSource = musicSource;
            float startVolume = audioSource.volume;
            float downVolumnInFrame = startVolume * Time.unscaledDeltaTime / FadeTime;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= downVolumnInFrame;
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = 0;
        }

        public IEnumerator FadeInMain(float FadeTime)
        {
            AudioSource audioSource = musicSource;
            
            float maxVolumn = 0.85f;            

            audioSource.clip = mainClip;

            float upVolumnInFrame = maxVolumn * Time.unscaledDeltaTime / FadeTime;

            if (audioSource.isPlaying == false)
                audioSource.Play();

            while (audioSource.volume < maxVolumn)
            {
                audioSource.volume += upVolumnInFrame;
                yield return null;
            }
            audioSource.volume = maxVolumn;
        }

        Dictionary<AudioClip, float> sfxCooldown = new Dictionary<AudioClip, float>();
        //AudioClip lastClip = null;
        //float lastSfxTime = 0f;
        public AudioClip PlaySound(string path)
        {
            AudioClip clip = Resources.Load(path) as AudioClip;

            if (clip != null)
            {
                PlaySound(clip);
            }
            return clip;
        }

        public void PlaySound(AudioClip clip, float vol = 1f)
        {
            if (SoundEnable == false) return;

            var lastSfxTime = 0f;
            if (sfxCooldown.ContainsKey(clip))
            {
                lastSfxTime = sfxCooldown[clip];
            }

            if (lastSfxTime > 0) return;

            if (clip != null)
            {
                NGUITools.PlaySound(clip, vol);

                if (sfxCooldown.ContainsKey(clip))
                {
                    sfxCooldown[clip] = 0.1f;
                }
                else
                {
                    sfxCooldown.Add(clip, 0.1f);
                }
            }
        }

        private void Update()
        {
            if (sfxCooldown.Count > 0)
            {
                var listClip = Hiker.Util.ListPool<AudioClip>.Claim();
                listClip.AddRange(sfxCooldown.Keys);

                foreach (var k in listClip)
                {
                    if (k != null && sfxCooldown.ContainsKey(k))
                    {
                        var t = sfxCooldown[k];
                        t -= Time.unscaledDeltaTime;

                        if (t <= 0f) sfxCooldown.Remove(k);
                        else sfxCooldown[k] = t;
                    }
                }

                Hiker.Util.ListPool<AudioClip>.Release(listClip);
            }
        }
    }

}