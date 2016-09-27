using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class NewIntroController : MonoBehaviour 
{
    private enum State
    {
        FADING_IN,
        LOADING,
        VIRUS_DETECTED,
        STARTING_KDT_1,
        STARTING_KDT_2,
        ACTIVATING_COLOR,
        STARTING_PROGRAM,
        FADING_OUT
    }

    private State state;

    [Header("Timimg settings")]
    public float initialFadeTime = 1f;
    public float loadingUntil = 20f;
    public float loadingDuration = 2f;

    public float virusDetectedDuration = 4f;
    public float exclamationMarkBlinkInterval = 0.2f;
    public float alertCirclesDPS = 45f;

    public float startingKDT1Until = 40f;
    public float startingKDT1InitialDelay = 1f;
    public float startingKDT1Duration = 4f;
    public float wireMiddleThickness = 1.5f;
    public float startingKDT2Until = 60f;
    public float startingKDT2Duration = 4f;
    public float wireMaxThickness = 15f;
    public float startingKdtDPS = -90f;

    public float activatingColorUntil = 80f;
    public float activatingColorDuration = 4f;

    public float startingProgramUntil = 100f;
    public float startingProgramDuration = 4f;
    public float startingProgramDPS = -90f;

    public float finalFadeTime = 1f;

    [Header("Scene objects references")]
    public Slider loadingSlider;
    public Slider percentageSlider;
    public Text percentageBackgroundText;
    public Text percentageForegroundText;
    public Text loadingText;
    public GameObject alertCircle;
    public GameObject alertTriangle;
    public GameObject exclamationMark;

    public GameObject kdt;
    public SkinnedMeshRenderer kdtRenderer;
    public GameObject kdtShield;
    public Animator kdtAnimator;

    public AudioSource alarmSoundFx;
    public AudioSource wireframeToSolidSoundFx;
    public ParticleSystem wireframeToSolidFx;

    public GameObject skipHint;
    public FadeSceneScript fadeScript;

    [Header("Materials")]
    public Material wireBase;
    public Material wireColor;
    public Material wireFace;

    public Material matBase;
    public Material matColor;
    public Material matFace;
    public Color matEmissionFinalColor;

    //control variables
    private string loadingStr = "";
    private int loadingDots = 0;

    private float elapsedTime;

    private AsyncOperation async;

    private Coroutine blinkExclamation = null;
    private Coroutine rotateCircle = null;

	// Use this for initialization
	void Start () 
	{
        async = SceneManager.LoadSceneAsync("Level01");
        async.allowSceneActivation = false;

        alertCircle.SetActive(false);
        alertTriangle.SetActive(false);
        exclamationMark.SetActive(false);

        Material[] mats = kdtRenderer.sharedMaterials;
        mats[0] = wireBase;
        mats[1] = wireColor;
        mats[2] = wireFace;
        kdtRenderer.sharedMaterials = mats;
        kdt.SetActive(false);
        kdtShield.SetActive(false);

        skipHint.SetActive(false);

        loadingSlider.value = 0;
        percentageSlider.value = 0;

        percentageBackgroundText.text = "0%";
        percentageForegroundText.text = "0%";

        rsc.audioMng.FadeInMusic(AudioManager.MusicType.INTRO, 1f);
        fadeScript.StartFadingToClear(initialFadeTime);
        state = State.FADING_IN;

        loadingStr = "LOADING";

        StartCoroutine(ManageLoadText());
	}
	
	// Update is called once per frame
	void Update () 
	{
        switch (state)
        {
            case State.FADING_IN:
                if (!fadeScript.FadingToClear)
                {
                    skipHint.SetActive(true);

                    elapsedTime = 0;

                    loadingStr = "LOADING";
                    state = State.LOADING;
                }
                break;

            case State.LOADING:
                if(elapsedTime < loadingDuration)
                {
                    float factor = elapsedTime / loadingDuration;
                    int percentage = Mathf.CeilToInt(Mathf.Lerp(0, loadingUntil, factor));

                    loadingSlider.value = percentage;
                    percentageSlider.value = percentage;

                    percentageBackgroundText.text = percentage + "%";
                    percentageForegroundText.text = percentage + "%";

                    elapsedTime += Time.deltaTime;
                }
                else
                {                  
                    alertCircle.SetActive(true);
                    alertTriangle.SetActive(true);

                    blinkExclamation = StartCoroutine(BlinkExclamationMark());
                    rotateCircle = StartCoroutine(RotateCircles());

                    alarmSoundFx.Play();

                    elapsedTime = 0f;

                    loadingStr = "CHROMA VIRUS DETECTED";
                    state = State.VIRUS_DETECTED;
                }
                break;

            case State.VIRUS_DETECTED:
                if(elapsedTime > virusDetectedDuration)
                {
                    StopCoroutine(blinkExclamation);
                    StopCoroutine(rotateCircle);
                    alertCircle.SetActive(false);
                    alertTriangle.SetActive(false);
                    exclamationMark.SetActive(false);

                    //Set kdt
                    wireBase.SetFloat("_Thickness", 0f);
                    wireColor.SetFloat("_Thickness", 0f);
                    wireFace.SetFloat("_Thickness", 0f);

                    kdt.SetActive(true);

                    elapsedTime = 0f;

                    loadingStr = "STARTING KDT PROTOCOL";
                    state = State.STARTING_KDT_1;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
                break;

            case State.STARTING_KDT_1:
                if(elapsedTime < startingKDT1Duration)
                {
                    float factor = elapsedTime / startingKDT1Duration;
                    int percentage = Mathf.CeilToInt(Mathf.Lerp(loadingUntil, startingKDT1Until, factor));

                    loadingSlider.value = percentage;
                    percentageSlider.value = percentage;

                    percentageBackgroundText.text = percentage + "%";
                    percentageForegroundText.text = percentage + "%";

                    if (elapsedTime >= startingKDT1InitialDelay)
                    {
                        factor = (elapsedTime - startingKDT1InitialDelay) / (startingKDT1Duration - startingKDT1InitialDelay);
                        float thickness = Mathf.Lerp(0, wireMiddleThickness, factor);
                        wireBase.SetFloat("_Thickness", thickness);
                        wireColor.SetFloat("_Thickness", thickness);
                        wireFace.SetFloat("_Thickness", thickness);
                    }

                    kdt.transform.Rotate(0, Time.deltaTime * startingKdtDPS, 0);

                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    elapsedTime = 0f;

                    state = State.STARTING_KDT_2;
                }
                break;

            case State.STARTING_KDT_2:
                if (elapsedTime < startingKDT2Duration)
                {
                    float factor = elapsedTime / startingKDT2Duration;
                    int percentage = Mathf.CeilToInt(Mathf.Lerp(startingKDT1Until, startingKDT2Until, factor));

                    loadingSlider.value = percentage;
                    percentageSlider.value = percentage;

                    percentageBackgroundText.text = percentage + "%";
                    percentageForegroundText.text = percentage + "%";

                    float thickness = Mathf.Lerp(wireMiddleThickness, wireMaxThickness, factor);
                    wireBase.SetFloat("_Thickness", thickness);
                    wireColor.SetFloat("_Thickness", thickness);
                    wireFace.SetFloat("_Thickness", thickness);

                    kdt.transform.Rotate(0, Time.deltaTime * startingKdtDPS, 0);

                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    matColor.SetColor("_EmissionColor", Color.black);
                    Material[] mats = kdtRenderer.sharedMaterials;
                    mats[0] = matBase;
                    mats[1] = matColor;
                    mats[2] = matFace;
                    kdtRenderer.sharedMaterials = mats;

                    wireframeToSolidFx.Play();
                    wireframeToSolidSoundFx.Play();

                    elapsedTime = 0f;

                    loadingStr = "ACTIVATING COLOR SYSTEM";
                    state = State.ACTIVATING_COLOR;
                }
                break;

            case State.ACTIVATING_COLOR:
                if(elapsedTime < activatingColorDuration)
                {
                    float factor = elapsedTime / activatingColorDuration;
                    int percentage = Mathf.CeilToInt(Mathf.Lerp(startingKDT2Until, activatingColorUntil, factor));

                    loadingSlider.value = percentage;
                    percentageSlider.value = percentage;

                    percentageBackgroundText.text = percentage + "%";
                    percentageForegroundText.text = percentage + "%";

                    matColor.SetColor("_EmissionColor", Color.Lerp(Color.black, matEmissionFinalColor, factor));

                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    kdtShield.SetActive(true);
                    kdtAnimator.SetBool("Aiming", true);
                    matColor.SetColor("_EmissionColor", matEmissionFinalColor);

                    elapsedTime = 0;

                    loadingStr = "STARTING PROGRAM";
                    state = State.STARTING_PROGRAM;
                }
                break;

            case State.STARTING_PROGRAM:
                if (elapsedTime < startingProgramDuration)
                {
                    float factor = elapsedTime / startingProgramDuration;
                    int percentage = Mathf.CeilToInt(Mathf.Lerp(activatingColorUntil, startingProgramUntil, factor));

                    loadingSlider.value = percentage;
                    percentageSlider.value = percentage;

                    percentageBackgroundText.text = percentage + "%";
                    percentageForegroundText.text = percentage + "%";

                    kdt.transform.Rotate(0, Time.deltaTime * startingProgramDPS, 0);

                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    skipHint.SetActive(false);
                    fadeScript.StartFadingToColor(finalFadeTime);
                    rsc.audioMng.FadeOutMusic(finalFadeTime);

                    elapsedTime = 0;

                    state = State.FADING_OUT;
                }
                break;

            case State.FADING_OUT:
                if (!fadeScript.FadingToColor)
                {
                    //SceneManager.LoadScene("Level01");
                    async.allowSceneActivation = true;
                }
                break;

            default:
                break;
        }

        if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2)
            && state != State.FADING_IN && state != State.FADING_OUT)
        {
            skipHint.SetActive(false);
            alarmSoundFx.Stop();
            wireframeToSolidSoundFx.Stop();
            rsc.audioMng.acceptFx.Play();
            fadeScript.StartFadingToColor(finalFadeTime);
            rsc.audioMng.FadeOutMusic(finalFadeTime);
            state = State.FADING_OUT;
        }
    }

    private IEnumerator ManageLoadText()
    {
        while (true)
        {
            if (loadingStr != "")
            {
                loadingText.text = loadingStr;
                for (int i = 0; i < loadingDots; ++i)
                    loadingText.text += ".";

                loadingDots = (loadingDots + 1) % 4;
            }
            else
            {
                loadingText.text = "";
                loadingDots = 0;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator BlinkExclamationMark()
    {
        while(true)
        {
            exclamationMark.SetActive(!exclamationMark.activeSelf);
            yield return new WaitForSeconds(exclamationMarkBlinkInterval);
        }
    }

    private IEnumerator RotateCircles()
    {
        while(true)
        {
            alertCircle.transform.Rotate(0f, 0f, Time.deltaTime * alertCirclesDPS);

            yield return null;
        }
    }
}
