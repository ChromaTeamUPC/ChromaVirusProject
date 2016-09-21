using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using InControl;

public class CreditsController : MonoBehaviour
{

    private enum CreditsState
    {
        FADING_IN,
        WAITING_TITLE,
        CRAWLING,
        WAITING_COPYRIGHT,
        FADING_OUT
    }
    private CreditsState currentState;

    public float fadeInTime = 0.25f;
    public float forcedFadeOutTime = 0.25f;
    public float endOfCreditsFadeOutTime = 4f;

    private AsyncOperation loadingScene;
    public FadeSceneScript fadeScript;

    public Transform credits;
    public Transform copyright;
    public float initialDelay = 3f;
    public float crawlingSpeed = 1f;
    public float finalDelay = 3f;

    private float waitingTime;
    private bool buttonPressed;

	// Use this for initialization
	void Start ()
    {
        buttonPressed = false;
        currentState = CreditsState.FADING_IN;
        fadeScript.StartFadingToClear(fadeInTime);
        rsc.audioMng.FadeInMusic(AudioManager.MusicType.CREDITS, fadeInTime);
	}
	
	// Update is called once per frame
	void Update () {

        switch(currentState)
        {
            case CreditsState.FADING_IN:
                if (!fadeScript.FadingToClear)
                {
                    currentState = CreditsState.WAITING_TITLE;
                    waitingTime = 0;
                }
                break;

            case CreditsState.WAITING_TITLE:
                waitingTime += Time.deltaTime;
                if (waitingTime > initialDelay)
                    currentState = CreditsState.CRAWLING;
                break;

            case CreditsState.CRAWLING:
                Vector3 displacement = Vector3.up * Time.deltaTime * crawlingSpeed;
                credits.Translate(displacement);
                copyright.Translate(displacement);

                Vector3 copyScreenPos = Camera.main.WorldToScreenPoint(copyright.transform.position);

                //if (copyright.transform.position.y > Screen.height/2)
                if (copyScreenPos.y > Screen.height / 2)
                {
                    currentState = CreditsState.WAITING_COPYRIGHT;
                    waitingTime = 0;
                }
                break;

            case CreditsState.WAITING_COPYRIGHT:
                credits.Translate(Vector3.up * Time.deltaTime * crawlingSpeed);
                waitingTime += Time.deltaTime;
                if (waitingTime > finalDelay)
                {
                    loadingScene = SceneManager.LoadSceneAsync("MainMenu");
                    loadingScene.allowSceneActivation = false;

                    fadeScript.StartFadingToColor(endOfCreditsFadeOutTime);
                    rsc.audioMng.FadeOutMusic(endOfCreditsFadeOutTime);
                    currentState = CreditsState.FADING_OUT;
                }
                break;

            case CreditsState.FADING_OUT:
                //Continue crawling
                //credits.Translate(Vector3.up * Time.deltaTime * crawlingSpeed);

                if (!fadeScript.FadingToColor)
                {
                    loadingScene.allowSceneActivation = true;
                    //SceneManager.LoadScene("MainMenu");
                }
                break;
        }


        if (enableB && !buttonPressed && InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2)) 
        {
            buttonPressed = true;

            if (currentState != CreditsState.FADING_OUT)
            {
                rsc.audioMng.BackFx.Play();
                loadingScene = SceneManager.LoadSceneAsync("MainMenu");
                loadingScene.allowSceneActivation = false;

                fadeScript.StartFadingToColor(forcedFadeOutTime);
                currentState = CreditsState.FADING_OUT;
                rsc.audioMng.FadeOutMusic(forcedFadeOutTime);
            }
        }

        CheckKC();
    }

    #region KC
    private InputControlType[] kc =
    {
        InputControlType.DPadUp,
        InputControlType.DPadUp,
        InputControlType.DPadDown,
        InputControlType.DPadDown,
        InputControlType.DPadLeft,
        InputControlType.DPadRight,
        InputControlType.DPadLeft,
        InputControlType.DPadRight,
        InputControlType.Action2,
        InputControlType.Action1
    };

    public Image ctImg;
    private int kcIndex = 0;
    private float kcMaxTime = 2f;
    private float kcTime = 0f;
    private bool enableB = true;

    private void CheckKC()
    {
        if (currentState == CreditsState.FADING_OUT) return;

        if (kcIndex == kc.Length) return;

        if(kcTime > 0)
        {
            kcTime -= Time.deltaTime;
            if(kcTime <= 0)
            {
                kcIndex = 0;
                kcTime = 0;
            }
        }
        
        if (InputManager.GetAnyControllerButtonWasPressed(kc[kcIndex]))
        {
            if (kcIndex == kc.Length - 1)
            {
                kcIndex++;
                kcTime = 0;
                rsc.audioMng.StartFx.Play();
                ctImg.enabled = true;
            }
            else
            {
                kcIndex++;
                kcTime = kcMaxTime;
            }
        }
        else
        {
            if (InputManager.GetAnyControllerAnyButtonWasPressed())
            {
                if (InputManager.GetAnyControllerButtonWasPressed(kc[0]))
                {
                    kcIndex = 1;
                    kcTime = kcMaxTime;
                }
                else
                {
                    kcIndex = 0;
                    kcTime = 0;
                }
            }
        }

        if (kcIndex == 8)
            enableB = false;
        else
            enableB = true;
    }
    #endregion
}
