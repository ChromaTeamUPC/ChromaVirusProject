using UnityEngine;
using System.Collections;

public class FloorController : MonoBehaviour {

    public bool colorPrewarnBlink = false;
    public float startFlashingSecondsBeforeChange = 1f;
    public float totalTimeFlashing = 0.9f;
    public float flashDuration = 0.083f;
    public float normalDuration = 0.25f;

    private ChromaColor currentColor;
    private BlinkController blinkController;
    public Material floorMaterial;
    public Material floorWhiteMat;
    public Material bridgeMaterial;

    void Start()
    {
        blinkController = GetComponent<BlinkController>();
        SetEmissionColor(Color.grey);
    }

    void OnDestroy()
    {
        SetEmissionColor(Color.grey);

        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
            if(colorPrewarnBlink)
                rsc.eventMng.StopListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        }
    }

    public void Activate()
    {
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        if (colorPrewarnBlink)
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
        StopCoroutine("ColorChangeWarning");

        Color color = rsc.coloredObjectsMng.GetFloorColor(currentColor);
        SetEmissionColor(color);
    }

    private void SetEmissionColor(Color emissionColor)
    {
        floorMaterial.SetColor("_EmissionColor", emissionColor);
        bridgeMaterial.SetColor("_EmissionColor", emissionColor);
    }

    private void ColorPrewarn(EventInfo eventInfo)
    {
        //Debug.Log("Color prewarn: " + Time.time);
        ColorPrewarnEventInfo info = (ColorPrewarnEventInfo)eventInfo;
        StopCoroutine("ColorChangeWarning");

        StartCoroutine(ColorChangeWarning(info.prewarnSeconds));
    }

    private IEnumerator ColorChangeWarning(float prewarnTime)
    {
        yield return new WaitForSeconds(prewarnTime - startFlashingSecondsBeforeChange);

        blinkController.BlinkCustomMultipleTimes(floorWhiteMat, totalTimeFlashing, flashDuration, normalDuration);
    }
}
