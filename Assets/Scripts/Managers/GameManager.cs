using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using InControl;

public class GameManager : MonoBehaviour {

    public bool motionBlur = true;
    public bool colorBar = false;

    public enum Level
    {
        LEVEL_01,
        LEVEL_BOSS
    }

    public enum GameState
    {
        NOT_STARTED,
        STARTED,
        PAUSED,
        SHOWING_TUTORIAL
    }

    private GameState state;

    public Level startLevel = Level.LEVEL_01;
    private Level currentLevel;

    public GameState State { get { return state; } }

    void Awake()
    {
        state = GameState.NOT_STARTED;
    }

    // Use this for initialization
    void Start ()
    {
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
        rsc.eventMng.StartListening(EventManager.EventType.SHOW_STATS, LevelCleared); //TODo change proper event
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
            rsc.eventMng.StopListening(EventManager.EventType.SHOW_STATS, LevelCleared);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (state == GameState.NOT_STARTED) return;

        switch (state)
        {
            case GameState.STARTED:
                if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Start))
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
            default:
                break;
        }
	}

    public void Pause()
    {
        if (state != GameState.STARTED) return;

        state = GameState.PAUSED;
        Time.timeScale = 0.000000000001f;
        rsc.audioMng.PauseMusic();
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_PAUSED, EventInfo.emptyInfo);
    }

    public void Resume()
    {
        if (state != GameState.PAUSED) return;

        state = GameState.STARTED;
        Time.timeScale = 1f;
        rsc.audioMng.ResumeMusic();
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESUMED, EventInfo.emptyInfo);
    }

    public void TutorialOpened(EventInfo eventInfo)
    {
        state = GameState.SHOWING_TUTORIAL;
        Time.timeScale = 0.000000000001f;
        //rsc.audioMng.PauseMainMusic();
    }

    public void CloseTutorial(bool disableTutorial = false)
    {
        if (state != GameState.SHOWING_TUTORIAL) return;

        state = GameState.STARTED;
        Time.timeScale = 1f;
        //rsc.audioMng.ResumeMainMusic();
        if (disableTutorial)
            rsc.tutorialMng.active = false;
        rsc.eventMng.TriggerEvent(EventManager.EventType.HIDE_TUTORIAL, EventInfo.emptyInfo);
    }

    public void SetGameStartedDEBUG()
    {
        state = GameState.STARTED;

        if (SceneManager.GetActiveScene().name == "Level01")
        {
            Debug.Log("Level01 scene loaded");
            currentLevel = Level.LEVEL_01;
        }
        else if (SceneManager.GetActiveScene().name == "LevelBoss")
        {
            Debug.Log("LevelBoss scene loaded");
            currentLevel = Level.LEVEL_BOSS;
        }

        Debug.Log("WARNING: gameStarted set to true through a debug function!");
    }

    public void StartNewGame(int numPlayers)
    {
        InitPlayers(numPlayers);

        state = GameState.STARTED;

        switch (startLevel)
        {
            case Level.LEVEL_01:
                //SceneManager.LoadScene("Level01");
                currentLevel = Level.LEVEL_01;
                SceneManager.LoadScene("Intro");
                break;
            case Level.LEVEL_BOSS:
                currentLevel = Level.LEVEL_BOSS;
                SceneManager.LoadScene("LevelBoss");
                break;
            default:
                break;
        }
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

    private void LevelCleared(EventInfo eventInfo)
    {
        switch(currentLevel)
        {
            case Level.LEVEL_01: //Level01
                StartCoroutine(GoToNextLevel(Level.LEVEL_BOSS));
                break;

            case Level.LEVEL_BOSS: //Boss
                GameFinished();
                break;
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

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator GoToMainMenu()
    {
        rsc.audioMng.FadeOutMusic(2.5f);

        yield return new WaitForSeconds(3.0f);

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator GoToCredits()
    {
        rsc.audioMng.FadeOutMusic(2.5f);

        yield return new WaitForSeconds(3.0f);

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);
        SceneManager.LoadScene("Credits");
    }

    private IEnumerator GoToNextLevel(Level newLevel)
    {
        string nextLevel = "";

        switch (newLevel)
        {
            case Level.LEVEL_01:
                nextLevel = "Level01";
                currentLevel = Level.LEVEL_01;
                break;

            case Level.LEVEL_BOSS:
                nextLevel = "LevelBoss";
                currentLevel = Level.LEVEL_BOSS;
                break;
        }

        rsc.audioMng.FadeOutMusic(2.5f);

        yield return new WaitForSeconds(3.0f);

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_UNLOADED, EventInfo.emptyInfo);
        SceneManager.LoadScene(nextLevel);
    }
}
