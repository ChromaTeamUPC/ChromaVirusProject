﻿using UnityEngine;
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
    public float exclamationMarkFadeTime = 2f;
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
    public float activatingColorStartChangingAt = 1.5f;

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
    public GameObject alertArea;
    public GameObject alertCircle1;
    public GameObject alertCircle2;
    public GameObject alertTriangle;
    public GameObject exclamationMark;

    public AudioSource alarmSoundFx;
    public AudioSource wireframeToSolidSoundFx;

    public GameObject skipHint;
    public FadeSceneScript fadeScript;

    public Material backgroundMat;
    public Image background;
    public Image leftTriangles;
    public Image rightTriangles;
    public Color backgroundInitialTint;
    public Color backgroundInfectedTint;
    public Color trianglesInitialTint;
    public Color trianglesInfectedTint;

    //KDT
    [Header("KDT Objects")]
    public GameObject kdt;
    public SkinnedMeshRenderer kdtRenderer;
    public GameObject kdtShield;
    public Animator kdtAnimator;
    public ParticleSystem wireframeToSolidFx;
    public Light kdtLight;
    public Material wireBase;
    public Material wireColor;
    public Material wireFace;

    public Material matBase;
    public Material matColor;
    public Material matFace;
    public Material matShield;
    public Color matEmissionFinalColor;

    //JNK
    [Header("JNK Objects")]
    public GameObject jnk;
    public SkinnedMeshRenderer jnkRenderer;
    public GameObject jnkShield;
    public Animator jnkAnimator;
    public ParticleSystem jnkWireframeToSolidFx;
    public Light jnkLight;
    public Material jnkWireBase;
    public Material jnkWireColor;
    public Material jnkWireFace;

    public Material jnkMatBase;
    public Material jnkMatColor;
    public Material jnkMatFace;
    public Material jnkMatShield;
    public Color jnkMatEmissionFinalColor;

    //control variables
    private string loadingStr = "";
    private int loadingDots = 0;

    private float elapsedTime;

    private Coroutine blinkExclamation = null;
    private Coroutine rotateCircle = null;

    private bool multiPlayer;

	// Use this for initialization
	void Start () 
	{
        rsc.gameMng.CurrentLevel = GameManager.Level.INTRO;

        rsc.gameMng.StartLoadingNextScene(GameManager.Level.LEVEL_01);

        alertArea.SetActive(true);
        alertTriangle.SetActive(false);
        exclamationMark.SetActive(false);

        rotateCircle = StartCoroutine(RotateCircles());

        //backgroundMat.color = backgroundInitialTint;
        background.color = backgroundInitialTint;
        leftTriangles.color = trianglesInitialTint;
        rightTriangles.color = trianglesInitialTint;

        Material[] mats = kdtRenderer.sharedMaterials;
        mats[0] = wireBase;
        mats[1] = wireColor;
        mats[2] = wireFace;
        kdtRenderer.sharedMaterials = mats;
        kdt.SetActive(false);
        kdtShield.SetActive(false);

        mats = jnkRenderer.sharedMaterials;
        mats[0] = jnkWireBase;
        mats[1] = jnkWireFace;
        mats[2] = jnkWireColor;
        jnkRenderer.sharedMaterials = mats;
        jnk.SetActive(false);
        jnkShield.SetActive(false);

        skipHint.SetActive(false);

        loadingSlider.value = 0;
        percentageSlider.value = 0;

        percentageBackgroundText.text = "0%";
        percentageForegroundText.text = "0%";

        ChromaColor kdtColor = ChromaColorInfo.Random;
        matEmissionFinalColor = rsc.coloredObjectsMng.GetPlayerColor(kdtColor);
        kdtLight.color = rsc.coloredObjectsMng.GetColor(kdtColor);
        kdtLight.intensity = 0;
        kdtLight.enabled = false;

        multiPlayer = rsc.gameInfo.numberOfPlayers > 1;

        if (!multiPlayer)
        {
            kdt.transform.position = Vector3.zero;
        }
        else
        {
            ChromaColor jnkColor = ChromaColorInfo.Random;

            while(jnkColor == kdtColor)
                jnkColor = ChromaColorInfo.Random;

            jnkMatEmissionFinalColor = rsc.coloredObjectsMng.GetPlayerColor(jnkColor);
            jnkLight.color = rsc.coloredObjectsMng.GetColor(jnkColor);
            jnkLight.intensity = 0;
            jnkLight.enabled = false;
        }

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
                    //alertArea.SetActive(true);
                    alertTriangle.SetActive(true);
                    exclamationMark.SetActive(true);

                    blinkExclamation = StartCoroutine(BlinkExclamationMark());
                    //rotateCircle = StartCoroutine(RotateCircles());

                    alarmSoundFx.Play();

                    elapsedTime = 0f;

                    loadingStr = "CHROMAvIRUS DETECTED";
                    state = State.VIRUS_DETECTED;
                }
                break;

            case State.VIRUS_DETECTED:
                if(elapsedTime > virusDetectedDuration)
                {
                    StopCoroutine(blinkExclamation);
                    StopCoroutine(rotateCircle);
                    StartCoroutine(FadeAlertArea());
                    //alertArea.SetActive(false);

                    rsc.audioMng.FadeOutExternalMusic(alarmSoundFx, 1f);

                    //backgroundMat.color = backgroundInfectedTint;
                    background.color = backgroundInfectedTint;
                    leftTriangles.color = trianglesInfectedTint;
                    rightTriangles.color = trianglesInfectedTint;

                    //Set kdt
                    /*wireBase.SetFloat("_Thickness", 0f);
                    wireColor.SetFloat("_Thickness", 0f);
                    wireFace.SetFloat("_Thickness", 0f);*/
                    wireBase.SetFloat("_V_WIRE_Size", 0f);
                    wireColor.SetFloat("_V_WIRE_Size", 0f);
                    wireFace.SetFloat("_V_WIRE_Size", 0f);

                    kdt.SetActive(true);

                    //Set jnk
                    jnkWireBase.SetFloat("_V_WIRE_Size", 0f);
                    jnkWireColor.SetFloat("_V_WIRE_Size", 0f);
                    jnkWireFace.SetFloat("_V_WIRE_Size", 0f);

                    if (multiPlayer)
                        jnk.SetActive(true);

                    elapsedTime = 0f;

                    loadingStr = "STARTING KDT PROTOCOL";
                    state = State.STARTING_KDT_1;
                }
                else
                {
                    float factor = elapsedTime / virusDetectedDuration;

                    //backgroundMat.color = Color.Lerp(backgroundInitialTint, backgroundInfectedTint, factor);
                    background.color = Color.Lerp(backgroundInitialTint, backgroundInfectedTint, factor);
                    leftTriangles.color = Color.Lerp(trianglesInitialTint, trianglesInfectedTint, factor);
                    rightTriangles.color = Color.Lerp(trianglesInitialTint, trianglesInfectedTint, factor);

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
                        wireBase.SetFloat("_V_WIRE_Size", thickness);
                        wireColor.SetFloat("_V_WIRE_Size", thickness);
                        wireFace.SetFloat("_V_WIRE_Size", thickness);

                        jnkWireBase.SetFloat("_V_WIRE_Size", thickness);
                        jnkWireColor.SetFloat("_V_WIRE_Size", thickness);
                        jnkWireFace.SetFloat("_V_WIRE_Size", thickness);
                    }

                    kdt.transform.Rotate(0, Time.deltaTime * startingKdtDPS, 0);

                    jnk.transform.Rotate(0, Time.deltaTime * startingKdtDPS, 0);

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
                    wireBase.SetFloat("_V_WIRE_Size", thickness);
                    wireColor.SetFloat("_V_WIRE_Size", thickness);
                    wireFace.SetFloat("_V_WIRE_Size", thickness);

                    jnkWireBase.SetFloat("_V_WIRE_Size", thickness);
                    jnkWireColor.SetFloat("_V_WIRE_Size", thickness);
                    jnkWireFace.SetFloat("_V_WIRE_Size", thickness);

                    kdt.transform.Rotate(0, Time.deltaTime * startingKdtDPS, 0);

                    jnk.transform.Rotate(0, Time.deltaTime * startingKdtDPS, 0);

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
                    kdtLight.enabled = true;

                    jnkMatColor.SetColor("_EmissionColor", Color.black);
                    mats = jnkRenderer.sharedMaterials;
                    mats[0] = jnkMatBase;
                    mats[1] = jnkMatFace;
                    mats[2] = jnkMatColor;
                    jnkRenderer.sharedMaterials = mats;
                    jnkLight.enabled = true;

                    wireframeToSolidFx.Play();
                    if (multiPlayer) jnkWireframeToSolidFx.Play();

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

                    factor = (elapsedTime - activatingColorStartChangingAt) / (activatingColorDuration - activatingColorStartChangingAt);
                    matColor.SetColor("_EmissionColor", Color.Lerp(Color.black, matEmissionFinalColor, factor));
                    jnkMatColor.SetColor("_EmissionColor", Color.Lerp(Color.black, jnkMatEmissionFinalColor, factor));

                    kdtLight.intensity = Mathf.Lerp(0f, 1f, factor);
                    jnkLight.intensity = Mathf.Lerp(0f, 1f, factor);

                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    matShield.SetColor("_EmissionColor", matEmissionFinalColor);
                    kdtShield.SetActive(true);
                    kdtAnimator.SetBool("Aiming", true);
                    matColor.SetColor("_EmissionColor", matEmissionFinalColor);

                    jnkMatShield.SetColor("_EmissionColor", jnkMatEmissionFinalColor);
                    jnkShield.SetActive(true);
                    jnkAnimator.SetBool("Aiming", true);
                    jnkMatColor.SetColor("_EmissionColor", jnkMatEmissionFinalColor);

                    kdtLight.intensity = 1f;
                    jnkLight.intensity = 1f;

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
                    jnk.transform.Rotate(0, Time.deltaTime * startingProgramDPS, 0);

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
                    rsc.gameMng.AllowNextSceneActivation();
                }
                break;

            default:
                break;
        }

        if ((InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2) ||
            InputManager.GetAnyControllerButtonWasPressed(InputControlType.Start))
            && state != State.FADING_IN && state != State.FADING_OUT)
        {
            skipHint.SetActive(false);
            rsc.audioMng.FadeOutExternalMusic(alarmSoundFx, 0.5f);
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
            yield return new WaitForSeconds(exclamationMarkBlinkInterval);
            exclamationMark.SetActive(!exclamationMark.activeSelf);
        }
    }

    private IEnumerator RotateCircles()
    {
        while(true)
        {
            alertCircle1.transform.Rotate(0f, 0f, Time.deltaTime * alertCirclesDPS);
            alertCircle2.transform.Rotate(0f, 0f, -Time.deltaTime * alertCirclesDPS);

            yield return null;
        }
    }

    private IEnumerator FadeAlertArea()
    {
        float elapsedTime = 0f;
        float factor = 0f;
        Image alert1 = alertCircle1.GetComponent<Image>();
        Image alert2 = alertCircle2.GetComponent<Image>();
        Image hexagon = alertTriangle.GetComponent<Image>();
        Image exclamation = exclamationMark.GetComponent<Image>();
        Color color;

        while (elapsedTime < exclamationMarkFadeTime)
        {
            factor = elapsedTime / exclamationMarkFadeTime;
            color = Color.Lerp(Color.white, Color.clear, factor);
            alert1.color = color;
            alert2.color = color;
            hexagon.color = color;
            exclamation.color = color; 

            yield return null;

            elapsedTime += Time.deltaTime;
        }

        alertArea.SetActive(false);
        color = Color.white; //Reset
        alert1.color = color;
        alert2.color = color;
        hexagon.color = color;
        exclamation.color = color;
    }
}
