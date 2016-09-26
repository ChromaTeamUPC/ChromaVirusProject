using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class PauseOptionsController : MonoBehaviour, ISelectHandler, IDeselectHandler //This Interfaces are required to receive OnSelect and OnDeselect callbacks. 
{
    public bool skipFirstTime = false;
    private bool firstTime = true;
    private Text text;

    void Awake()
    {
        text = GetComponentInChildren<Text>();
    }

    public void SetFirstTime()
    {
        firstTime = true;
    }

    public void OnEnable()
    {
        text.fontSize = 75;
        text.color = Color.white;
    }

    public void OnSelect(BaseEventData eventData)
    {
        text.fontSize = 100;
        text.color = rsc.coloredObjectsMng.GetColor();

        if (!skipFirstTime || !firstTime)
            rsc.audioMng.selectFx.Play();

        firstTime = false;
    }

    public void OnDeselect(BaseEventData data)
    {
        text.fontSize = 75;
        text.color = Color.white;
    }
}
