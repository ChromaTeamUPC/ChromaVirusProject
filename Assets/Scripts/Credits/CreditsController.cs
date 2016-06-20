using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour {
    private enum CreditsState
    {
        FadingIn,
        WaitingTitle,
        Crawling,
        WaitingCopyRight,
        FadingOut
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
        currentState = CreditsState.FadingIn;
        fadeScript.StartFadingToClear();
        rsc.audioMng.FadeInCreditsMusic(1.5f);
	}
	
	// Update is called once per frame
	void Update () {

        switch(currentState)
        {
            case CreditsState.FadingIn:
                if (!fadeScript.FadingToClear)
                {
                    currentState = CreditsState.WaitingTitle;
                    waitingTime = 0;
                }
                break;

            case CreditsState.WaitingTitle:
                waitingTime += Time.deltaTime;
                if (waitingTime > initialDelay)
                    currentState = CreditsState.Crawling;
                break;

            case CreditsState.Crawling:
                Vector3 displacement = Vector3.up * Time.deltaTime * crawlingSpeed;
                credits.Translate(displacement);
                copyright.Translate(displacement);
                if (copyright.transform.position.y > Screen.height/2)
                {
                    currentState = CreditsState.WaitingCopyRight;
                    waitingTime = 0;
                }
                break;

            case CreditsState.WaitingCopyRight:
                credits.Translate(Vector3.up * Time.deltaTime * crawlingSpeed);
                waitingTime += Time.deltaTime;
                if (waitingTime > finalDelay)
                {
                    fadeScript.StartFadingToColor(Color.black, 4f);
                    rsc.audioMng.FadeOutCreditsMusic(4f);
                    currentState = CreditsState.FadingOut;
                }
                break;

            case CreditsState.FadingOut:
                //Continue crawling
                //credits.Translate(Vector3.up * Time.deltaTime * crawlingSpeed);

                if (!fadeScript.FadingToColor)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                break;
        }

        if (!buttonPressed && Input.GetButtonDown("Back"))
        {
            buttonPressed = true;

            if (currentState != CreditsState.FadingOut)
            {
                fadeScript.StartFadingToColor(Color.black, 2f);
                currentState = CreditsState.FadingOut;
                rsc.audioMng.FadeOutCreditsMusic(2f);
            }
        }
    }
}
