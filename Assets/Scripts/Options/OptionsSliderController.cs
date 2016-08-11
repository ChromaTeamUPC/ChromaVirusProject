using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;// Required when using Event data.
using UnityEngine.UI;

public class OptionsSliderController : MonoBehaviour, ISelectHandler, IDeselectHandler //This Interfaces are required to receive OnSelect and OnDeselect callbacks.  
{
    [SerializeField]
    private OptionsMenuManager manager;

    private Image image;

    void Awake()
    {
        image = transform.FindDeepChild("Fill").GetComponent<Image>();
    }


    public void OnSelect(BaseEventData eventData)
    {
        manager.SetSelectedSliderFill(image);
    }

    public void OnDeselect(BaseEventData data)
    {
        image.color = Color.white;
    }
}
