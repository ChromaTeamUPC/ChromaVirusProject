using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using InControl;

public class GameManager : MonoBehaviour {

    public bool firstTime = true;
    public bool motionBlur = true;
    public bool colorBar = false;

    public enum Level
    {
        LEVEL_01,
        LEVEL_BOSS,
        INTRO
    }

    public enum GameState
    {
        NOT_STARTED,
        STARTED,
        PAUSED,
        SHOWING_TUTORIAL,
        OPENING_SCORE,
        SHOWING_SCORE
    }

    private GameState state;

    private Level startLevel = Level.INTRO;
    private Level currentLevel;

    private AsyncOperation nextLevel;

    public GameState State { get { return state; } }

    public Level StartLevel { get { return startLevel; } set { startLevel = value; } }
    public Level CurrentLevel { get { return currentLevel; } set { currentLevel = value; } }

    void Awake()
    {
        state = GameState.NOT_STARTED;
    }

    // Use this for initialization
    void Start ()
    {
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
        rsc.eventMng.StartListening(EventManager.EventType.SCORE_OPENING, ScoreOpening);
        rsc.eventMng.StartListening(EventManager.EventType.SCORE_OPENED, ScoreOpened);
        rsc.eventMng.StartListening(EventManager.EventType.SCORE_CLOSED, ScoreClosed);
        //rsc.eventMng.StartListening(EventManager.EventType.SHOW_SCORE, LevelCleared); //TODo change proper event
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
            rsc.eventMng.StopListening(EventManager.EventType.SCORE_OPENING, ScoreOpening);
            rsc.eventMng.StopListening(EventManager.EventType.SCORE_OPENED, ScoreOpened);
            rsc.eventMng.StopListening(EventManager.EventType.SCORE_CLOSED, ScoreClosed);
            //rsc.eventMng.StopListening(EventManager.EventType.SHOW_SCORE, LevelCleared);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (state == GameState.NOT_STARTED) return;

        switch (state)
        {
            case GameState.STARTED:
                if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Start) && currentLevel != Level.INTRO)
                    Pause();
                break;

            case GameState.PAUSED:
                if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Start))
                    Resume();
                break;

            case GameState.SHOWING_TUTORIAL:
                if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action1))
                    CloseTutorial();
                else if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action4))
                    CloseTutorial(true);
                break;

            case GameState.OPENING_SCORE:
                break;

            case GameState.SHOWING_SCORE:
                if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action1))
                    CloseScore();
                break;

            default:
                break;
        }
	}

    public bool IsPaused()
    {
        return state == GameState.PAUSED || state == GameState.SHOWING_TUTORIAL;
    }

    public void Pause()
    {
        if (state != GameState.STARTED) return;

        state = GameState.PAUSED;
        Time.timeScale = 0.000000000001f;
        rsc.audioMng.PauseMusic();
        AudioListener.pause = true;
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_PAUSED, EventInfo.emptyInfo);
    }

    public void Resume()
    {
        if (state != GameState.PAUSED) return;

        state = GameState.STARTED;
        Time.timeScale = 1f;
        rsc.audioMng.ResumeMusic();
        AudioListener.pause = false;
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESUMED, EventInfo.emptyInfo);
    }

    public void TutorialOpened(EventInfo eventInfo)
    {
        state = GameState.SHOWING_TUTORIAL;
        Time.timeScale = 0.000000000001f;
        AudioListener.pause = true;
    }

    public void CloseTutorial(bool disableTutorial = false)
    {
        if (state != GameState.SHOWING_TUTORIAL) return;

        state = GameState.STARTED;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (disableTutorial)
            rsc.tutorialMng.active = false;
        rsc.eventMng.TriggerEvent(EventManager.EventType.HIDE_TUTORIAL, EventInfo.emptyInfo);
    }

    public void ScoreOpening(EventInfo eventInfo)
    {
        state = GameState.OPENING_SCORE;
    }

    public void ScoreOpened(EventInfo eventInfo)
    {
        state = GameState.SHOWING_SCORE;
    }

    public void CloseScore()
    {
        if (state != GameState.SHOWING_SCORE) return;

        rsc.eventMng.TriggerEvent(EventManager.EventType.HIDE_SCORE, EventInfo.emptyInfo);      
    }

    public void ScoreClosed(EventInfo eventInfo)
    {
        state = GameState.STARTED;
        switch (currentLevel)
        {
            case Level.LEVEL_01: //Level01
                StartCoroutine(GoToNextLevel(Level.LEVEL_BOSS));
                break;

            case Level.LEVEL_BOSS: //Boss
                GameFinished();
                break;
        }
    }

    public void SetGameStartedDEBUG()
    {
        state = GameState.STARTED;

        Debug.Log(SceneManager.GetActiveScene().name + " scene loaded");

        Debug.Log("WARNING: gameStarted set to true through a debug function!");
    }

    public void StartNewGame(int numPlayers)
    {
        InitPlayers(numPlayers);

        state = GameState.STARTED;

        switch (startLevel)
        {
            case Level.INTRO:
                SceneManager.LoadScene("Intro");
                break;
            case Level.LEVEL_01:
                SceneManager.LoadScene("Level01");
                break;
            case Level.LEVEL_BOSS:
                SceneManager.LoadScene("LevelBoss");
                break;
            default:
                break;
        }
    }

    public void StartLoadingNextScene(Level level)
    {
        switch (level)
        {
            case Level.INTRO:
                nextLevel = SceneManager.LoadSceneAsync("Intro");
                break;
            case Level.LEVEL_01:
                nextLevel = SceneManager.LoadSceneAsync("Level01");
                break;
            case Level.LEVEL_BOSS:
                nextLevel = SceneManager.LoadSceneAsync("LevelBoss");
                break;
            default:
                break;
        }
        nextLevel.allowSceneActivation = false;
    }

    public void AllowNextSceneActivation()
    {
        nextLevel.allowSceneActivation = true;
    }

    public void InitPlayers(int numPlayers)
    {
        rsc.gameInfo.numberOfPlayers = numPlayers;

        //Force player scripts to Awake
        if (!rsc.gameInfo.player1Controller.Initialized)
        {
            rsc.gameInfo.player1.SetActive(true);
            rsc.gameInfo.player1.SetActive(false);
        }

        if (!rsc.gameInfo.player2Controller.Initialized)
        {
            rsc.gameInfo.player2.SetActive(true);
            rsc.gameInfo.player2.SetActive(false);
        }

        rsc.gameInfo.player1Controller.Reset();
        rsc.gameInfo.player1Controller.Active = true;
              
        if (numPlayers == 2)
        {
            //rsc.gameInfo.player2.SetActive(true);
            rsc.gameInfo.player2Controller.Reset();
            rsc.gameInfo.player2Controller.Active = true;
        }
    }  

    private void PlayerDied(EventInfo eventInfo)
    {
        if (rsc.gameInfo.numberOfPlayers == 1)
        {
            if (rsc.gameInfo.player1Controller.Lives == 0)
            {
                GameOver();
            }
        }
        else
        {
            if (rsc.gameInfo.player1Controller.Lives + rsc.gameInfo.player2Controller.Lives == 0)
            {            
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        state = GameState.NOT_STARTED;
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_OVER, EventInfo.emptyInfo);
        StartCoroutine(GoToMainMenu());
    }

    private void GameFinished()
    {
        state = GameState.NOT_STARTED;
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_FINISHED, EventInfo.emptyInfo);
        StartCoroutine(GoToCredits());
    }

    public void GameCancelled()
    {
        state = GameState.NOT_STARTED;
        GoToMainMenuInmediate();
    }

    private void GoToMainMenuInmediate()
    {
        Time.timeScale = 1f;
        rsc.audioMng.StopMusic();
        AudioListener.pause = false;

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator GoToMainMenu()
    {
        rsc.audioMng.FadeOutMusic(2.5f);

        yield return new WaitForSeconds(1.5f);

        FadeCurtainEventInfo.eventInfo.fadeIn = false;
        FadeCurtainEventInfo.eventInfo.useDefaultColor = true;
        FadeCurtainEventInfo.eventInfo.useDefaultTime = false;
        FadeCurtainEventInfo.eventInfo.fadeTime = 1.5f;
        rsc.eventMng.TriggerEvent(EventManager.EventType.FADE_CURTAIN, FadeCurtainEventInfo.eventInfo);

        yield return new WaitForSeconds(1.5f);

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator GoToCredits()
    {
        rsc.audioMng.FadeOutMusic(2.5f);

        yield return new WaitForSeconds(1.5f);

        FadeCurtainEventInfo.eventInfo.fadeIn = false;
        FadeCurtainEventInfo.eventInfo.useDefaultColor = true;
        FadeCurtainEventInfo.eventInfo.useDefaultTime = false;
        FadeCurtainEventInfo.eventInfo.fadeTime = 1.5f;
        rsc.eventMng.TriggerEvent(EventManager.EventType.FADE_CURTAIN, FadeCurtainEventInfo.eventInfo);

        yield return new WaitForSeconds(1.5f);

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);
        SceneManager.LoadScene("Credits");
    }

    private IEnumerator GoToNextLevel(Level newLevel)
    {
        StartLoadingNextScene(newLevel);

        rsc.audioMng.FadeOutMusic(2.5f);

        yield return new WaitForSeconds(1.5f);

        FadeCurtainEventInfo.eventInfo.fadeIn = false;
        FadeCurtainEventInfo.eventInfo.useDefaultColor = true;
        FadeCurtainEventInfo.eventInfo.useDefaultTime = false;
        FadeCurtainEventInfo.eventInfo.fadeTime = 1.5f;
        rsc.eventMng.TriggerEvent(EventManager.EventType.FADE_CURTAIN, FadeCurtainEventInfo.eventInfo);

        yield return new WaitForSeconds(1.5f);

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);

        AllowNextSceneActivation();
    }
}
