using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class MainMenuButtonController : MonoBehaviour, ISelectHandler //This Interfaces are required to receive OnSelect and OnDeselect callbacks. 
{
    public bool skipFirstTime = false;
    private bool firstTime = true;

    public void SetFirstTime()
    {
        firstTime = true;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!skipFirstTime || !firstTime)
            rsc.audioMng.SelectFx.Play();

        firstTime = false;
    }
}