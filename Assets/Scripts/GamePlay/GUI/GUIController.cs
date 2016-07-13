using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIController : MonoBehaviour
{
    [Header("Both players config")]
    public Color healthEmptyColor;
    public float healthThreshold1 = 0.2f;
    public Color healthThreshold1Color;
    public float healthThreshold2 = 0.3f;
    public Color healthThreshold2Color;
    public float healthThreshold3 = 0.4f;
    public Color healthThreshold3Color;
    public float startGradientThreshold = 0.5f;

    public float brightnessCicleDuration = 0.5f;
    public float startBrightnessThreshold = 0.3f;
    private Color currentP1HealthColor;
    private Color currentP2HealthColor;
    private float currentBrightness;
    private float brightnessSpeed;

    [Header("Player 1 Items")]
    public GameObject player1Zone;
    public Slider player1Health;
    public Image player1HealthFill;
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
    public Image player2HealthFill;
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

    [Header("Central Panel")]
    public GameObject infectionAndNextColorZone;

    private Color currentColor;
    public Slider nextColorSlider;
    public Image nextColorBackground;
    public Image nextColorForeground;
    private float nextColorElapsedTime;
    private float nextColorPrewarnTime;

    public Text currentInfectionTxt;
    public Text maxInfectionTxt;
    public Text percentageTxt;
    public Text infectionTxt;
    private int currentInfectionNumber;

    public Text zoneTxt;
    public Text clearedTxt;

    [Header("Other Items")]
    public GameObject infoArea;
    public GameObject pauseGroup;
    public GameObject optionsGroup;
    public GameObject confirmationGroup;
    public Button continueBtn;
    public Button quitBtn;
    public Button yesBtn;
    public Button noBtn;
    public Text youWinTxt;
    public Text gameOverTxt;
    public Text godModeTxt;

    public GameObject skipHint;


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

        nextColorElapsedTime = 0f;
        nextColorPrewarnTime = 0f;

        currentBrightness = 1f;
        currentP1HealthColor = Color.white;
        currentP2HealthColor = Color.white;

        if (brightnessCicleDuration > 0)
            brightnessSpeed = 1 / brightnessCicleDuration;
        else
            brightnessSpeed = 1;

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
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_REACHED, ZoneReached);
        rsc.eventMng.StartListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
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
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_REACHED, ZoneReached);
            rsc.eventMng.StopListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        currentBrightness = (Mathf.Sin(Time.time * Mathf.PI * brightnessSpeed) / 2) + 1; //Values between 0.5 and 1.5

        //Player 1 update
        if (player1Controller.Active)
        {
            //Health bar
            float p1HealthValue = player1Controller.Health * referenceHealthFactor;
            player1Health.value = p1HealthValue;

            if (p1HealthValue > startGradientThreshold)
                currentP1HealthColor = Color.white;
            else if (p1HealthValue >= healthThreshold3)
                currentP1HealthColor = Color.Lerp(healthThreshold3Color, Color.white, (p1HealthValue - healthThreshold3) / (startGradientThreshold - healthThreshold3));
            else if (p1HealthValue >= healthThreshold2)
                currentP1HealthColor = Color.Lerp(healthThreshold2Color, healthThreshold3Color, (p1HealthValue - healthThreshold2) / (healthThreshold3 - healthThreshold2));
            else if (p1HealthValue >= healthThreshold1)
                currentP1HealthColor = Color.Lerp(healthThreshold1Color, healthThreshold2Color, (p1HealthValue - healthThreshold1) / (healthThreshold2 - healthThreshold1));
            else
                currentP1HealthColor = Color.Lerp(healthEmptyColor, healthThreshold1Color, p1HealthValue / healthThreshold1);

            if (p1HealthValue < startBrightnessThreshold)
            {
                currentP1HealthColor *= currentBrightness;
                currentP1HealthColor.a = 1f;
            }

            player1HealthFill.color = currentP1HealthColor;


            //Energy bar
            player1Energy.value = player1Controller.Energy * referenceEnergyFactor;

            //Lives
            if (player1Controller.Lives > 1)
                player1ExtraLife1.SetActive(true);
            else
                player1ExtraLife1.SetActive(false);

            if (player1Controller.Lives > 2)
                player1ExtraLife2.SetActive(true);
            else
                player1ExtraLife2.SetActive(false);

            //Hints
            player1ButtonHints.transform.position = rsc.camerasMng.currentCamera.WorldToScreenPoint(player1Controller.hintPoint.position);
        }


        //Player 2 update
        if (player2Controller.Active)
        {
            //Health bar
            player2Health.value = player2Controller.Health * referenceHealthFactor;

            float p2HealthValue = player2Controller.Health * referenceHealthFactor;
            player2Health.value = p2HealthValue;

            if (p2HealthValue > startGradientThreshold)
                currentP2HealthColor = Color.white;
            else if (p2HealthValue >= healthThreshold3)
                currentP2HealthColor = Color.Lerp(healthThreshold3Color, Color.white, (p2HealthValue - healthThreshold3) / (startGradientThreshold - healthThreshold3));
            else if (p2HealthValue >= healthThreshold2)
                currentP2HealthColor = Color.Lerp(healthThreshold2Color, healthThreshold3Color, (p2HealthValue - healthThreshold2) / (healthThreshold3 - healthThreshold2));
            else if (p2HealthValue >= healthThreshold1)
                currentP2HealthColor = Color.Lerp(healthThreshold1Color, healthThreshold2Color, (p2HealthValue - healthThreshold1) / (healthThreshold2 - healthThreshold1));
            else
                currentP2HealthColor = Color.Lerp(healthEmptyColor, healthThreshold1Color, p2HealthValue / healthThreshold1);

            if (p2HealthValue < startBrightnessThreshold)
            {
                currentP2HealthColor *= currentBrightness;
                currentP2HealthColor.a = 1f;
            }

            player2HealthFill.color = currentP2HealthColor;

            //Energy bar
            player2Energy.value = player2Controller.Energy * referenceEnergyFactor;

            //Lives
            if (player2Controller.Lives > 1)
                player2ExtraLife1.SetActive(true);
            else
                player2ExtraLife1.SetActive(false);

            if (player2Controller.Lives > 2)
                player2ExtraLife2.SetActive(true);
            else
                player2ExtraLife2.SetActive(false);

            //Hints
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

        //Next color slider update
        if (nextColorElapsedTime < nextColorPrewarnTime)
        {
            float factor = nextColorElapsedTime / nextColorPrewarnTime;

            nextColorSlider.value = Mathf.Lerp(0f, 1f, factor);

            nextColorBackground.color = Color.Lerp(currentColor, Color.black, factor * 2);

            nextColorElapsedTime += Time.deltaTime;
        }

        //Current infection update
        currentInfectionNumber = rsc.enemyMng.blackboard.GetCurrentInfectionPercentage();

        if (currentInfectionNumber == 100)
        {
            currentInfectionTxt.enabled = false;
            percentageTxt.enabled = false;

            maxInfectionTxt.enabled = true;

            infectionTxt.enabled = true;

            zoneTxt.enabled = false;
            clearedTxt.enabled = false;
        }
        else if (currentInfectionNumber == 0)
        {
            currentInfectionTxt.enabled = false;
            percentageTxt.enabled = false;

            maxInfectionTxt.enabled = false;

            infectionTxt.enabled = false;

            zoneTxt.enabled = true;
            clearedTxt.enabled = true;
        }
        else
        {
            currentInfectionTxt.enabled = true;
            percentageTxt.enabled = true;

            maxInfectionTxt.enabled = false;

            infectionTxt.enabled = true;

            zoneTxt.enabled = false;
            clearedTxt.enabled = false;

            currentInfectionTxt.text = currentInfectionNumber.ToString();
        }
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
        infoArea.SetActive(false);
        pauseGroup.SetActive(true);
        optionsGroup.SetActive(true);
        confirmationGroup.SetActive(false);
        continueBtn.Select();
    }

    private void GameResumed(EventInfo eventInfo)
    {
        infoArea.SetActive(true);
        pauseGroup.SetActive(false);
    }

    public void ResumeGame()
    {
        rsc.gameMng.Resume();
    }

    public void Quit()
    {
        optionsGroup.SetActive(false);
        confirmationGroup.SetActive(true);
        noBtn.Select();
    }

    public void QuitConfirmed()
    {
        rsc.gameMng.GameCancelled();
    }

    public void QuitCancelled()
    {
        optionsGroup.SetActive(true);
        confirmationGroup.SetActive(false);
        quitBtn.Select();
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

    private void ColorPrewarn(EventInfo eventInfo)
    {
        ColorPrewarnEventInfo info = (ColorPrewarnEventInfo)eventInfo;

        nextColorPrewarnTime = info.prewarnSeconds;
        nextColorElapsedTime = 0f;

        //nextColorBackground.color = nextColorForeground.color;
        nextColorForeground.color = rsc.coloredObjectsMng.GetColor(info.newColor);

        if (info.prewarnSeconds > 0)
            nextColorSlider.value = 1f;
        else
            nextColorSlider.value = 0f;
    }

    private void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        currentColor = rsc.coloredObjectsMng.GetColor(info.newColor);
        nextColorBackground.color = currentColor;
        nextColorSlider.value = 0f;
    }

    private void ZoneReached(EventInfo eventInfo)
    {
        infectionAndNextColorZone.SetActive(true);
    }

    private void CameraEnded(EventInfo eventInfo)
    {
        skipHint.SetActive(false);
    }
}
