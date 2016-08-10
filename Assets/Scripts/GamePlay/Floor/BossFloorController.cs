using UnityEngine;
using System.Collections;

public class BossFloorController : MonoBehaviour 
{
    public float startFlashingSecondsBeforeChange = 1f;
    public float totalTimeFlashing = 0.9f;
    public float flashDuration = 0.083f;
    public float normalDuration = 0.25f;

    public Material hexagonMaterial;
    private ChromaColor currentColor;
    private Color color;
    private ColoredObjectsManager coloredObjMng;

    void Start()
    {
        coloredObjMng = rsc.coloredObjectsMng;
        SetColor(Color.white, Color.grey);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        }
    }

    public void Activate()
    {
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        currentColor = rsc.colorMng.CurrentColor;
        SetMaterial();
    }

    void ColorChanged(EventInfo eventInfo)
    {
        //Debug.Log("Color changed: " + Time.time);
        currentColor = ((ColorEventInfo)eventInfo).newColor;
        SetMaterial();
    }

    private void SetMaterial()
    {        
        StopAllCoroutines();

        color = rsc.coloredObjectsMng.GetColor(currentColor);
        SetColor(color);
    }

    private void SetColor(Color mainColor, Color emissionColor)
    {
        hexagonMaterial.SetColor("_Color", mainColor);
        hexagonMaterial.SetColor("_EmissionColor", emissionColor);
    }

    private void SetColor(Color mainColor)
    {
        hexagonMaterial.SetColor("_Color", mainColor);
        hexagonMaterial.SetColor("_EmissionColor", mainColor);
    }

    private void ColorPrewarn(EventInfo eventInfo)
    {
        //Debug.Log("Color prewarn: " + Time.time);
        ColorPrewarnEventInfo info = (ColorPrewarnEventInfo)eventInfo;
        StopAllCoroutines();

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
