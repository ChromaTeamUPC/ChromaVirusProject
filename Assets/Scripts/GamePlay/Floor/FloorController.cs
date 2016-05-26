using UnityEngine;
using System.Collections;

public class FloorController : MonoBehaviour {

    public Renderer rend;
    private ColoredObjectsManager coloredObjMng;
    private ChromaColor currentColor;

    void Start()
    {
        coloredObjMng = rsc.coloredObjectsMng;
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        currentColor = rsc.colorMng.CurrentColor;
        SetMaterial();
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        }
    }

    void ColorChanged(EventInfo eventInfo)
    {
        currentColor = ((ColorEventInfo)eventInfo).newColor;
        SetMaterial();
    }

    private void SetMaterial()
    {
        Material mat = coloredObjMng.GetFloorMaterial(currentColor);

        rend.material = mat;
    }
}
