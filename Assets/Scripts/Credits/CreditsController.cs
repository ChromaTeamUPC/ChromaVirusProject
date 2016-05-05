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
    public AudioSource music;
    public float initialDelay = 3f;
    public float crawlingSpeed = 1f;
    public float finalDelay = 3f;

    private float volumeFadeTime;
    private float waitingTime;

	// Use this for initialization
	void Start ()
    {
        currentState = CreditsState.FadingIn;
        fadeScript.StartFadingToClear();
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
                    volumeFadeTime = 0f;
                    currentState = CreditsState.FadingOut;
                }
                break;

            case CreditsState.FadingOut:
                //Continue crawling
                //credits.Translate(Vector3.up * Time.deltaTime * crawlingSpeed);
                volumeFadeTime += Time.deltaTime;
                music.volume = Mathf.Lerp(1, 0, 1 / 4f * volumeFadeTime);

                if (!fadeScript.FadingToColor)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                break;
        }

        if (Input.GetButtonDown("Back"))
        {
            fadeScript.StartFadingToColor(Color.black, 2f);
            volumeFadeTime = 0f;
            currentState = CreditsState.FadingOut;
        }
    }
}
