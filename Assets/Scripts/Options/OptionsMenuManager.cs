using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class OptionsMenuManager : MonoBehaviour 
{
    private enum CreditsState
    {
        FADING_IN,
        IDLE,
        FADING_OUT
    }
    private CreditsState currentState;

    public float fadeInTime = 0.25f;
    public float fadeOutTime = 0.25f;

    public Color[] colors;

    public float changeEverySeconds = 3f;
    public float transitionDuration = 0.5f;

    private float changeTime = 0f;
    private int currentColorIndex;
    private Color currentColor;
    public Image img;

    public Slider musicSlider;
    public Slider fxSlider;
    public Slider vibrationSlider;
    public Slider motionBlurSlider;
    public Slider tutorialSlider;
    public Slider colorBarSlider;

    private Image selectedSliderFill;

    private AsyncOperation loadingScene; 
    public FadeSceneScript fadeScript;

    private float originalMusicVolume;
    private float originalFxVolume;

    private bool buttonPressed;

    // Use this for initialization
    void Start () 
	{
        buttonPressed = false;
        changeTime = 0f;
        currentState = CreditsState.FADING_IN;
        fadeScript.StartFadingToClear(fadeInTime);
        if(!rsc.audioMng.IsMusicPlaying())
            rsc.audioMng.FadeInMusic(AudioManager.MusicType.CREDITS, fadeInTime);

        LoadValues();

        currentColorIndex = Random.Range(0, colors.Length);
        currentColor = colors[currentColorIndex];
        SetColor();

        musicSlider.Select();
    }

    public void SetSelectedSliderFill(Image fill)
    {
        selectedSliderFill = fill;
        selectedSliderFill.color = currentColor;
    }

    private void LoadValues()
    {
        rsc.audioMng.audioMixer.GetFloat("MusicVolume", out originalMusicVolume);
        musicSlider.value = originalMusicVolume;

        rsc.audioMng.audioMixer.GetFloat("FxVolume", out originalFxVolume);
        fxSlider.value = originalFxVolume;

        vibrationSlider.value = (rsc.rumbleMng.active? 1 : 0);
        motionBlurSlider.value = (rsc.gameMng.motionBlur ? 1 : 0);
        tutorialSlider.value = (rsc.tutorialMng.active ? 1 : 0);
        colorBarSlider.value = (rsc.gameMng.colorBar ? 1 : 0);
    }

    private void SaveValues()
    {
        rsc.rumbleMng.active = (vibrationSlider.value == 1 ? true : false);
        rsc.gameMng.motionBlur = (motionBlurSlider.value == 1 ? true : false);
        rsc.tutorialMng.active = (tutorialSlider.value == 1 ? true : false);
        rsc.gameMng.colorBar = (colorBarSlider.value == 1 ? true : false);
    }


    private void RestoreValues()
    {
        rsc.audioMng.audioMixer.SetFloat("MusicVolume", originalMusicVolume);
        rsc.audioMng.audioMixer.SetFloat("FxVolume", originalFxVolume);
    }

    public void SetAudioVolume(float volume)
    {
        rsc.audioMng.audioMixer.SetFloat("MusicVolume", volume);
    }

    public void SetFxVolume(float volume)
    {
        rsc.audioMng.audioMixer.SetFloat("FxVolume", volume);
    }

    // Update is called once per frame
    void Update () 
	{
        if (changeTime >= changeEverySeconds)
        {
            ++currentColorIndex;
            if (currentColorIndex >= colors.Length)
                currentColorIndex = 0;

            StartCoroutine(ChangeColor(colors[currentColorIndex]));
            changeTime = 0f;
        }
        else
            changeTime += Time.deltaTime;

        switch (currentState)
        {
            case CreditsState.FADING_IN:
                if (!fadeScript.FadingToClear)
                {
                    currentState = CreditsState.IDLE;
                    //Set focus on first slider
                }
                break;


            case CreditsState.FADING_OUT:
                if (!fadeScript.FadingToColor)
                {
                    loadingScene.allowSceneActivation = true;
                    //SceneManager.LoadScene("MainMenu");
                }
                break;
        }


        if (!buttonPressed 
            && (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action1)
                || InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2)))
        {
            buttonPressed = true;

            if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action1))
                SaveValues();
            else
                RestoreValues();

            if (currentState != CreditsState.FADING_OUT)
            {
                loadingScene = SceneManager.LoadSceneAsync("MainMenu");
                loadingScene.allowSceneActivation = false;

                fadeScript.StartFadingToColor(fadeOutTime);
                currentState = CreditsState.FADING_OUT;
                //rsc.audioMng.FadeOutMusic(fadeOutTime);
            }
        }
    }

    private IEnumerator ChangeColor(Color to)
    {
        float elapsedTime = 0f;
        Color from = img.color;

        while (elapsedTime <= transitionDuration)
        {
            currentColor = Color.Lerp(from, to, elapsedTime * (1 / transitionDuration));
            SetColor();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        img.color = to;
    }

    private void SetColor()
    {
        img.color = currentColor;
        ColorBlock cb = musicSlider.colors;
        cb.highlightedColor = currentColor;

        musicSlider.colors = cb;
        fxSlider.colors = cb;
        vibrationSlider.colors = cb;
        motionBlurSlider.colors = cb;
        tutorialSlider.colors = cb;
        colorBarSlider.colors = cb;

        if(selectedSliderFill != null)
            selectedSliderFill.color = currentColor;
    }
}
