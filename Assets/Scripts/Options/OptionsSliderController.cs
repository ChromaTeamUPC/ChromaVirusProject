using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;// Required when using Event data.
using UnityEngine.UI;

public class OptionsSliderController : MonoBehaviour, ISelectHandler, IDeselectHandler //This Interfaces are required to receive OnSelect and OnDeselect callbacks.  
{
    [SerializeField]
    private OptionsMenuManager manager;

    private Image image;

    public bool skipFirstTimeSelect = false;
    private bool firstTimeSelect = true;

    public bool skipFirstTimeSetValue = true;
    private bool firstTimeSetValue = true;
 
    void Awake()
    {
        image = transform.FindDeepChild("Fill").GetComponent<Image>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!skipFirstTimeSelect || !firstTimeSelect)
            rsc.audioMng.selectFx.Play();

        firstTimeSelect = false;

        manager.SetSelectedSliderFill(image);
    }

    public void OnDeselect(BaseEventData data)
    {
        image.color = Color.white;
    }

    public void SetValue(float value)
    {
        if (!skipFirstTimeSetValue || !firstTimeSetValue)
            rsc.audioMng.selectFx.Play();

        firstTimeSetValue = false;
    }
}
