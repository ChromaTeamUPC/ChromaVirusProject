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
    public GameObject player1RedButton;
    public GameObject player1GreenButton;
    public GameObject player1BlueButton;
    public GameObject player1YellowButton;
    public GameObject player1ColorsButton;
    private bool player1ColorMismatch;

    [Header("Player 2 Items")]
    public GameObject player2Zone;
    public Slider player2Health;
    public Slider player2Energy;
    public GameObject player2ExtraLife1;
    public GameObject player2ExtraLife2;
    public GameObject player2ColorMismatchTxt;
    public GameObject player2ButtonHints;
    public GameObject player2RedButton;
    public GameObject player2GreenButton;
    public GameObject player2BlueButton;
    public GameObject player2YellowButton;
    public GameObject player2ColorsButton;
    private bool player2ColorMismatch;

    [Header("Other Items")]
    public Image colorWarnTest;
    public Text youWinTxt;
    public Text gameOverTxt;
    public Text pauseTxt;
    public Text godModeTxt;

    public Text currentInfectionTxt;

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

        DisableHintButtons(0);

        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_START, PlayerColorMismatchStart);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_END, PlayerColorMismatchEnd);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DYING, PlayerDying);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDying);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameOver);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameFinished);
        rsc.eventMng.StartListening(EventManager.EventType.BUTTON_HINT, ButtonHint);
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_START, PlayerColorMismatchStart);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_COLOR_MISMATCH_END, PlayerColorMismatchEnd);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DYING, PlayerDying);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDying);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameOver);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameFinished);
            rsc.eventMng.StopListening(EventManager.EventType.BUTTON_HINT, ButtonHint);
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
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

        currentInfectionTxt.text = rsc.enemyMng.blackboard.GetCurrentInfectionPercentage().ToString();
    }

    private void DisableHintButtons(int playerId)
    {
        if (playerId == 0 || playerId == 1)
        {
            player1RedButton.SetActive(false);
            player1GreenButton.SetActive(false);
            player1BlueButton.SetActive(false);
            player1YellowButton.SetActive(false);
            player1ColorsButton.SetActive(false);
        }

        if (playerId == 0 || playerId == 2)
        {
            player2RedButton.SetActive(false);
            player2GreenButton.SetActive(false);
            player2BlueButton.SetActive(false);
            player2YellowButton.SetActive(false);
            player2ColorsButton.SetActive(false);
        }
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

        DisableHintButtons(info.player.Id);

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

    private void ButtonHint(EventInfo eventInfo)
    {
        ButtonHintEventInfo info = (ButtonHintEventInfo)eventInfo;

        switch (info.buttonType)
        {
            case ButtonHintEventInfo.ButtonType.A:
                if (info.playerId == 1)
                    player1GreenButton.SetActive(info.show);
                else
                    player2GreenButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.B:
                if (info.playerId == 1)
                    player1RedButton.SetActive(info.show);
                else
                    player2RedButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.X:
                if (info.playerId == 1)
                    player1BlueButton.SetActive(info.show);
                else
                    player2BlueButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.Y:
                if (info.playerId == 1)
                    player1YellowButton.SetActive(info.show);
                else
                    player2YellowButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.COLOR_BUTTONS:
                if (info.playerId == 1)
                    player1ColorsButton.SetActive(info.show);
                else
                    player2ColorsButton.SetActive(info.show);
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
        colorWarnTest.enabled = true;
    }

    private void ColorChanged(EventInfo eventInfo)
    {
        colorWarnTest.enabled = false;
    }
}
