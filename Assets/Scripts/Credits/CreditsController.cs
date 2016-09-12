using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using InControl;

public class CreditsController : MonoBehaviour {
    private enum CreditsState
    {
        FADING_IN,
        WAITING_TITLE,
        CRAWLING,
        WAITING_COPYRIGHT,
        FADING_OUT
    }
    private CreditsState currentState;

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
        fadeScript.StartFadingToClear();
        rsc.audioMng.FadeInMusic(AudioManager.MusicType.CREDITS, 1.5f);
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
                    fadeScript.StartFadingToColor(Color.black, 4f);
                    rsc.audioMng.FadeOutMusic(4f);
                    currentState = CreditsState.FADING_OUT;
                }
                break;

            case CreditsState.FADING_OUT:
                //Continue crawling
                //credits.Translate(Vector3.up * Time.deltaTime * crawlingSpeed);

                if (!fadeScript.FadingToColor)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                break;
        }


        if (!buttonPressed && InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2)) 
        {
            buttonPressed = true;

            if (currentState != CreditsState.FADING_OUT)
            {
                fadeScript.StartFadingToColor(Color.black, 2f);
                currentState = CreditsState.FADING_OUT;
                rsc.audioMng.FadeOutMusic(2f);
            }
        }
    }
}
