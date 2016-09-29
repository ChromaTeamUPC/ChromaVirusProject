using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DeviceController : MonoBehaviour
{
    public const int globalInfectionValue = 5;

    public enum Type
    {
        VIDEO,
        AUDIO,
        INPUT
    }

    public enum InfectionLevel
    {
        LEVEL0,
        LEVEL1,
        LEVEL2,
        LEVEL3
    }

    private bool active;
    private float currentInfection = 0f;
    private InfectionLevel infectionLevel;

    public Type type;
    public float level0To1Threshold = 33;
    public Color colorOnLevel0To1;
    public float level1To2Threshold = 66;
    public Color colorOnLevel1To2;
    public float level2To3Threshold = 99;
    public Color colorOnLevel2To3;

    public float infectionPerAttack = 10;
    public float disinfectionPerSecond = 33f;

    public GameObject[] endPoints;

    public Slider infectionBar;
    public Image infectionBarFillImg;
    public Text infectionNumberTxt;
    private Material infectionNumberMat;
    private Color currentColor;
    private float currentBrightness;
    public float brightnessCicleDuration;
    private float brightnessSpeed;

    public AudioSource disinfectSoundFx;
    private bool shouldDisinfectSoundBePlayed;

    private BlinkController blinkController;

    private bool tutorialTriggered;

    public float CurrentGlobalInfectionValue { get { return (globalInfectionValue * (currentInfection / 100)); } }

    public float CurrentInfectionAmount { get { return currentInfection; } }
    public InfectionLevel CurrentInfectionLevel { get { return infectionLevel; } }

    void Awake()
    {
        active = false;

        blinkController = GetComponent<BlinkController>();

        currentInfection = 0f;
        infectionLevel = InfectionLevel.LEVEL0;
        infectionNumberMat = infectionNumberTxt.material;
        currentBrightness = 1f;
        currentColor = Color.white;
        infectionNumberTxt.text = "0%";
        infectionBar.value = 0;


        if (brightnessCicleDuration > 0)
            brightnessSpeed = 1 / brightnessCicleDuration;
        else
            brightnessSpeed = 1;

        tutorialTriggered = false;

        SetColor();
    }

    private void SetColor()
    {
        currentColor *= currentBrightness;
        currentColor.a = 1f;

        infectionNumberMat.SetColor("_EmissionColor", currentColor);

        infectionBarFillImg.color = currentColor;
    }

    public void Activate()
    {
        active = true;
        DeviceEventInfo.eventInfo.device = this;
        rsc.eventMng.TriggerEvent(EventManager.EventType.DEVICE_ACTIVATED, DeviceEventInfo.eventInfo);
    }

    public void Deactivate()
    {
        DeviceEventInfo.eventInfo.device = this;
        rsc.eventMng.TriggerEvent(EventManager.EventType.DEVICE_DEACTIVATED, DeviceEventInfo.eventInfo);
        active = false;
    }   

    public void Infect()
    {
        if (!active) return;

        if(!tutorialTriggered)
        {
            tutorialTriggered = true;
            TutorialEventInfo.eventInfo.type = TutorialManager.Type.DEVICE;
            rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
        }

        if (currentInfection < 100f)
        {
            blinkController.BlinkWhiteOnce(0.02f);

            currentInfection += infectionPerAttack;

            if (currentInfection > 100f)
                currentInfection = 100f;
        }
    }

    public void Disinfect()
    {
        if (!active) return;

        if (currentInfection > 0)
        {
            if(!disinfectSoundFx.isPlaying)
            {
                disinfectSoundFx.Play();
            }
            shouldDisinfectSoundBePlayed = true;

            currentInfection -= disinfectionPerSecond * Time.deltaTime;
            if (currentInfection < 0f)
                currentInfection = 0f;
        }
    }

    void Update()
    {
        if (!active) return;

        if (!shouldDisinfectSoundBePlayed && disinfectSoundFx.isPlaying)
        {
            disinfectSoundFx.Stop();
        }

        infectionNumberTxt.text = (Mathf.CeilToInt(currentInfection)) + "%";
        infectionBar.value = Mathf.CeilToInt(currentInfection);
        currentBrightness = (Mathf.Sin(Time.time * Mathf.PI * brightnessSpeed) / 2) + 1; //Values between 0.5 and 1.5

        switch (infectionLevel)
        {
            case InfectionLevel.LEVEL0:
                currentColor = Color.Lerp(Color.white, colorOnLevel0To1, currentInfection / level0To1Threshold);
                if(currentInfection >= level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL3;
                    ManageInfectionChange();
                }
                else if (currentInfection >= level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL2;
                    ManageInfectionChange();
                }
                else if (currentInfection >= level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL1;
                    ManageInfectionChange();
                }
                break;

            case InfectionLevel.LEVEL1:
                currentColor = Color.Lerp(colorOnLevel0To1, colorOnLevel1To2, (currentInfection - level0To1Threshold) / (level1To2Threshold - level0To1Threshold));
                if (currentInfection >= level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL3;
                    ManageInfectionChange();
                }
                else if (currentInfection >= level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL2;
                    ManageInfectionChange();
                }
                else if (currentInfection < level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL0;
                    ManageInfectionChange();
                }
                break;

            case InfectionLevel.LEVEL2:
                currentColor = Color.Lerp(colorOnLevel1To2, colorOnLevel2To3, (currentInfection - level1To2Threshold) / (level2To3Threshold - level1To2Threshold));
                if (currentInfection >= level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL3;
                    ManageInfectionChange();
                }
                else if (currentInfection < level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL0;
                    ManageInfectionChange();
                }
                else if (currentInfection < level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL1;
                    ManageInfectionChange();
                }
                break;

            case InfectionLevel.LEVEL3:
                currentColor = colorOnLevel2To3;
                if (currentInfection < level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL0;
                    ManageInfectionChange();
                }
                else if (currentInfection < level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL1;
                    ManageInfectionChange();
                }
                else if (currentInfection < level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL2;
                    ManageInfectionChange();
                }
                break;
            default:
                break;
        }

        SetColor();

        shouldDisinfectSoundBePlayed = false;
    }

    public GameObject GetRandomEndPoint()
    {
        if (endPoints.Length > 0)
            return endPoints[Random.Range(0, endPoints.Length)];
        else
            return gameObject;
    }

    private void ManageInfectionChange()
    {
        StopAllCoroutines();
        StopInfectionEffects();

        switch (infectionLevel)
        {
            case InfectionLevel.LEVEL1:
                StartCoroutine(ManageInfectionEffects(1f, 3f));
                break;
            case InfectionLevel.LEVEL2:
                StartCoroutine(ManageInfectionEffects(2f, 2f));
                break;
            case InfectionLevel.LEVEL3:
                StartCoroutine(ManageInfectionEffects(3f, 1f, true));
                break;
            default:
                break;
        }
    }
    private void StopInfectionEffects()
    {
        rsc.camerasMng.RemoveContinousEffect(EffectId.DEVICE_INFECTION, 0);
    }

    private IEnumerator ManageInfectionEffects(float timeEnabled, float timeDisabled, bool keepNoise = false)
    {
        while (true)
        {
            if (keepNoise)
                rsc.camerasMng.RemoveContinousEffect(EffectId.DEVICE_INFECTION, 0);
            rsc.camerasMng.AddContinousEffect(EffectId.DEVICE_INFECTION, 0, 0, Effects.BN | Effects.NOISE);
            yield return new WaitForSeconds(timeEnabled);
            rsc.camerasMng.RemoveContinousEffect(EffectId.DEVICE_INFECTION, 0);
            if (keepNoise)
                rsc.camerasMng.AddContinousEffect(EffectId.DEVICE_INFECTION, 0, 0, Effects.NOISE);
            yield return new WaitForSeconds(timeDisabled);
        }
    }  
}
