using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using InControl;

public class GameManager : MonoBehaviour {

    public enum GameState
    {
        NOT_STARTED,
        STARTED,
        PAUSED,
        SHOWING_TUTORIAL
    }

    private GameState state;

    public GameState State { get { return state; } }

    void Awake()
    {
        state = GameState.NOT_STARTED;
    }

    // Use this for initialization
    void Start ()
    {
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
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
                if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2))
                    CloseTutorial();
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
        rsc.audioMng.PauseMainMusic();
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_PAUSED, EventInfo.emptyInfo);
    }

    public void Resume()
    {
        if (state != GameState.PAUSED) return;

        state = GameState.STARTED;
        Time.timeScale = 1f;
        rsc.audioMng.ResumeMainMusic();
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESUMED, EventInfo.emptyInfo);
    }

    public void TutorialOpened(EventInfo eventInfo)
    {
        state = GameState.SHOWING_TUTORIAL;
        Time.timeScale = 0.000000000001f;
        //rsc.audioMng.PauseMainMusic();
    }

    public void CloseTutorial()
    {
        if (state != GameState.SHOWING_TUTORIAL) return;

        state = GameState.STARTED;
        Time.timeScale = 1f;
        //rsc.audioMng.ResumeMainMusic();
        rsc.eventMng.TriggerEvent(EventManager.EventType.HIDE_TUTORIAL, EventInfo.emptyInfo);
    }

    public void SetGameStartedDEBUG()
    {
        state = GameState.STARTED;
        Debug.Log("WARNING: gameStarted set to true through a debug function!");
    }

    public void StartNewGame(int numPlayers)
    {
        InitPlayers(numPlayers);

        state = GameState.STARTED;
        //SceneManager.LoadScene("Level01");
        SceneManager.LoadScene("Intro");
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
        int levelId = ((LevelEventInfo)eventInfo).levelId;

        switch(levelId)
        {
            case 1: //Level01
                StartCoroutine(GoToNextLevel("LevelBoss"));
                break;

            case -1: //Boss
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
        rsc.audioMng.StopMainMusic();

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator GoToMainMenu()
    {
        rsc.audioMng.FadeOutMainMusic(2.5f);

        yield return new WaitForSeconds(3.0f);

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator GoToCredits()
    {
        rsc.audioMng.FadeOutMainMusic(2.5f);

        yield return new WaitForSeconds(3.0f);

        rsc.gameInfo.player1Controller.Active = false;
        rsc.gameInfo.player2Controller.Active = false;

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);
        SceneManager.LoadScene("Credits");
    }

    private IEnumerator GoToNextLevel(string nextLevel)
    {
        rsc.audioMng.FadeOutMainMusic(2.5f);

        yield return new WaitForSeconds(3.0f);

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);
        SceneManager.LoadScene(nextLevel);
    }
}
