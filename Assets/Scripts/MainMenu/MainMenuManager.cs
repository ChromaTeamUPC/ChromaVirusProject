using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using InControl;

public class MainMenuManager : MonoBehaviour {

    private enum MainMenuState
    {
        FADING_IN,
        IDLE,
        SHOW_HELP,
        SELECTING_PLAYERS,
        FADING_TO_GAME,
        FADING_TO_CREDITS,
        FADING_TO_OPTIONS,
        FADING_OUT
    }
    private MainMenuState currentState;

    public FadeSceneScript fadeScript;

    public Button playBtn;
    public Button helpBtn;
    public Button optionsBtn;
    public Button creditsBtn;
    public Button exitBtn;

    public Button p1Btn;
    public Button p2Btn;

    public GameObject help;
    private Image helpImg;
    public Text helpPageNumberTxt;
    public Text levelTxt;
    public GameObject backArrow;
    public GameObject forwardArrow;
    public GameObject playerSelection;
    private AsyncOperation loadResources;
    private bool loadingResources;
    private AsyncOperation loadLevel;


    private int tutorialCurrentIndex;
    private int tutorialTotalItems;

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
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);
        helpImg = help.GetComponent<Image>();
        tutorialCurrentIndex = 0;
        tutorialTotalItems = rsc.tutorialMng.GetTotalImages();
        DisableMainButtons();      
        currentState = MainMenuState.FADING_IN;
        fadeScript.StartFadingToClear(Color.black, 1f);
        rsc.audioMng.FadeInMainMenuMusic();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            rsc.gameMng.startLevel = GameManager.Level.LEVEL_01;
            levelTxt.text = "LEVEL 01";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            rsc.gameMng.startLevel = GameManager.Level.LEVEL_BOSS;
            levelTxt.text = "LEVEL BOSS";
        }

        switch (currentState)
        {
            case MainMenuState.FADING_IN:
                if (!fadeScript.FadingToClear)
                {
                    EnableMainButtons();
                    currentState = MainMenuState.IDLE;
                }
                break;

            case MainMenuState.SHOW_HELP:
                //if(Input.GetButtonDown("Back"))
                if (InputManager.GetAnyControllerWasLeft()
                    && tutorialCurrentIndex > 0)
                {
                    tutorialCurrentIndex--;
                    SetHelpImage();
                }

                if (InputManager.GetAnyControllerWasRight()
                    && tutorialCurrentIndex < tutorialTotalItems - 1)
                {
                    tutorialCurrentIndex++;
                    SetHelpImage();
                }

                if ((InputManager.Devices.Count >= 1 && InputManager.Devices[0].Action2.WasPressed)
                    || (InputManager.Devices.Count >= 2 && InputManager.Devices[1].Action2.WasPressed))
                {
                    help.SetActive(false);
                    EnableMainButtons();
                }
                break;

            case MainMenuState.SELECTING_PLAYERS:
                //if (Input.GetButtonDown("Back"))
                if ((InputManager.Devices.Count >= 1 && InputManager.Devices[0].Action2.WasPressed)
                    || (InputManager.Devices.Count >= 2 && InputManager.Devices[1].Action2.WasPressed))
                {
                    DisablePlayerSelectionButtons();
                    playerSelection.SetActive(false);
                    EnableMainButtons();                               
                }
                break;

            case MainMenuState.FADING_TO_GAME:
                if(!fadeScript.FadingToColor)
                {
                    rsc.gameMng.StartNewGame(playersNumber);
                }
                break;

            case MainMenuState.FADING_TO_CREDITS:
                if (!fadeScript.FadingToColor)
                {
                    SceneManager.LoadScene("Credits");
                }
                break;

            case MainMenuState.FADING_TO_OPTIONS:
                if (!fadeScript.FadingToColor)
                {
                    SceneManager.LoadScene("Options");
                }
                break;
        }
    }

    private void SetHelpImage()
    {
        Sprite sprite = rsc.tutorialMng.GetImage(tutorialCurrentIndex);
        if (sprite != null)
        {
            backArrow.SetActive(tutorialCurrentIndex == 0? false : true);
            helpImg.sprite = sprite;
            helpPageNumberTxt.text = (tutorialCurrentIndex + 1) + "/" + tutorialTotalItems;
            forwardArrow.SetActive(tutorialCurrentIndex == tutorialTotalItems -1 ? false : true);
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
        optionsBtn.interactable = true;
        creditsBtn.interactable = true;
        exitBtn.interactable = true;
        playBtn.Select();
    }

    private void DisableMainButtons()
    {
        playBtn.interactable = false;
        helpBtn.interactable = false;
        optionsBtn.interactable = false;
        creditsBtn.interactable = false;
        exitBtn.interactable = false;
    }
    
    private void EnablePlayerSelectionButtons()
    {      
        p1Btn.interactable = true;
        
        //if(Input.GetJoystickNames().Length > 1)
        if(InputManager.Devices.Count > 1)
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
            DisableMainButtons();

            //if(Input.GetJoystickNames().Length > 1)
            if (InputManager.Devices.Count > 1)
            {
                playerSelection.SetActive(true);
                EnablePlayerSelectionButtons();
                currentState = MainMenuState.SELECTING_PLAYERS;
            }
            else
            {
                playersNumber = 1;
                currentState = MainMenuState.FADING_TO_GAME;
                FadeOut();
            }          
        }
    }

    public void OnClick1Player()
    {
        playersNumber = 1;
        DisablePlayerSelectionButtons();
        currentState = MainMenuState.FADING_TO_GAME;
        FadeOut();
    }

    public void OnClick2Players()
    {
        playersNumber = 2;
        DisablePlayerSelectionButtons();
        currentState = MainMenuState.FADING_TO_GAME;
        FadeOut();
    }

    public void OnClickHelp()
    {
        DisableMainButtons();
        tutorialCurrentIndex = 0;
        SetHelpImage();

        help.SetActive(true);
        currentState = MainMenuState.SHOW_HELP;
    }

    public void OnClickCredits()
    {
        DisableMainButtons();
        currentState = MainMenuState.FADING_TO_CREDITS;
        FadeOut();
    }

    public void OnClickOptions()
    {
        DisableMainButtons();
        currentState = MainMenuState.FADING_TO_OPTIONS;
        FadeOut();
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
