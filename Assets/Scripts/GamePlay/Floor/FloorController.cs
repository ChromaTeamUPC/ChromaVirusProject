using UnityEngine;
using System.Collections;

public class FloorController : MonoBehaviour {

    public Renderer rend;
    private ColoredObjectsManager coloredObjMng;
    private ChromaColor currentColor;
    private BlinkController blinkController;
    private Material whiteMat;

    void Start()
    {
        blinkController = GetComponent<BlinkController>();
        coloredObjMng = rsc.coloredObjectsMng;
        whiteMat = coloredObjMng.GetFloorWhiteMaterial();     
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
        blinkController.StopPreviousBlinkings();
        rend.sharedMaterial = coloredObjMng.GetFloorMaterial(currentColor);
        blinkController.InvalidateMaterials();
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
        yield return new WaitForSeconds(prewarnTime - 1.5f);

        blinkController.BlinkCustomMultipleTimes(whiteMat, 1.3f, 0.1f, 0.4f);
    }
}
