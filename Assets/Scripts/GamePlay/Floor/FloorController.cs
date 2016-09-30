using UnityEngine;
using System.Collections;

public class FloorController : MonoBehaviour {

    public bool colorPrewarnBlink = false;
    public float startFlashingSecondsBeforeChange = 1f;
    public float totalTimeFlashing = 0.9f;
    public float flashDuration = 0.083f;
    public float normalDuration = 0.25f;

    public Renderer rend;
    private ColoredObjectsManager coloredObjMng;
    private ChromaColor currentColor;
    private Color color;
    private BlinkController blinkController;
    private Material whiteMat;
    public Material hexagonMaterial;

    void Start()
    {
        blinkController = GetComponent<BlinkController>();
        coloredObjMng = rsc.coloredObjectsMng;
        whiteMat = coloredObjMng.GetFloorWhiteMaterial();
        hexagonMaterial.SetColor("_Color", Color.white);
        hexagonMaterial.SetColor("_EmissionColor", Color.grey);
    }

    void OnDestroy()
    {
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
        blinkController.StopPreviousBlinkings();
        rend.sharedMaterial = coloredObjMng.GetFloorMaterial(currentColor);
        blinkController.InvalidateMaterials();

        color = coloredObjMng.GetColor(currentColor);
        hexagonMaterial.SetColor("_EmissionColor", color);
    }

    private void ColorPrewarn(EventInfo eventInfo)
    {
        //Debug.Log("Color prewarn: " + Time.time);
        ColorPrewarnEventInfo info = (ColorPrewarnEventInfo)eventInfo;
        StopAllCoroutines();
        blinkController.StopPreviousBlinkings();

        StartCoroutine(ColorChangeWarning(info.prewarnSeconds));
    }

    private IEnumerator ColorChangeWarning(float prewarnTime)
    {
        yield return new WaitForSeconds(prewarnTime - startFlashingSecondsBeforeChange);

        blinkController.BlinkCustomMultipleTimes(whiteMat, totalTimeFlashing, flashDuration, normalDuration);
    }
}
