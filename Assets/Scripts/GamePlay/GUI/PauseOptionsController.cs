using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class PauseOptionsController : MonoBehaviour, ISelectHandler, IDeselectHandler //This Interfaces are required to receive OnSelect and OnDeselect callbacks. 
{
    private Text text;

    void Awake()
    {
        text = GetComponentInChildren<Text>();
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
    }

    public void OnDeselect(BaseEventData data)
    {
        text.fontSize = 75;
        text.color = Color.white;
    }
}
