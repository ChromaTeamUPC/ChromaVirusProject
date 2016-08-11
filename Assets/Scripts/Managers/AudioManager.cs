using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    public AudioMixer audioMixer;

    public float defaultFadeSeconds;

    public float musicMaxVolume = 1f;

    [SerializeField]
    private AudioSource mainMenuMusic;
    [SerializeField]
    private AudioSource introMusic;
    [SerializeField]
    private AudioSource creditsMusic;
    [SerializeField]
    private AudioSource gamePlayMusicTrak1;
    [SerializeField]
    private AudioSource gamePlayMusicTrak2;
    [SerializeField]
    private AudioSource gamePlayMusicTrak3;

    [SerializeField]
    private BeatSynchronizer beatSynchronizer;
    [SerializeField]
    private BeatCounter beatCounter;



    #region COMMON_MUSICS
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
        AudioSource music = fadingMusic;
        float fadeSpeed;
        float fadeTime = 0f;       

        if (fadeIn)
        {
            float actualMaxVolume = Mathf.Min(maxVolume, musicMaxVolume);

            if (!music.isPlaying)
            {
                music.Play();

                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;
                    music.volume = 0;

                    while (music.volume < actualMaxVolume - 0.025f) 
                    {
                        fadeTime += Time.deltaTime;
                        music.volume = Mathf.Lerp(0, actualMaxVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }

                music.volume = actualMaxVolume;
            }
        }
        else
        {
            if (music.isPlaying)
            {
                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;

                    float originalVolume = music.volume;
                    while (music.volume > 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        music.volume = originalVolume - Mathf.Lerp(0, originalVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }
                music.volume = 0;
                music.Stop();
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
        gamePlayMusicTrak1.Pause();
        //gamePlayMusicTrak2.Pause();
        //gamePlayMusicTrak3.Pause();
    }

    public void ResumeMainMusic()
    {
        gamePlayMusicTrak1.UnPause();
        //gamePlayMusicTrak2.UnPause();
        //gamePlayMusicTrak3.UnPause();
    }

    public void StopMainMusic()
    {
        gamePlayMusicTrak1.Stop();
        //gamePlayMusicTrak2.Stop();
        //gamePlayMusicTrak3.Stop();
    }

    IEnumerator FadeMainMusic(float fadeSeconds, bool fadeIn)
    {
        float fadeSpeed;
        float fadeTime = 0f;

        if (fadeIn)
        {
            if (!gamePlayMusicTrak1.isPlaying)
            {
                beatCounter.enabled = true;
                beatSynchronizer.PlayMusic(); //Main track, with beat synchronizer
                //gamePlayMusicTrak2.Play();
                //gamePlayMusicTrak3.Play();

                //gamePlayMusicTrak2.volume = 0;
                //gamePlayMusicTrak3.volume = 0;

                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;
                    gamePlayMusicTrak1.volume = 0;

                    while (gamePlayMusicTrak1.volume < musicMaxVolume - 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        gamePlayMusicTrak1.volume = Mathf.Lerp(0, musicMaxVolume, fadeSpeed * fadeTime);
                        yield return null;
                    }
                }

                gamePlayMusicTrak1.volume = musicMaxVolume;
            }
        }
        else
        {
            if (gamePlayMusicTrak1.isPlaying)
            {
                if (fadeSeconds > 0)
                {
                    fadeSpeed = 1 / fadeSeconds;

                    float originalVolumeT1 = gamePlayMusicTrak1.volume;
                    //float originalVolumeT2 = gamePlayMusicTrak2.volume;
                    //float originalVolumeT3 = gamePlayMusicTrak3.volume;

                    while (gamePlayMusicTrak1.volume > 0.025f)
                    {
                        fadeTime += Time.deltaTime;
                        float lerpValue = Mathf.Lerp(0, originalVolumeT1, fadeSpeed * fadeTime);
                        gamePlayMusicTrak1.volume = originalVolumeT1 - lerpValue;

                        //gamePlayMusicTrak2.volume = originalVolumeT2 - lerpValue;
                        //gamePlayMusicTrak3.volume = originalVolumeT3 - lerpValue;
                        yield return null;
                    }
                }
                gamePlayMusicTrak1.volume = 0;
                //gamePlayMusicTrak2.volume = 0;
                //gamePlayMusicTrak3.volume = 0;

                gamePlayMusicTrak1.Stop();
                //gamePlayMusicTrak2.Stop();
                //gamePlayMusicTrak3.Stop();

                beatCounter.enabled = false;
            }
        }
    }
    #endregion
}
