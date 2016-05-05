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
    private AsyncOperation loadLevel;

    private int playersNumber = 1;

    void Awake()
    {
        //Loading of managers, players, and various resources
        loadResources = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    void Start()
    {
        DisableMainButtons();      
        currentState = MainMenuState.FadingIn;
        fadeScript.StartFadingToClear(Color.black, 1f);
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
        if (loadResources.isDone)
        {
            rsc.gameMng.StartPreloadingFirstLevel();
            DisableMainButtons();
            playerSelection.SetActive(true);
            EnablePlayerSelectionButtons();
            currentState = MainMenuState.SelectingPlayers;
        }
    }

    public void Select1Player()
    {
        playersNumber = 1;
        DisablePlayerSelectionButtons();
        currentState = MainMenuState.FadingToGame;
        fadeScript.StartFadingToColor();      
    }

    public void Select2Players()
    {
        playersNumber = 2;
        DisablePlayerSelectionButtons();
        currentState = MainMenuState.FadingToGame;
        fadeScript.StartFadingToColor();     
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
        fadeScript.StartFadingToColor();      
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
