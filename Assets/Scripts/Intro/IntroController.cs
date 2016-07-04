using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using InControl;

public class IntroController : MonoBehaviour 
{
    private enum IntroState
    {
        FadingIn,
        Playing,
        FadingOut
    }

    private IntroState state;
    public GameObject movieGO;
    private MovieTexture movie;
    public FadeSceneScript fadeScript;
    public GameObject skipHint;

    private float elapsedTime;
    private float startFadingTime;

    private AsyncOperation async;

    void Awake()
    {
        movie = ((MovieTexture)movieGO.GetComponent<RawImage>().texture);
    }

	// Use this for initialization
	void Start () 
	{
        fadeScript.StartFadingToClear(1f);
        rsc.audioMng.FadeInIntroMusic(1f, 0.2f);
        state = IntroState.FadingIn;

        async = SceneManager.LoadSceneAsync("Level01");
        async.allowSceneActivation = false;

        movie.Play();
        startFadingTime = movie.duration - 1f;
        elapsedTime = 0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
        elapsedTime += Time.deltaTime;

        switch (state)
        {
            case IntroState.FadingIn:
                if (!fadeScript.FadingToClear)
                {
                    skipHint.SetActive(true);
                    state = IntroState.Playing;
                }
                break;

            case IntroState.Playing:
                int ctrlNumber = 0;
                bool processed = false;

                while (!processed && ctrlNumber < rsc.gameInfo.numberOfPlayers)
                {
                    if ((ctrlNumber < InputManager.Devices.Count && InputManager.Devices[ctrlNumber].Action2.WasPressed)
                       || elapsedTime >= startFadingTime)
                    {
                        processed = true;
                        skipHint.SetActive(false);
                        fadeScript.StartFadingToColor(1f);
                        rsc.audioMng.FadeOutIntroMusic(1f);
                        state = IntroState.FadingOut;
                    }

                    ++ctrlNumber;
                }

                break;

            case IntroState.FadingOut:
                if (!fadeScript.FadingToColor)
                {
                    //SceneManager.LoadScene("Level01");
                    async.allowSceneActivation = true;
                }
                break;

            default:
                break;
        }
    }
}
