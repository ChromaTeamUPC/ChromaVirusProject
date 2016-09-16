using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossFloorController : MonoBehaviour 
{
    public bool colorPrewarnBlink = false;
    public float startFlashingSecondsBeforeChange = 1f;
    public float totalTimeFlashing = 0.9f;
    public float flashDuration = 0.083f;
    public float normalDuration = 0.25f;

    public Material hexagonMaterial;
    private ChromaColor currentColor;
    private Color color;
    private ColoredObjectsManager coloredObjMng;


    private List<HexagonController> hexagons = new List<HexagonController>();

    //Meteor rain settings;
    private Queue<HexagonController> hexagonsToCheck = new Queue<HexagonController>();
    private bool checkMeteorsEnd;
    private float meteorRainDuration;
    private int meteorInitialBurst;
    private float meteorInterval;
    private int meteorsPerInterval;
    //Meteor settings
    private float meteorWaitTime;
    private float meteorWarningTime;

    private PlayerController player1;
    private PlayerController player2;

    //TODO remove when tested
    public HexagonController hex;

    void Awake()
    {
        FindHexagonsRec(transform);
    }

    private void FindHexagonsRec(Transform trf)
    {
        if(trf.gameObject.name == "Hexagon"
            || trf.gameObject.name == "HexagonTopRight"
            || trf.gameObject.name == "HexagonTop"
            || trf.gameObject.name == "HexagonTopLeft"
            || trf.gameObject.name == "HexagonBottomLeft"
            || trf.gameObject.name == "HexagonBottom"
            || trf.gameObject.name == "HexagonBottomRight")
        {
            hexagons.Add(trf.gameObject.GetComponent<HexagonController>());
        }
        else
        {
            foreach (Transform t in trf)
                FindHexagonsRec(t);
        }
    }

    void Start()
    {
        coloredObjMng = rsc.coloredObjectsMng;
        SetColor(Color.white, Color.grey);
        player1 = rsc.gameInfo.player1Controller;
        player2 = rsc.gameInfo.player2Controller;
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
            rsc.eventMng.StopListening(EventManager.EventType.METEOR_RAIN_START, MeteorRainStart);
            if (colorPrewarnBlink)
                rsc.eventMng.StopListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        }
    }

    public void Activate()
    {
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        rsc.eventMng.StartListening(EventManager.EventType.METEOR_RAIN_START, MeteorRainStart);
        if (colorPrewarnBlink)
            rsc.eventMng.StartListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        currentColor = rsc.colorMng.CurrentColor;
        SetMaterial();
    }

    void Update()
    {
        //TODO: remove when tested
        if (Input.GetKeyDown(KeyCode.N))
        {
            hex.FallMeteor(0f, 3f);
        }        

        if(hexagonsToCheck.Count > 0)
        {
            while (hexagonsToCheck.Count > 0 && !hexagonsToCheck.Peek().IsFallingMeteor())
            {     
                hexagonsToCheck.Dequeue();
            }
        }

        if(checkMeteorsEnd && hexagonsToCheck.Count == 0)
        {
            rsc.eventMng.TriggerEvent(EventManager.EventType.METEOR_RAIN_ENDED, EventInfo.emptyInfo);
            checkMeteorsEnd = false;
        }
    }

    private void MeteorRainStart(EventInfo eventInfo)
    {
        MeteorAttackEventInfo info = (MeteorAttackEventInfo)eventInfo;
        meteorInitialBurst = info.meteorInitialBurst;
        meteorRainDuration = info.meteorRainDuration;
        meteorInterval = info.meteorInterval;
        meteorsPerInterval = info.meteorsPerInterval;

        meteorWaitTime = info.meteorWaitTime;
        meteorWarningTime = info.meteorWarningTime;

        checkMeteorsEnd = false;

        StopCoroutine(MeteorRain());
        StartCoroutine(MeteorRain());
    }

    private IEnumerator MeteorRain()
    {
        rsc.eventMng.TriggerEvent(EventManager.EventType.METEOR_RAIN_STARTED, EventInfo.emptyInfo);

        float elapsedTime = 0f;
        HexagonController hexagon;

        //Shuffle
        List<HexagonController> randomized = new List<HexagonController>(hexagons);
        randomized.Shuffle();
        randomized.Shuffle();

        int index = 0;

        for(int i = 0; i < meteorInitialBurst; ++i)
        {
            hexagon = randomized[index];
            //Used to prevent an infinite loop
            int originalIndex = index;
            bool looped = false;

            while(!hexagon.CanFallMeteor() && !looped)
            {
                index = (index + 1) % randomized.Count;
                hexagon = randomized[index];
                looped = index == originalIndex;
            }

            if (looped) break;

            hexagonsToCheck.Enqueue(hexagon);
            hexagon.FallMeteor(meteorWaitTime, meteorWarningTime);
            index = (index + 1) % randomized.Count;
        }

        yield return new WaitForSeconds(meteorInterval);

        elapsedTime += meteorInterval;

        while(elapsedTime < meteorRainDuration)
        {
            //Force meteor over player
            if(player1.Active && player1.Alive)
            {
                hexagon = player1.GetNearestHexagon();
                if(hexagon != null && hexagon.CanFallMeteor())
                {
                    hexagonsToCheck.Enqueue(hexagon);
                    hexagon.FallMeteor(meteorWaitTime, meteorWarningTime);
                }
            }

            if (player2.Active && player2.Alive)
            {
                hexagon = player2.GetNearestHexagon();
                if (hexagon != null && hexagon.CanFallMeteor())
                {
                    hexagonsToCheck.Enqueue(hexagon);
                    hexagon.FallMeteor(meteorWaitTime, meteorWarningTime);
                }
            }

            for (int i = 0; i < meteorsPerInterval; ++i)
            {
                hexagon = randomized[index];
                //Used to prevent an infinite loop
                int originalIndex = index;
                bool looped = false;

                while (!hexagon.CanFallMeteor() && !looped)
                {
                    index = (index + 1) % randomized.Count;
                    hexagon = randomized[index];
                    looped = index == originalIndex;
                }

                if (looped) break;

                hexagonsToCheck.Enqueue(hexagon);
                hexagon.FallMeteor(meteorWaitTime, meteorWarningTime);
                index = (index + 1) % randomized.Count;
            }

            yield return new WaitForSeconds(meteorInterval);

            elapsedTime += meteorInterval;
        }

        checkMeteorsEnd = true;
    }

    void ColorChanged(EventInfo eventInfo)
    {
        //Debug.Log("Color changed: " + Time.time);
        currentColor = ((ColorEventInfo)eventInfo).newColor;
        SetMaterial();
    }

    private void SetMaterial()
    {
        StopCoroutine("ColorChangeWarning");
        StopCoroutine("DoBlinkMultiple");

        color = coloredObjMng.GetColor(currentColor);
        SetColor(color);
    }

    private void SetColor(Color mainColor, Color emissionColor)
    {
        hexagonMaterial.SetColor("_Color", mainColor);
        hexagonMaterial.SetColor("_EmissionColor", emissionColor);
    }

    private void SetColor(Color mainColor)
    {
        //hexagonMaterial.SetColor("_Color", mainColor);
        hexagonMaterial.SetColor("_EmissionColor", mainColor);
    }

    private void ColorPrewarn(EventInfo eventInfo)
    {
        //Debug.Log("Color prewarn: " + Time.time);
        ColorPrewarnEventInfo info = (ColorPrewarnEventInfo)eventInfo;
        StopCoroutine("ColorChangeWarning");
        StopCoroutine("DoBlinkMultiple");

        StartCoroutine(ColorChangeWarning(info.prewarnSeconds));
    }

    private IEnumerator ColorChangeWarning(float prewarnTime)
    {
        yield return new WaitForSeconds(prewarnTime - startFlashingSecondsBeforeChange);

        StartCoroutine(DoBlinkMultiple(totalTimeFlashing, flashDuration, normalDuration));
    }

    private IEnumerator DoBlinkMultiple(float totalDuration, float blinkInterval, float normalInterval)
    {
        float elapsedTime = 0f;
        bool blink = true;

        while (elapsedTime < totalDuration)
        {
            if (blink)
            {
                SetColor(Color.white);
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }
            else
            {
                SetColor(color);
                yield return new WaitForSeconds(normalInterval);
                elapsedTime += normalInterval;
            }

            blink = !blink;
        }

        SetColor(color);
    }
}
