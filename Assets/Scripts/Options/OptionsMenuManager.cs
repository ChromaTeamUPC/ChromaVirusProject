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

    private Image selectedSliderFill;

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
        fadeScript.StartFadingToClear();
        rsc.audioMng.FadeInMusic(AudioManager.MusicType.CREDITS, 1.5f);

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

        if (rsc.rumbleMng.active)
            vibrationSlider.value = 1;
        else
            vibrationSlider.value = 0;

        if (rsc.gameMng.motionBlur)
            motionBlurSlider.value = 1;
        else
            motionBlurSlider.value = 0;

        if (rsc.tutorialMng.active)
            tutorialSlider.value = 1;
        else
            tutorialSlider.value = 0;
    }

    private void SaveValues()
    {
        if (vibrationSlider.value == 1)
            rsc.rumbleMng.active = true;
        else
            rsc.rumbleMng.active = false;

        if (motionBlurSlider.value == 1)
            rsc.gameMng.motionBlur = true;
        else
            rsc.gameMng.motionBlur = false;

        if (tutorialSlider.value == 1)
            rsc.tutorialMng.active = true;
        else
            rsc.tutorialMng.active = false;
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
                    SceneManager.LoadScene("MainMenu");
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
                fadeScript.StartFadingToColor(Color.black, 2f);
                currentState = CreditsState.FADING_OUT;
                rsc.audioMng.FadeOutMusic(2f);
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

        if(selectedSliderFill != null)
            selectedSliderFill.color = currentColor;
    }
}
