using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChainIncrementController : MonoBehaviour 
{
    public float speed = 100;
    public float duration = 2f;
    public float startFadingAt = 0.5f;

    private bool active;
    private float fadingAt;
    private float fadingTime;
    private float elapsedTime;
    private Text text;
    private RectTransform trf;

	// Use this for initialization
	void Awake () 
	{
        fadingAt = duration * startFadingAt;
        fadingTime = duration - fadingAt;
        text = GetComponentInChildren<Text>();
        trf = (RectTransform)transform;
	}

    void OnEnable()
    {
        active = false;
        text.enabled = false;
    }

    public void Set(uint increment)
    {
        text.text = "+" + increment;
        text.color = Color.white;
        elapsedTime = 0f;

        float scaleFactor = 1 + ((increment - 1) / 10);
        trf.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    public void Show()
    {
        active = true;
        text.enabled = true;
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (!active) return;

	    if(elapsedTime >= duration)
        {
            //return to pool
            rsc.poolMng.comboIncrementPool.AddObject(this);
        }
        else
        {
            //Move up
            trf.Translate(0, speed * Time.deltaTime, 0);

            if (elapsedTime >= startFadingAt)
            {
                float t = fadingTime * (elapsedTime - fadingAt); 
                text.color = Color.Lerp(Color.white, Color.clear, t);
            }

            elapsedTime += Time.deltaTime;
        }
	}
}
