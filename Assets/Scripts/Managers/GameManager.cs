using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour {

    private AsyncOperation loadLevel;

    private bool gameStarted = true;
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
            if (Input.GetButtonDown("Ok"))
            {
                paused = !paused;

                if (paused)
                {
                    Time.timeScale = 0.000000000001f;
                    rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_PAUSED, EventInfo.emptyInfo);
                }
                else
                {
                    Time.timeScale = 1f;
                    rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESUMED, EventInfo.emptyInfo);
                }
            }
        }
	}

    public void StartPreloadingFirstLevel()
    {
        if (loadLevel == null)
        {
            loadLevel = SceneManager.LoadSceneAsync("Level01");
            loadLevel.allowSceneActivation = false;
        }
    }


    public void StartNewGame(int numPlayers)
    {
        InitPlayers(numPlayers);

        gameStarted = true;
        loadLevel.allowSceneActivation = true;
    }

    public void InitPlayers(int numPlayers)
    {
        rsc.gameInfo.numberOfPlayers = numPlayers;
        rsc.gameInfo.player1Controller.ResetPlayer();
        rsc.gameInfo.player1Controller.Active = true;
        rsc.gameInfo.player2Controller.ResetPlayer();
        if (numPlayers == 2)
        {
            rsc.gameInfo.player2Controller.Active = true;
            rsc.gameInfo.player2.SetActive(true);
        }
        else {
            rsc.gameInfo.player2Controller.Active = false;
            rsc.gameInfo.player2.SetActive(false);
        }
    }

    private void PlayerDied(EventInfo eventInfo)
    {
        if (rsc.gameInfo.numberOfPlayers == 1)
        {
            if (rsc.gameInfo.player1Controller.Lives == 0)
            {
                rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_OVER, EventInfo.emptyInfo);
                StartCoroutine(GoToMainMenu());
            }
        }
        else
        {
            if (rsc.gameInfo.player1Controller.Lives + rsc.gameInfo.player2Controller.Lives == 0)
            {
                rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_OVER, EventInfo.emptyInfo);
                StartCoroutine(GoToMainMenu());
            }
        }
    }

    private IEnumerator GoToMainMenu()
    {
        yield return new WaitForSeconds(3.0f);

        SceneManager.LoadScene("MainMenu");
    }

    private void LevelCleared(EventInfo eventInfo)
    {
        int levelId = ((LevelEventInfo)eventInfo).levelId;

        switch(levelId)
        {
            case 1: //Our only level for now
                rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_FINISHED, EventInfo.emptyInfo);
                StartCoroutine(GoToCredits());
                break;
        }
    }

    private IEnumerator GoToCredits()
    {
        yield return new WaitForSeconds(3.0f);

        SceneManager.LoadScene("Credits");
    }
}
