using UnityEngine;
using System.Collections;

public class TreesMaterialController : MonoBehaviour 
{
    public Material[] treesMat;
    public Color baseColor;

    // Use this for initialization
    void Start()
    {
        foreach (Material mat in treesMat)
            mat.SetColor("_EmissionColor", baseColor);

        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        }
    }

    private void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;

        Color color = rsc.coloredObjectsMng.GetTreeColor(info.newColor);
        foreach (Material mat in treesMat)
            mat.SetColor("_EmissionColor", color);
    }
}
