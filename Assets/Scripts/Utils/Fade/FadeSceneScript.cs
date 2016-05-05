using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeSceneScript : MonoBehaviour {

    public Color defaultFadeColor = Color.black;
    public float defaultFadeSeconds = 1.5f;

    private Image curtain;
    private Color sourceColor;
    private Color targetColor;
    private float fadeTime;
    private float fadeSpeed;
    private bool fadingToColor;
    private bool fadingToClear;

    public bool FadingToColor { get { return fadingToColor; } }
    public bool FadingToClear { get { return fadingToClear; } }

    void Awake()
    {
        curtain = GetComponent<Image>();
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(fadingToClear)
        {
            FadeToClear();
        } 
        else if(fadingToColor)
        {
            FadeToColor();
        }
	}

    public void StartFadingToClear()
    {
        StartFadingToClear(defaultFadeColor, defaultFadeSeconds);
    }

    public void StartFadingToClear(Color initialColor, float fadeSeconds)
    {
        curtain.color = initialColor;
        sourceColor = initialColor;
        targetColor = Color.clear;
        curtain.enabled = true;
        if (fadeSeconds > 0)
            fadeSpeed = 1 / fadeSeconds;
        else
            fadeSpeed = float.MaxValue;
        fadeTime = 0f;
        fadingToClear = true;
    }

    public void StartFadingToColor()
    {
        StartFadingToColor(defaultFadeColor, defaultFadeSeconds);
    }

    public void StartFadingToColor(Color finalColor, float fadeSeconds)
    {
        curtain.color = Color.clear;
        sourceColor = Color.clear;
        targetColor = finalColor;
        curtain.enabled = true;
        if (fadeSeconds > 0)
            fadeSpeed = 1 / fadeSeconds;
        else
            fadeSpeed = float.MaxValue;
        fadeTime = 0f;
        fadingToColor = true;
    }

    private void FadeToClear()
    {
        fadeTime += Time.deltaTime;
        curtain.color = Color.Lerp(sourceColor, targetColor, fadeSpeed * fadeTime);

        if (curtain.color.a <= 0.05f)
        {
            curtain.color = targetColor;
            curtain.enabled = false;

            fadingToClear = false;
        }
    }

    private void FadeToColor()
    {
        fadeTime += Time.deltaTime;
        curtain.color = Color.Lerp(sourceColor, targetColor, fadeSpeed * fadeTime);

        if (curtain.color.a >= 0.95f)
        {
            curtain.color = targetColor;

            fadingToColor = false;
        }
    }
}
