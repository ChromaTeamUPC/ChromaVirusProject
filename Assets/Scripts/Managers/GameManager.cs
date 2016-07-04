using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using InControl;

public class GameManager : MonoBehaviour {

    private bool gameStarted = false;
    private bool paused = false;

    // Use this for initialization
    void Start ()
    {
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (gameStarted)
        {
            int ctrlNumber = 0;

            while (ctrlNumber < rsc.gameInfo.numberOfPlayers)
            {
                //if (Input.GetButtonDown("Pause"))
                if (ctrlNumber < InputManager.Devices.Count && InputManager.Devices[ctrlNumber].GetControl(InputControlType.Start).WasPressed)
                {
                    paused = !paused;

                    if (paused)
                    {
                        Time.timeScale = 0.000000000001f;
                        rsc.audioMng.PauseMainMusic();
                        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_PAUSED, EventInfo.emptyInfo);
                    }
                    else
                    {
                        Time.timeScale = 1f;
                        rsc.audioMng.ResumeMainMusic();
                        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESUMED, EventInfo.emptyInfo);
                    }
                }

                ++ctrlNumber;
            }
        }
	}

    public void SetGameStartedDEBUG()
    {
        gameStarted = true;
        Debug.Log("WARNING: gameStarted set to true through a debug function!");
    }

    public void StartNewGame(int numPlayers)
    {
        InitPlayers(numPlayers);

        gameStarted = true;
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
            case 1: //Our only level for now
                GameFinished();
                break;
        }
    }

    private void GameOver()
    {
        gameStarted = false;
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_OVER, EventInfo.emptyInfo);
        StartCoroutine(GoToMainMenu());
    }

    private void GameFinished()
    {
        gameStarted = false;
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_FINISHED, EventInfo.emptyInfo);
        StartCoroutine(GoToCredits());
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
}
