﻿using UnityEngine;
using UnityEngine.EventSystems;
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
        FADING_TO_GAME,
        ASK_TUTORIAL,
        FADING_TO_CREDITS,
        FADING_TO_OPTIONS,
        FADING_OUT
    }
    private MainMenuState currentState;

    public float fadeInFirstTime = 2f;
    public float fadeInTime = 0.25f;
    public float fadeOutToPlayTime = 2f;
    public float fadeOutToOptionsTime = 0.25f;
    public float fadeOutToCreditsTime = 0.25f;

    public FadeSceneScript fadeScript;

    public Sprite singlePlayerIdleSprite;
    public Sprite singlePlayerSelectedSprite;
    public Sprite singlePlayerClickedSprite;
    public Sprite player1IdleSprite;
    public Sprite player1SelectedSprite;
    public Sprite player1ClickedSprite;

    public Button playBtn;
    private MainMenuButtonController playBtnController;
    public Button player2Btn;
    public Sprite player2IdleSprite;
    public Sprite player2DisabledSprite;

    public Button helpBtn;
    private MainMenuButtonController helpBtnController;
    public Button optionsBtn;
    public Button creditsBtn;
    public Button exitBtn;

    public GameObject help;
    public Image helpImg;
    public Text helpPageNumberTxt;
    public Text level01Txt;
    public Text levelBossTxt;
    public Color unselectedLevelColor;
    public GameObject backArrow;
    public GameObject forwardArrow;
    private AsyncOperation loadResources;
    private bool loadingResources;
    private AsyncOperation loadLevel;

    private int tutorialCurrentIndex;
    private int tutorialTotalItems;

    public GameObject askTutorial;
    public GameObject noController;

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

        playBtnController = playBtn.GetComponent<MainMenuButtonController>();
        helpBtnController = helpBtn.GetComponent<MainMenuButtonController>();
    }

    void Start()
    {      
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);
        tutorialCurrentIndex = 0;
        tutorialTotalItems = rsc.tutorialMng.GetTotalImages();

        SetPlayButtonImages();
        DisableMainButtons();
              
        currentState = MainMenuState.FADING_IN;
        if (rsc.gameMng.firstTime)
        {
            fadeScript.StartFadingToClear(fadeInFirstTime);
            rsc.gameMng.firstTime = false;
        }
        else
        {
            fadeScript.StartFadingToClear(fadeInTime);
        }
        if(!rsc.audioMng.IsMusicPlaying() || !rsc.audioMng.IsCurrentMusic(AudioManager.MusicType.MAIN_MENU))
            rsc.audioMng.FadeInMusic(AudioManager.MusicType.MAIN_MENU, fadeInTime);

        switch (rsc.gameMng.StartLevel)
        {
            case GameManager.Level.INTRO:
                level01Txt.color = Color.white;
                levelBossTxt.color = unselectedLevelColor;
                break;

            case GameManager.Level.LEVEL_BOSS:
                level01Txt.color = unselectedLevelColor;
                levelBossTxt.color = Color.white;
                break;
            default:
                break;
        }

        InputManager.OnDeviceAttached += OnDeviceAttached;
        InputManager.OnDeviceDetached += OnDeviceDetached;
    }

    void OnDestroy()
    {
        InputManager.OnDeviceAttached -= OnDeviceAttached;
        InputManager.OnDeviceDetached -= OnDeviceDetached;
    }

    private void OnDeviceAttached(InputDevice obj)
    {
        Debug.Log("Added controller: " + obj.Name);
        SetPlayButtonImages();
        CheckConnectedControllers();
    }
    private void OnDeviceDetached(InputDevice obj)
    {
        Debug.Log("Removed controller: " + obj.Name);
        playBtn.Select();
        SetPlayButtonImages();
        CheckConnectedControllers();
    }

    // Update is called once per frame
    void Update()
    {
        if ((InputManager.Devices.Count >= 1 && InputManager.Devices[0].Action4.WasPressed)
                    || (InputManager.Devices.Count >= 2 && InputManager.Devices[1].Action4.WasPressed))
        {
            switch (rsc.gameMng.StartLevel)
            {
                case GameManager.Level.INTRO:
                    rsc.gameMng.StartLevel = GameManager.Level.LEVEL_BOSS;
                    level01Txt.color = unselectedLevelColor;
                    levelBossTxt.color = Color.white;                   
                    break;

                case GameManager.Level.LEVEL_BOSS:
                    rsc.gameMng.StartLevel = GameManager.Level.INTRO;
                    level01Txt.color = Color.white;
                    levelBossTxt.color = unselectedLevelColor;
                    break;
                default:
                    break;
            }
        }

        if (InputManager.Devices.Count == 0 && Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        switch (currentState)
        {
            case MainMenuState.FADING_IN:
                if (!fadeScript.FadingToClear)
                {
                    EnableMainButtons();
                    CheckConnectedControllers();
                    currentState = MainMenuState.IDLE;
                }
                break;

            case MainMenuState.SHOW_HELP:
                //if(Input.GetButtonDown("Back"))
                if (InputManager.GetAnyControllerWasLeft()
                    && tutorialCurrentIndex > 0)
                {
                    rsc.audioMng.selectFx.Play();
                    tutorialCurrentIndex--;
                    SetHelpImage();
                }

                if (InputManager.GetAnyControllerWasRight()
                    && tutorialCurrentIndex < tutorialTotalItems - 1)
                {
                    rsc.audioMng.selectFx.Play();
                    tutorialCurrentIndex++;
                    SetHelpImage();
                }

                if ((InputManager.Devices.Count >= 1 && InputManager.Devices[0].Action2.WasPressed)
                    || (InputManager.Devices.Count >= 2 && InputManager.Devices[1].Action2.WasPressed))
                {
                    rsc.audioMng.backFx.Play();
                    help.SetActive(false);
                    playBtnController.SetFirstTime();
                    EnableMainButtons(helpBtn);
                }
                break;

            case MainMenuState.FADING_TO_GAME:
                if(!fadeScript.FadingToColor)
                {                  
                    if (rsc.tutorialMng.Active)
                    {
                        askTutorial.SetActive(true);
                        currentState = MainMenuState.ASK_TUTORIAL;
                    }
                    else
                        rsc.gameMng.StartNewGame(playersNumber);
                }
                break;

            case MainMenuState.ASK_TUTORIAL:
                if ((InputManager.Devices.Count >= 1 && InputManager.Devices[0].Action1.WasPressed)
                    || (InputManager.Devices.Count >= 2 && InputManager.Devices[1].Action1.WasPressed))
                {
                    rsc.audioMng.acceptFx.Play();
                    askTutorial.SetActive(false);
                    rsc.gameMng.StartNewGame(playersNumber);
                }
                else if ((InputManager.Devices.Count >= 1 && InputManager.Devices[0].Action2.WasPressed)
                    || (InputManager.Devices.Count >= 2 && InputManager.Devices[1].Action2.WasPressed))
                {
                    rsc.tutorialMng.Active = false;
                    rsc.audioMng.acceptFx.Play();
                    askTutorial.SetActive(false);
                    rsc.gameMng.StartNewGame(playersNumber);
                }
                break;

            case MainMenuState.FADING_TO_CREDITS:
                if (!fadeScript.FadingToColor)
                {
                    loadLevel.allowSceneActivation = true;
                    //SceneManager.LoadScene("Credits");
                }
                break;

            case MainMenuState.FADING_TO_OPTIONS:
                if (!fadeScript.FadingToColor)
                {
                    loadLevel.allowSceneActivation = true;
                    //SceneManager.LoadScene("Options");
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

    private void FadeOut(float fadeCurtainTime = 2f, float fadeMusicTime = 1.5f)
    {
        fadeScript.StartFadingToColor(fadeCurtainTime);
        if(fadeMusicTime != -1f)
            rsc.audioMng.FadeOutMusic(fadeMusicTime);
    }

    private void SetPlayButtonImages()
    {
        if (InputManager.Devices.Count == 1)
        {
            player2Btn.GetComponent<Image>().sprite = player2DisabledSprite;
            player2Btn.enabled = false;
        }
        else if (InputManager.Devices.Count > 1)
        {
            player2Btn.GetComponent<Image>().sprite = player2IdleSprite;
            player2Btn.enabled = true;
        }
    }

    private void CheckConnectedControllers()
    {
        if (InputManager.Devices.Count == 0)
        {
            //DisableMainButtons();
            noController.SetActive(true);
        }
        else
        {
            //EnableMainButtons();
            noController.SetActive(false);
        }
    }

    private void EnableMainButtons(Button selectedButton = null)
    {
        SetPlayButtonImages();

        playBtn.interactable = true;
        player2Btn.interactable = true;
        helpBtn.interactable = true;
        optionsBtn.interactable = true;
        creditsBtn.interactable = true;
        exitBtn.interactable = true;

        if (selectedButton != null)
            selectedButton.Select();
        else
            playBtn.Select();
    }

    private void DisableMainButtons()
    {
        playBtn.interactable = false;
        player2Btn.interactable = false;
        helpBtn.interactable = false;
        optionsBtn.interactable = false;
        creditsBtn.interactable = false;
        exitBtn.interactable = false;
    }  

    public void OnClick1Player()
    {
        if (!loadingResources || loadResources.isDone)
        {
            DisableMainButtons();
            playersNumber = 1;
            currentState = MainMenuState.FADING_TO_GAME;
            rsc.audioMng.startFx.Play();
            FadeOut(fadeOutToPlayTime, fadeOutToPlayTime);
        }
    }

    public void OnClick2Players()
    {
        if (!loadingResources || loadResources.isDone)
        {
            DisableMainButtons();
            playersNumber = 2;
            currentState = MainMenuState.FADING_TO_GAME;
            rsc.audioMng.startFx.Play();
            FadeOut(fadeOutToPlayTime, fadeOutToPlayTime);
        }
    }

    public void OnClickHelp()
    {      
        DisableMainButtons();
        tutorialCurrentIndex = 0;
        SetHelpImage();

        help.SetActive(true);
        rsc.audioMng.acceptFx.Play();
        currentState = MainMenuState.SHOW_HELP;
    }

    public void OnClickCredits()
    {
        DisableMainButtons();
        currentState = MainMenuState.FADING_TO_CREDITS;
        loadLevel = SceneManager.LoadSceneAsync("Credits");
        loadLevel.allowSceneActivation = false;
        rsc.audioMng.acceptFx.Play();
        FadeOut(fadeOutToCreditsTime, fadeOutToCreditsTime);
    }

    public void OnClickOptions()
    {
        DisableMainButtons();
        currentState = MainMenuState.FADING_TO_OPTIONS;
        loadLevel = SceneManager.LoadSceneAsync("Options");
        loadLevel.allowSceneActivation = false;
        rsc.audioMng.acceptFx.Play();
        FadeOut(fadeOutToOptionsTime, -1f);
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
