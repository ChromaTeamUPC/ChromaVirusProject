using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TeamSceneManager : MonoBehaviour 
{
    private enum TeamState
    {
        FADING_IN,
        TEAM,
        FADING_OUT_TO_PRESENT,
        FADING_IN_TO_PRESENT,
        PRESENT,      
        FADING_OUT
    }
    private TeamState currentState;

    public float fadeInTime = 2f;
    public float chromaTeamTime = 2f;
    public float fadeToPresentTime = 2f;
    public float presentsTime = 2f;
    public float fadeToMenuTime = 2f;

    public FadeSceneScript fadeScript;

    public GameObject teamGroupGO;
    public GameObject presentsGroupGO;

    private float elapsedTime;

    private AsyncOperation loadMainMenu;
    private AsyncOperation loadResources;
    private bool loadingResources;

    void Awake()
    {
        //Loading of managers, players, and various resources
        if (!rsc.ObjectsInitialized)
        {
            loadResources = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
            loadingResources = true;
        }
        else
        {
            loadingResources = false;
        }
    }

    // Use this for initialization
    void Start () 
	{
        teamGroupGO.SetActive(true);
        presentsGroupGO.SetActive(false);

        elapsedTime = 0f;
        currentState = TeamState.FADING_IN;

        loadMainMenu = SceneManager.LoadSceneAsync("MainMenu");
        loadMainMenu.allowSceneActivation = false;

        fadeScript.StartFadingToClear(fadeInTime);
    }
	
	// Update is called once per frame
	void Update () 
	{
        switch (currentState)
        {
            case TeamState.FADING_IN:
                if (!fadeScript.FadingToClear)
                {
                    elapsedTime = 0f;
                    currentState = TeamState.TEAM;
                }
                break;

            case TeamState.TEAM:
                if (elapsedTime >= chromaTeamTime)
                {
                    fadeScript.StartFadingToColor(fadeToPresentTime / 2);
                    currentState = TeamState.FADING_OUT_TO_PRESENT;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
                break;

            case TeamState.FADING_OUT_TO_PRESENT:
                if (!fadeScript.FadingToColor)
                {
                    teamGroupGO.SetActive(false);
                    presentsGroupGO.SetActive(true);
                    fadeScript.StartFadingToClear(fadeToPresentTime / 2);

                    currentState = TeamState.FADING_IN_TO_PRESENT;
                }
                break;

            case TeamState.FADING_IN_TO_PRESENT:
                if (!fadeScript.FadingToClear)
                {
                    elapsedTime = 0f;
                    currentState = TeamState.PRESENT;
                }
                break;

            case TeamState.PRESENT:
                if (elapsedTime >= presentsTime)
                {
                    fadeScript.StartFadingToColor(fadeToMenuTime);
                    currentState = TeamState.FADING_OUT;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
                break;

            case TeamState.FADING_OUT:
                if (!fadeScript.FadingToColor)
                {
                    loadMainMenu.allowSceneActivation = true;
                }
                break;        
        }
    }
}
