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
        LEVEL_BOSS_01,
        LEVEL_BOSS_02,
        LEVEL_BOSS_03,
        LEVEL_BOSS_04,
    }

    public AudioMixer audioMixer;
    private AudioSource currentMusic = null;

    public float defaultFadeSeconds = 2f;

    public float musicMaxVolume = 1f;

    [Header("Music")]
    [SerializeField]
    private AudioSource mainMenuMusic;
    [SerializeField]
    private AudioSource introMusic;
    [SerializeField]
    private AudioSource creditsMusic;
    [SerializeField]
    private AudioSource level01Music;
    [SerializeField]
    private AudioSource levelBoss01Music;
    [SerializeField]
    private AudioSource levelBoss02Music;
    [SerializeField]
    private AudioSource levelBoss03Music;
    [SerializeField]
    private AudioSource levelBoss04Music;

    [Header("Sound Fx")]
    public AudioSource AcceptFx;
    public AudioSource BackFx;
    public AudioSource SelectFx;
    public AudioSource StartFx;

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

    public void ChangeMusic(MusicType type)
    {
        ChangeMusic(type, defaultFadeSeconds);
    }

    public void ChangeMusic(MusicType type, float fadeSeconds)
    {
        StopAllCoroutines();
        AudioSource newMusic = GetAudioSource(type);
        StartCoroutine(CrossFade(newMusic, fadeSeconds));
    }

    public bool IsCurrentMusic(MusicType type)
    {
        AudioSource checkMusic = GetAudioSource(type);
        return checkMusic == currentMusic;
    }

    public bool IsMusicPlaying()
    {
        return currentMusic != null && currentMusic.isPlaying;
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
        levelBoss01Music.Stop();
        levelBoss02Music.Stop();
        levelBoss03Music.Stop();
        levelBoss04Music.Stop();
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

            case MusicType.LEVEL_BOSS_01:
                return levelBoss01Music;

            case MusicType.LEVEL_BOSS_02:
                return levelBoss02Music;

            case MusicType.LEVEL_BOSS_03:
                return levelBoss03Music;

            case MusicType.LEVEL_BOSS_04:
                return levelBoss04Music;

            default:
                return level01Music;
        }
    }

    private IEnumerator FadeMusic(float fadeSeconds, bool fadeIn, float maxVolume = 1f)
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

    private IEnumerator CrossFade(AudioSource newMusic, float fadeSeconds, float maxVolume = 1f)
    {
        float fadeSpeed;
        float fadeTime = 0f;

        if (!currentMusic.isPlaying)
        {
            currentMusic = newMusic;
        }
        else
        {
            if(fadeSeconds == 0)
            {
                currentMusic.Stop();
                currentMusic = newMusic;
                currentMusic.Play();
                currentMusic.volume = Mathf.Min(maxVolume, musicMaxVolume);
            }
            else
            {
                fadeSpeed = 1 / fadeSeconds * 2;

                float originalVolume = currentMusic.volume;
                while (currentMusic.volume > 0.025f)
                {
                    fadeTime += Time.deltaTime;
                    currentMusic.volume = originalVolume - Mathf.Lerp(0, originalVolume, fadeSpeed * fadeTime);
                    yield return null;
                }
                currentMusic.volume = 0;
                currentMusic.Stop();

                currentMusic = newMusic;
                currentMusic.Play();
                currentMusic.volume = 0;

                float actualMaxVolume = Mathf.Min(maxVolume, musicMaxVolume);

                while (currentMusic.volume < actualMaxVolume - 0.025f)
                {
                    fadeTime += Time.deltaTime;
                    currentMusic.volume = Mathf.Lerp(0, actualMaxVolume, fadeSpeed * fadeTime);
                    yield return null;
                }

                currentMusic.volume = actualMaxVolume;
            }
        }
    }

    #endregion

}
