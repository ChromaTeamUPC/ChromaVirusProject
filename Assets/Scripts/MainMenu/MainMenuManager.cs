using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour {

    private enum MainMenuState
    {
        FadingIn,
        Idle,
        ShowHelp,
        SelectingPlayers,
        FadingToGame,
        FadingToCredits,
        FadingOut
    }
    private MainMenuState currentState;

    public FadeSceneScript fadeScript;

    public Button playBtn;
    public Button helpBtn;
    public Button creditsBtn;
    public Button exitBtn;

    public Button p1Btn;
    public Button p2Btn;

    public GameObject help;
    public GameObject playerSelection;
    private AsyncOperation loadResources;
    private bool loadingResources;
    private AsyncOperation loadLevel;

    private int playersNumber = 1;

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

    void Start()
    {
        DisableMainButtons();      
        currentState = MainMenuState.FadingIn;
        fadeScript.StartFadingToClear(Color.black, 1f);
        rsc.audioMng.FadeInMainMenuMusic();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case MainMenuState.FadingIn:
                if (!fadeScript.FadingToClear)
                {
                    EnableMainButtons();
                    currentState = MainMenuState.Idle;
                }
                break;

            case MainMenuState.ShowHelp:
                if(Input.GetButtonDown("Back"))
                {
                    help.SetActive(false);
                    EnableMainButtons();
                }
                break;

            case MainMenuState.SelectingPlayers:
                if (Input.GetButtonDown("Back"))
                {
                    DisablePlayerSelectionButtons();
                    playerSelection.SetActive(false);
                    EnableMainButtons();                               
                }
                break;

            case MainMenuState.FadingToGame:
                if(!fadeScript.FadingToColor)
                {
                    rsc.gameMng.StartNewGame(playersNumber);
                }
                break;

            case MainMenuState.FadingToCredits:
                if (!fadeScript.FadingToColor)
                {
                    SceneManager.LoadScene("Credits");
                }
                break;
        }
    }
    private void FadeOut()
    {
        fadeScript.StartFadingToColor(2f);
        rsc.audioMng.FadeOutMainMenuMusic(1.5f);
    }

    private void EnableMainButtons()
    {
        playBtn.interactable = true;
        helpBtn.interactable = true;
        creditsBtn.interactable = true;
        exitBtn.interactable = true;
        playBtn.Select();
    }

    private void DisableMainButtons()
    {
        playBtn.interactable = false;
        helpBtn.interactable = false;
        creditsBtn.interactable = false;
        exitBtn.interactable = false;
    }
    
    private void EnablePlayerSelectionButtons()
    {      
        p1Btn.interactable = true;
        
        if(Input.GetJoystickNames().Length > 1)
        {
            p2Btn.interactable = true;
        }
        else
        {
            p2Btn.interactable = false;
        }

        p1Btn.Select();
    }

    private void DisablePlayerSelectionButtons()
    {
        p1Btn.interactable = false;
        p2Btn.interactable = false;
    }

    public void OnClickStart()
    {
        if (!loadingResources || loadResources.isDone)
        {
            if (Input.GetJoystickNames().Length > 1)
            {
                DisableMainButtons();
                playerSelection.SetActive(true);
                EnablePlayerSelectionButtons();
                currentState = MainMenuState.SelectingPlayers;
            }
            else
            {
                playersNumber = 1;
                currentState = MainMenuState.FadingToGame;
                FadeOut();
            }          
        }
    }

    public void OnClick1Player()
    {
        playersNumber = 1;
        DisablePlayerSelectionButtons();
        currentState = MainMenuState.FadingToGame;
        FadeOut();
    }

    public void OnClick2Players()
    {
        playersNumber = 2;
        DisablePlayerSelectionButtons();
        currentState = MainMenuState.FadingToGame;
        FadeOut();
    }

    public void OnClickHelp()
    {
        DisableMainButtons();
        help.SetActive(true);
        currentState = MainMenuState.ShowHelp;
    }

    public void OnClickCredits()
    {
        currentState = MainMenuState.FadingToCredits;
        FadeOut();
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
