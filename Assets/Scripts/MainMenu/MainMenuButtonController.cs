using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class MainMenuButtonController : MonoBehaviour, ISelectHandler, IDeselectHandler //This Interfaces are required to receive OnSelect and OnDeselect callbacks. 
{
    public bool skipFirstTime = false;
    private bool firstTime = true;
    public bool scaleOnSelect = true;
    public float scaleMin = 1.0f;
    public float scaleMax = 1.1f;
    public float scaleCicleTime = 2f;
    private float scaleRange;

    private float elapsedTime = 0f;

    public void SetFirstTime()
    {
        firstTime = true;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!skipFirstTime || !firstTime)
            rsc.audioMng.selectFx.Play();

        firstTime = false;

        //transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        elapsedTime = 0f;
        StartCoroutine(Scale());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StopAllCoroutines();
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    IEnumerator Scale()
    {
        scaleRange = scaleMax - scaleMin;

        while (true)
        {
            float factor = (Mathf.Sin((elapsedTime) / scaleCicleTime * Mathf.PI * 2) + 1) / 2; //Between 0 and 1
            float newScale = scaleMin + (scaleRange * factor);
            transform.localScale = new Vector3(newScale, newScale, 1f);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

    }
}