using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    public enum MusicType
    {
        MAIN_MENU,
        INTRO,
        CREDITS,
        LEVEL_01,
        LEVEL_BOSS
    }

    public AudioMixer audioMixer;
    private AudioSource currentMusic = null;

    public float defaultFadeSeconds = 2f;

    public float musicMaxVolume = 1f;

    [SerializeField]
    private AudioSource mainMenuMusic;
    [SerializeField]
    private AudioSource introMusic;
    [SerializeField]
    private AudioSource creditsMusic;
    [SerializeField]
    private AudioSource level01Music;
    [SerializeField]
    private AudioSource levelBossMusic;

    [SerializeField]
    private BeatSynchronizer beatSynchronizer;
    [SerializeField]
    private BeatCounter beatCounter;


    #region MUSIC_CONTROL
    public void FadeInMusic(MusicType type)
    {
        FadeInMusic(type, defaultFadeSeconds);
    }

    public void FadeInMusic(MusicType type, float fadeSeconds)
    {
        StopAllCoroutines();
        EnsureAllMusicsStopped();
        currentMusic = GetAudioSource(type);
        StartCoroutine(FadeMusic(fadeSeconds, true));
    }

    public void FadeOutMusic()
    {
        FadeOutMusic(defaultFadeSeconds);
    }

    public void FadeOutMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(fadeSeconds, false));
    }

    public void PauseMusic()
    {
        if(currentMusic !=  null)
            currentMusic.Pause();
    }

    public void ResumeMusic()
    {
        if (currentMusic != null)
            currentMusic.UnPause();
    }

    public void StopMusic()
    {
        if (currentMusic != null)
            currentMusic.Stop();
    }

    private void EnsureAllMusicsStopped()
    {
        mainMenuMusic.Stop();
        introMusic.Stop();
        creditsMusic.Stop();
        level01Music.Stop();
        levelBossMusic.Stop();
    }

    private AudioSource GetAudioSource(MusicType type)
    {
        switch (type)
        {
            case MusicType.MAIN_MENU:
                return mainMenuMusic;

            case MusicType.INTRO:
                return introMusic;

            case MusicType.CREDITS:
                return creditsMusic;

            case MusicType.LEVEL_01:
                return level01Music;

            case MusicType.LEVEL_BOSS:
                return levelBossMusic;

            default:
                return level01Music;
        }
    }

    IEnumerator FadeMusic(float fadeSeconds, bool fadeIn, float maxVolume = 1f)
    {      
        float fadeSpeed;
        float fadeTime = 0f;

        if (fadeIn)
        {
            float actualMaxVolume = Mathf.Min(maxVolume, musicMaxVolume);

            if (!currentMusic.isPlaying)
            {
                currentMusic.Play();

                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;
                    currentMusic.volume = 0;

                    while (currentMusic.volume < actualMaxVolume - 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        currentMusic.volume = Mathf.Lerp(0, actualMaxVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }

                currentMusic.volume = actualMaxVolume;
            }
        }
        else
        {
            if (currentMusic.isPlaying)
            {
                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;

                    float originalVolume = currentMusic.volume;
                    while (currentMusic.volume > 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        currentMusic.volume = originalVolume - Mathf.Lerp(0, originalVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }
                currentMusic.volume = 0;
                currentMusic.Stop();
            }
        }
    }
    #endregion


    /*#region COMMON_MUSICS
    //Main menu music
    public void FadeInMainMenuMusic()
    {
        FadeInMainMenuMusic(defaultFadeSeconds);
    }

    public void FadeInMainMenuMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(mainMenuMusic, fadeSeconds, true));
    }

    public void FadeOutMainMenuMusic()
    {
        FadeOutMainMenuMusic(defaultFadeSeconds);
    }

    public void FadeOutMainMenuMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(mainMenuMusic, fadeSeconds, false));
    }

    //Intro music
    public void FadeInIntroMusic()
    {
        FadeInIntroMusic(defaultFadeSeconds);
    }

    public void FadeInIntroMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(introMusic, fadeSeconds, true));
    }

    public void FadeInIntroMusic(float fadeSeconds, float maxVolume)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(introMusic, fadeSeconds, true, maxVolume));
    }

    public void FadeOutIntroMusic()
    {
        FadeOutIntroMusic(defaultFadeSeconds);
    }

    public void FadeOutIntroMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(introMusic, fadeSeconds, false));
    }

    //Credits music
    public void FadeInCreditsMusic()
    {
        FadeInCreditsMusic(defaultFadeSeconds);
    }

    public void FadeInCreditsMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(creditsMusic, fadeSeconds, true));
    }

    public void FadeOutCreditsMusic()
    {
        FadeOutCreditsMusic(defaultFadeSeconds);
    }

    public void FadeOutCreditsMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMusic(creditsMusic, fadeSeconds, false));
    }

    IEnumerator FadeMusic(AudioSource fadingMusic, float fadeSeconds, bool fadeIn, float maxVolume = 1f)
    {
        currentMusic = fadingMusic;
        float fadeSpeed;
        float fadeTime = 0f;       

        if (fadeIn)
        {
            float actualMaxVolume = Mathf.Min(maxVolume, musicMaxVolume);

            if (!currentMusic.isPlaying)
            {
                currentMusic.Play();

                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;
                    currentMusic.volume = 0;

                    while (currentMusic.volume < actualMaxVolume - 0.025f) 
                    {
                        fadeTime += Time.deltaTime;
                        currentMusic.volume = Mathf.Lerp(0, actualMaxVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }

                currentMusic.volume = actualMaxVolume;
            }
        }
        else
        {
            if (currentMusic.isPlaying)
            {
                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;

                    float originalVolume = currentMusic.volume;
                    while (currentMusic.volume > 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        currentMusic.volume = originalVolume - Mathf.Lerp(0, originalVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }
                currentMusic.volume = 0;
                currentMusic.Stop();
            }
        }
    }

    #endregion

    #region MAIN_MUSIC
    public void FadeInMainMusic()
    {
        FadeInMainMusic(defaultFadeSeconds);
    }

    public void FadeInMainMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMainMusic(fadeSeconds, true));
    }

    public void FadeOutMainMusic()
    {
        FadeOutMainMusic(defaultFadeSeconds);
    }

    public void FadeOutMainMusic(float fadeSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMainMusic(fadeSeconds, false));
    }

    public void PauseMainMusic()
    {
        level01Music.Pause();
    }

    public void ResumeMainMusic()
    {
        level01Music.UnPause();
    }

    public void StopMainMusic()
    {
        level01Music.Stop();
    }

    IEnumerator FadeMainMusic(float fadeSeconds, bool fadeIn)
    {
        float fadeSpeed;
        float fadeTime = 0f;

        if (fadeIn)
        {
            if (!level01Music.isPlaying)
            {
                beatCounter.enabled = true;
                beatSynchronizer.PlayMusic(); //Main track, with beat synchronizer

                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;
                    level01Music.volume = 0;

                    while (level01Music.volume < musicMaxVolume - 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        level01Music.volume = Mathf.Lerp(0, musicMaxVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }

                level01Music.volume = musicMaxVolume;
            }
        }
        else
        {
            if (level01Music.isPlaying)
            {
                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;

                    float originalVolumeT1 = level01Music.volume;

                    while (level01Music.volume > 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        float lerpValue = Mathf.Lerp(0, originalVolumeT1, fadeSpeed * fadeTime);
                        level01Music.volume = originalVolumeT1 - lerpValue;

                        yield return null;
                    }
                }
                level01Music.volume = 0;

                level01Music.Stop();

                beatCounter.enabled = false;
            }
        }
    }
    #endregion*/
}
