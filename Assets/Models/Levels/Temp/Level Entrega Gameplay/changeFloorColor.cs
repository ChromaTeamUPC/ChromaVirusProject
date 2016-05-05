using UnityEngine;
using System.Collections;

public class changeFloorColor : MonoBehaviour {

    public Material platform1Red;
    public Material platform1Green;
    public Material platform1Blue;
    public Material platform1Yellow;

    public Material platform1_2Red;
    public Material platform1_2Green;
    public Material platform1_2Blue;
    public Material platform1_2Yellow;

    public Material platform2Red;
    public Material platform2Green;
    public Material platform2Blue;
    public Material platform2Yellow;

    public Material platform2bRed;
    public Material platform2bGreen;
    public Material platform2bBlue;
    public Material platform2bYellow;

    public Material platform3Red;
    public Material platform3Green;
    public Material platform3Blue;
    public Material platform3Yellow;

    private ColoredObjectsManager coloredObjMng;
    private ChromaColor currentColor;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    // Use this for initialization
    void Start () {
        coloredObjMng = rsc.coloredObjectsMng;
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        currentColor = rsc.colorMng.CurrentColor;
        SetMaterial();
    }

    void ColorChanged(EventInfo eventInfo)
    {
        currentColor = ((ColorEventInfo)eventInfo).newColor;
        SetMaterial();
    }

    private void SetMaterial()
    {
        Material[] mats = rend.materials;

        switch (currentColor)
        {
            case ChromaColor.RED:
                mats[0] = platform1Red;
                mats[9] = platform2bRed;
                mats[13] = platform2Red;
                mats[14] = platform1_2Red;
                mats[19] = platform3Red;
                break;
            case ChromaColor.GREEN:
                mats[0] = platform1Green;
                mats[9] = platform2bGreen;
                mats[13] = platform2Green;
                mats[14] = platform1_2Green;
                mats[19] = platform3Green;
                break;
            case ChromaColor.BLUE:
                mats[0] = platform1Blue;
                mats[9] = platform2bBlue;
                mats[13] = platform2Blue;
                mats[14] = platform1_2Blue;
                mats[19] = platform3Blue;
                break;
            case ChromaColor.YELLOW:
                mats[0] = platform1Yellow;
                mats[9] = platform2bYellow;
                mats[13] = platform2Yellow;
                mats[14] = platform1_2Yellow;
                mats[19] = platform3Yellow;
                break;
        }

        rend.materials = mats;
    }
}
