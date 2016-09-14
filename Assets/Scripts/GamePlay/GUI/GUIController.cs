using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GUIController : MonoBehaviour
{
    [Header("Both players config")]
    public float chainDelay = 0.1f;
    public float chainAddBGDuration = 0.1f;
    public float chainBreakBGDuration = 0.1f;

    [Header("Player 1 Items")]
    public GameObject player1Zone;
    public Text player1Lives;

    private PlayerStats player1Stats;
    public Text player1CurrentCombo;
    public Slider player1ChainSlider;
    public Image player1ChainColorImg;
    public GameObject player1ChainWhiteFill;
    public GameObject player1ChainPurpleFill;
    public RectTransform player1ComboIncrementSpawnPoint;

    public GameObject player1ButtonHints;
    public GameObject player1RedButton;
    public GameObject player1GreenButton;
    public GameObject player1BlueButton;
    public GameObject player1YellowButton;
    public GameObject player1ColorsButton;

    [Header("Player 2 Items")]
    public GameObject player2Zone;
    public Text player2Lives;

    private PlayerStats player2Stats;
    public Text player2CurrentCombo;
    public Slider player2ChainSlider;
    public Image player2ChainColorImg;
    public GameObject player2ChainWhiteFill;
    public GameObject player2ChainPurpleFill;
    public RectTransform player2ComboIncrementSpawnPoint;

    public GameObject player2ButtonHints;
    public GameObject player2RedButton;
    public GameObject player2GreenButton;
    public GameObject player2BlueButton;
    public GameObject player2YellowButton;
    public GameObject player2ColorsButton;

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
    public GameObject helpGO;
    public Image helpImg;
    public Text youWinTxt;
    public Text gameOverTxt;
    public Text godModeTxt;

    public GameObject skipHint;

    private PlayerController player1Controller;
    private PlayerController player2Controller;

    private float referenceHealthFactor;
    private float referenceEnergyFactor;

    private Queue<ChainIncrementController> chainsP1 = new Queue<ChainIncrementController>();
    private Queue<ChainIncrementController> chainsP2 = new Queue<ChainIncrementController>();

    // Use this for initialization
    void Start()
    {
        if (rsc.gameInfo.numberOfPlayers == 2)
        {
            player2Zone.SetActive(true);
        }
        else
        {
            player2Zone.SetActive(false);
        }

        player1Controller = rsc.gameInfo.player1Controller;
        player2Controller = rsc.gameInfo.player2Controller;

        player1Stats = rsc.statsMng.p1Stats;
        player1ChainSlider.maxValue = player1Stats.chainMaxTime;
        player2Stats = rsc.statsMng.p2Stats;
        player2ChainSlider.maxValue = player2Stats.chainMaxTime;

        DisableHintButtons(0);

        nextColorElapsedTime = 0f;
        nextColorPrewarnTime = 0f;

        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DYING, PlayerDying);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDying);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameOver);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameFinished);
        rsc.eventMng.StartListening(EventManager.EventType.BUTTON_HINT, ButtonHint);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
        rsc.eventMng.StartListening(EventManager.EventType.START_CUT_SCENE, StartCutScene);
        rsc.eventMng.StartListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
        rsc.eventMng.StartListening(EventManager.EventType.SHOW_TUTORIAL, ShowTutorial);
        rsc.eventMng.StartListening(EventManager.EventType.HIDE_TUTORIAL, HideTutorial);
        rsc.eventMng.StartListening(EventManager.EventType.COMBO_ADD, ComboAdd);
        rsc.eventMng.StartListening(EventManager.EventType.COMBO_BREAK, ComboBreak);

        rsc.eventMng.StartListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);

        StartCoroutine(ShowChainIncrementsP1());
        StartCoroutine(ShowChainIncrementsP2());
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DYING, PlayerDying);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDying);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameOver);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameFinished);
            rsc.eventMng.StopListening(EventManager.EventType.BUTTON_HINT, ButtonHint);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
            rsc.eventMng.StopListening(EventManager.EventType.START_CUT_SCENE, StartCutScene);
            rsc.eventMng.StopListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
            rsc.eventMng.StopListening(EventManager.EventType.SHOW_TUTORIAL, ShowTutorial);
            rsc.eventMng.StopListening(EventManager.EventType.HIDE_TUTORIAL, HideTutorial);
            rsc.eventMng.StopListening(EventManager.EventType.COMBO_ADD, ComboAdd);
            rsc.eventMng.StopListening(EventManager.EventType.COMBO_BREAK, ComboBreak);

            rsc.eventMng.StopListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Player 1 update
        if (player1Controller.Active)
        {
            //Lives
            player1Lives.text = "x" + player1Controller.Lives;

            //Combo
            player1CurrentCombo.text = player1Stats.currentCombo.ToString();
            player1ChainSlider.value = player1Stats.comboRemainingTime;

            //Hints
            player1ButtonHints.transform.position = rsc.camerasMng.currentCamera.WorldToScreenPoint(player1Controller.hintPoint.position);            
        }


        //Player 2 update
        if (player2Controller.Active)
        {
            //Lives
            player2Lives.text = "x" + player2Controller.Lives;

            //Combo
            player2CurrentCombo.text = player2Stats.currentCombo.ToString();
            player2ChainSlider.value = player2Stats.comboRemainingTime;

            //Hints
            player2ButtonHints.transform.position = rsc.camerasMng.currentCamera.WorldToScreenPoint(player2Controller.hintPoint.position);            
        }

        int secondFraction = (int)(Time.time * 10) % 10;
        bool shouldShowTxt = (secondFraction > 0 && secondFraction < 5);

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
        currentInfectionNumber = rsc.enemyMng.bb.GetCurrentInfectionPercentage();

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
    }

    private void ButtonHint(EventInfo eventInfo)
    {
        ButtonHintEventInfo info = (ButtonHintEventInfo)eventInfo;

        bool rumble = false;

        switch (info.buttonType)
        {
            case ButtonHintEventInfo.ButtonType.A:
                rumble = true;
                if (info.playerId == 1)
                    player1GreenButton.SetActive(info.show);
                else
                    player2GreenButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.B:
                rumble = true;
                if (info.playerId == 1)
                    player1RedButton.SetActive(info.show);
                else
                    player2RedButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.X:
                rumble = true;
                if (info.playerId == 1)
                    player1BlueButton.SetActive(info.show);
                else
                    player2BlueButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.Y:
                rumble = true;
                if (info.playerId == 1)
                    player1YellowButton.SetActive(info.show);
                else
                    player2YellowButton.SetActive(info.show);
                break;
            case ButtonHintEventInfo.ButtonType.COLOR_BUTTONS:
                rumble = true;
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

        if (rumble && info.show)
            rsc.rumbleMng.Rumble(info.playerId, 0.25f, 0f, 1f);
    }

    private void ComboAdd(EventInfo eventInfo)
    {
        ComboEventInfo info = (ComboEventInfo)eventInfo;

        if (info.playerId == 1)
        {
            player1ChainColorImg.color = rsc.coloredObjectsMng.GetColor(info.comboColor);
            StopCoroutine(ShowP1ChainAdd());
            StartCoroutine(ShowP1ChainAdd());

            ChainIncrementController inc = rsc.poolMng.comboIncrementPool.GetObject();
            if(inc != null)
            {
                inc.transform.SetParent(player1ComboIncrementSpawnPoint);
                inc.transform.position = player1ComboIncrementSpawnPoint.position;
                inc.Set(info.comboAdd);
                chainsP1.Enqueue(inc);
            }
        }
        else
        {
            player2ChainColorImg.color = rsc.coloredObjectsMng.GetColor(info.comboColor);
            StopCoroutine(ShowP2ChainAdd());
            StartCoroutine(ShowP2ChainAdd());

            ChainIncrementController inc = rsc.poolMng.comboIncrementPool.GetObject();
            if (inc != null)
            {
                inc.transform.SetParent(player2ComboIncrementSpawnPoint);
                inc.transform.position = player2ComboIncrementSpawnPoint.position;
                inc.Set(info.comboAdd);
                chainsP2.Enqueue(inc);
            }
        }
    }

    private IEnumerator ShowChainIncrementsP1()
    {
        while (true)
        {
            if (chainsP1.Count == 0)
                yield return null;
            else
            {
                ChainIncrementController chain = chainsP1.Dequeue();
                chain.Show();
                yield return new WaitForSeconds(chainDelay);
            }
        }
    }

    private IEnumerator ShowChainIncrementsP2()
    {
        while (true)
        {
            if (chainsP2.Count == 0)
                yield return null;
            else
            {
                ChainIncrementController chain = chainsP2.Dequeue();
                chain.Show();
                yield return new WaitForSeconds(chainDelay);
            }
        }
    }

    private IEnumerator ShowP1ChainAdd()
    {
        player1ChainWhiteFill.SetActive(true);
        yield return new WaitForSeconds(chainAddBGDuration);
        player1ChainWhiteFill.SetActive(false);
    }

    private IEnumerator ShowP2ChainAdd()
    {
        player2ChainWhiteFill.SetActive(true);
        yield return new WaitForSeconds(chainAddBGDuration);
        player2ChainWhiteFill.SetActive(false);
    }

    private void ComboBreak(EventInfo eventInfo)
    {
        ComboEventInfo info = (ComboEventInfo)eventInfo;

        if (info.playerId == 1)
        {           
            StopCoroutine(ShowP1ChainBreak());
            StartCoroutine(ShowP1ChainBreak());
        }
        else
        {
            StopCoroutine(ShowP2ChainBreak());
            StartCoroutine(ShowP2ChainBreak());
        }
    }

    private IEnumerator ShowP1ChainBreak()
    {
        player1ChainPurpleFill.SetActive(true);
        yield return new WaitForSeconds(chainBreakBGDuration);
        player1ChainPurpleFill.SetActive(false);
    }

    private IEnumerator ShowP2ChainBreak()
    {
        player2ChainPurpleFill.SetActive(true);
        yield return new WaitForSeconds(chainBreakBGDuration);
        player2ChainPurpleFill.SetActive(false);
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

    private void LevelStarted(EventInfo eventInfo)
    {
        infectionAndNextColorZone.SetActive(true);
    }

    private void StartCutScene(EventInfo eventInfo)
    {
        CutSceneEventInfo info = (CutSceneEventInfo)eventInfo;

        skipHint.SetActive(info.skippeable);
    }

    private void CameraEnded(EventInfo eventInfo)
    {
        skipHint.SetActive(false);
    }

    private void ShowTutorial(EventInfo eventInfo)
    {
        TutorialEventInfo info = (TutorialEventInfo)eventInfo;
        TutorialManager.Type type = info.type;

        Sprite sprite = rsc.tutorialMng.GetImageIfNotShown(type);

        if (sprite != null)
        {
            helpImg.sprite = sprite;
            helpGO.SetActive(true);
            rsc.eventMng.TriggerEvent(EventManager.EventType.TUTORIAL_OPENED, EventInfo.emptyInfo);
        }
    }

    private void HideTutorial(EventInfo eventInfo)
    {
        if (helpGO.activeSelf)
        {
            helpGO.SetActive(false);
            rsc.eventMng.TriggerEvent(EventManager.EventType.TUTORIAL_CLOSED, EventInfo.emptyInfo);
        }
    }
}
