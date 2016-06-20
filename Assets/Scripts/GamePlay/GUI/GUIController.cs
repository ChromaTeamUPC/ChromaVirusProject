using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIController : MonoBehaviour
{
    [Header("Player 1 Items")]
    public GameObject player1Zone;
    public Slider player1Health;
    public Slider player1Energy;
    public GameObject player1ExtraLife1;
    public GameObject player1ExtraLife2;
    public GameObject player1ColorMismatchTxt;
    public GameObject player1ButtonHints;
    public GameObject player1AButton;
    private bool player1ColorMismatch;

    [Header("Player 2 Items")]
    public GameObject player2Zone;
    public Slider player2Health;
    public Slider player2Energy;
    public GameObject player2ExtraLife1;
    public GameObject player2ExtraLife2;
    public GameObject player2ColorMismatchTxt;
    public GameObject player2ButtonHints;
    public GameObject player2AButton;
    private bool player2ColorMismatch;

    [Header("Other Items")]
    public Image colorWarnTest;
    public Text youWinTxt;
    public Text gameOverTxt;
    public Text pauseTxt;
    public Text godModeTxt;

    //private GameObject player1;
    //private GameObject player2;
    private PlayerController player1Controller;
    private PlayerController player2Controller;

    private float referenceHealthFactor;
    private float referenceEnergyFactor;

    // Use this for initialization
    void Start ()
    {
        if(rsc.gameInfo.numberOfPlayers == 2)
        {
            player2Zone.SetActive(true);
        }
        else
        {
            player2Zone.SetActive(false);
        }

        player1Controller = rsc.gameInfo.player1Controller;
        player2Controller = rsc.gameInfo.player2Controller;

        referenceHealthFactor = player1Health.maxValue / player1Controller.maxHealth;
        referenceEnergyFactor = player1Energy.maxValue / player1Controller.maxEnergy;

        player1ColorMismatch = false;
        player2ColorMismatch = false;

        player1AButton.SetActive(false);
        player2AButton.SetActive(false);

        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_START, PlayerColorMismatchStart);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_END, PlayerColorMismatchEnd);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DYING, PlayerDying);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameOver);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameFinished);
        rsc.eventMng.StartListening(EventManager.EventType.BUTTON_HINT, ButtonHint);
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_START, PlayerColorMismatchStart);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_END, PlayerColorMismatchEnd);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DYING, PlayerDying);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameOver);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameFinished);
            rsc.eventMng.StopListening(EventManager.EventType.BUTTON_HINT, ButtonHint);
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Player 1 update
        if (player1Controller.Active)
        {
            player1Health.value = player1Controller.Health * referenceHealthFactor;
            player1Energy.value = player1Controller.Energy * referenceEnergyFactor;

            if (player1Controller.Lives > 1)
                player1ExtraLife1.SetActive(true);
            else
                player1ExtraLife1.SetActive(false);

            if (player1Controller.Lives > 2)
                player1ExtraLife2.SetActive(true);
            else
                player1ExtraLife2.SetActive(false);

            player1ButtonHints.transform.position = rsc.camerasMng.currentCamera.WorldToScreenPoint(player1Controller.hintPoint.position);
        }


        //Player 2 update
        if (player2Controller.Active)
        {
            player2Health.value = player2Controller.Health * referenceHealthFactor;
            player2Energy.value = player2Controller.Energy * referenceEnergyFactor;

            if (player2Controller.Lives > 1)
                player2ExtraLife1.SetActive(true);
            else
                player2ExtraLife1.SetActive(false);

            if (player2Controller.Lives > 2)
                player2ExtraLife2.SetActive(true);
            else
                player2ExtraLife2.SetActive(false);

            player2ButtonHints.transform.position = rsc.camerasMng.currentCamera.WorldToScreenPoint(player2Controller.hintPoint.position);
        }

        int secondFraction = (int)(Time.time * 10) % 10;
        bool shouldShowTxt = (secondFraction > 0 && secondFraction < 5);

        if (player1ColorMismatch)
            player1ColorMismatchTxt.SetActive(shouldShowTxt);

        if (player2ColorMismatch)
            player2ColorMismatchTxt.SetActive(shouldShowTxt);

        if (rsc.debugMng.godMode)
            godModeTxt.enabled = shouldShowTxt;
        else
            godModeTxt.enabled = false;
    }

    private void PlayerColorMismatchStart(EventInfo eventInfo)
    {
        PlayerEventInfo info = (PlayerEventInfo)eventInfo;

        if (info.player.Id == 1)
        {
            player1ColorMismatch = true;
            player1ColorMismatchTxt.SetActive(true);
        }
        else
        {
            player2ColorMismatch = true;
            player2ColorMismatchTxt.SetActive(true);
        }
    }

    private void PlayerColorMismatchEnd(EventInfo eventInfo)
    {
        PlayerEventInfo info = (PlayerEventInfo)eventInfo;

        if (info.player.Id == 1)
        {
            player1ColorMismatch = false;
            player1ColorMismatchTxt.SetActive(false);
        }
        else
        {
            player2ColorMismatch = false;
            player2ColorMismatchTxt.SetActive(false);
        }
    }

    private void GamePaused(EventInfo eventInfo)
    {
        pauseTxt.enabled = true;
    }

    private void GameResumed(EventInfo eventInfo)
    {
        pauseTxt.enabled = false;
    }

    private void GameOver(EventInfo eventInfo)
    {
        gameOverTxt.enabled = true;
    }

    private void GameFinished(EventInfo eventInfo)
    {
        youWinTxt.enabled = true;
    }

    private void PlayerDying(EventInfo eventInfo)
    {
        PlayerEventInfo info = (PlayerEventInfo)eventInfo;

        if (info.player.Id == 1)
        {
            player1ColorMismatch = false;
            player1ColorMismatchTxt.SetActive(false);
            player1AButton.SetActive(false);
        }
        else
        {
            player2ColorMismatch = false;
            player2ColorMismatchTxt.SetActive(false);
            player2AButton.SetActive(false);
        }
    }

    private void ButtonHint(EventInfo eventInfo)
    {
        ButtonHintEventInfo info = (ButtonHintEventInfo)eventInfo;

        switch (info.buttonType)
        {
            case ButtonHintEventInfo.ButtonType.A:
                if (info.playerId == 1)
                    player1AButton.SetActive(info.show);
                else
                    player2AButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.B:
                break;
            case ButtonHintEventInfo.ButtonType.X:
                break;
            case ButtonHintEventInfo.ButtonType.Y:
                break;
            case ButtonHintEventInfo.ButtonType.COLOR_BUTTONS:
                if (info.playerId == 1)
                    player1AButton.SetActive(info.show);
                else
                    player2AButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.LB:
                break;
            case ButtonHintEventInfo.ButtonType.LT:
                break;
            case ButtonHintEventInfo.ButtonType.RB:
                break;
            case ButtonHintEventInfo.ButtonType.RT:
                break;
            default:
                break;
        }
    }

    private void ColorPrewarn(EventInfo evenInfo)
    {
        ColorEventInfo info = (ColorEventInfo)evenInfo;
        colorWarnTest.color = rsc.coloredObjectsMng.GetColor(info.newColor);
    }
}
