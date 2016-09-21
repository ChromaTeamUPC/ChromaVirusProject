using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GUIController : MonoBehaviour
{
    [Header("Chain Config")]
    public float chainDelay = 0.1f;
    public float chainAddBGDuration = 0.1f;
    public float chainBreakBGDuration = 0.1f;

    [Header("Score Config")]
    public float scoreInitialDelay = 0.5f;
    public float scoreChainDuration = 1f;
    public float scoreAccuracyDuration = 1f;
    public float scoreTimeDuration = 1f;
    public float showPlayerImgDuration = 2f;
    public float showGradeInitialScale = 20f;
    public float showGradeImgDuration = 0.5f;
    public float scoreFinalDelay = 0.2f;

    [Header("Score Items")]
    public GameObject scoreGO;
    public GameObject scoreContinueHintGO;
    public GameObject scoreSingleGO;
    public Sprite[] player1ScoreImages = new Sprite[4];
    public Sprite[] player2ScoreImages = new Sprite[4];
    public Sprite[] gradeScoreImages = new Sprite[4];
    public Text scoreSingleChain;
    public Text scoreSingleAccuracy;
    public Text scoreSingleTime;
    public Text scoreSingleTotal;
    public Image scoreSinglePlayerImg;
    public Image scoreSingleGradeImg;
    public GameObject scoreMultiGO;
    public Text scoreMultiChainP1;
    public Text scoreMultiAccuracyP1;
    public Text scoreMultiTotalP1;
    public Image scoreMultiPlayerImgP1;
    public Image scoreMultiGradeImgP1;
    public Text scoreMultiChainP2;
    public Text scoreMultiAccuracyP2;
    public Text scoreMultiTotalP2;
    public Image scoreMultiPlayerImgP2;
    public Image scoreMultiGradeImgP2;
    public Text scoreMultiTime;

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
    public Text player1ChainTxt;

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
    public Text player2ChainTxt;

    public GameObject player2ButtonHints;
    public GameObject player2RedButton;
    public GameObject player2GreenButton;
    public GameObject player2BlueButton;
    public GameObject player2YellowButton;
    public GameObject player2ColorsButton;

    [Header("Central Panel")]
    public GameObject infectionAndNextColorZone;

    [Header("Next Color Panel")]
    public Slider nextColorSlider;
    public Image nextColorBackground;
    public Image nextColorForeground;
    public Image nextColorHandle;
    private float nextColorElapsedTime;
    private float nextColorPrewarnTime;
    private ChromaColor currentChromaColor;
    private Color currentColor;

    [Header("Infection Panel")]
    public GameObject infectionZone;
    public Slider clearMainSlider;
    public Slider clearPercentageSlider;
    public Text percentageBackgroundText;
    public Text percentageForegroundText;
    private int currentInfectionNumber;

    [Header("Boss Panel")]
    public GameObject bossZone;
    public float bossBodyBlinkInterval = 0.2f;
    public float bossHeadBlinkInterval = 0.2f;
    public float bossHeadStunnedBlinkInterval = 0.1f;
    public float bossSpecialBlinkInterval = 0.3f;
    public float bossDiedBlinkInterval = 0.5f;

    public Color lightGrey;
    public Color darkGrey;

    public Image bossSpecialSign;
    public Image head;
    public Sprite headFillImage;
    public Sprite headHollowImage;
    public Slider headSlider;
    public GameObject headSliderGO;

    public Sprite bodyFillImage;
    public Sprite bodyHollowImage;
    public Image[] bodyParts;

    public Image tail;
    public Sprite tailFillImage;
    public Sprite tailHollowImage;
    private WormBlackboard wormBb;
    private WormBodySegmentController[] bodySegmentControllers;

    private Coroutine bodyBlinkCoroutine;
    private Coroutine headBlinkCoroutine;
    private Coroutine specialBlinkCoroutine;
    private Coroutine diedBlinkCoroutine;

    private enum BossUIState
    {
        DESTROYED,
        INVULNERABLE,
        BODY_ACTIVATED,
        HEAD_ACTIVATED,
        HEAD_STUNNED
    }
    private BossUIState bossUIState;


    [Header("Pause Items")]
    public GameObject pauseGroup;
    public GameObject optionsGroup;
    public GameObject confirmationGroup;
    public Button continueBtn;
    private PauseOptionsController continueBtnController;
    public Button quitBtn;
    public Button yesBtn;
    public Button noBtn;

    [Header("Tutorial Items")]
    public GameObject tutorialGO;
    public Image tutorialImg;

    [Header("Other Items")]
    public GameObject infoArea;
    public GameObject playersArea;
    public Text youWinTxt;
    public Text gameOverTxt;
    public Text godModeTxt;
    public GameObject skipHint;
    public FadeSceneScript fadeCurtain;

    private PlayerController player1Controller;
    private PlayerController player2Controller;

    private Queue<ChainIncrementController> chainsP1 = new Queue<ChainIncrementController>();
    private Queue<ChainIncrementController> chainsP2 = new Queue<ChainIncrementController>();

    // Use this for initialization
    void Start()
    {
        continueBtnController = continueBtn.GetComponent<PauseOptionsController>();

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
        player2Stats = rsc.statsMng.p2Stats;

        DisableHintButtons(0);

        infectionAndNextColorZone.SetActive(false);
        infectionZone.SetActive(false);
        bossZone.SetActive(false);
        scoreSingleGO.SetActive(false);
        scoreMultiGO.SetActive(false);
        scoreGO.SetActive(false);

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
        rsc.eventMng.StartListening(EventManager.EventType.SHOW_SCORE, ShowScore);
        rsc.eventMng.StartListening(EventManager.EventType.HIDE_SCORE, HideScore);
        rsc.eventMng.StartListening(EventManager.EventType.COMBO_ADD, ComboAdd);
        rsc.eventMng.StartListening(EventManager.EventType.COMBO_BREAK, ComboBreak);
        rsc.eventMng.StartListening(EventManager.EventType.FADE_CURTAIN, FadeCurtain);

        rsc.eventMng.StartListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);

        rsc.eventMng.StartListening(EventManager.EventType.WORM_VULNERABLE, BossVulnerable);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_INVULNERABLE, BossInvulnerable);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_CHARGED, BossHeadChargeChanged);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_DISCHARGED, BossHeadChargeChanged);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_ACTIVATED, BossHeadActivated);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_STUNNED, BossHeadStunned);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_DEACTIVATED, BossHeadDeactivated);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_PHASE_ENDED, BossPhaseEnded);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_DYING, BossDying);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_DIED, BossDied);

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
            rsc.eventMng.StopListening(EventManager.EventType.SHOW_SCORE, ShowScore);
            rsc.eventMng.StopListening(EventManager.EventType.HIDE_SCORE, HideScore);
            rsc.eventMng.StopListening(EventManager.EventType.COMBO_ADD, ComboAdd);
            rsc.eventMng.StopListening(EventManager.EventType.COMBO_BREAK, ComboBreak);
            rsc.eventMng.StartListening(EventManager.EventType.FADE_CURTAIN, FadeCurtain);

            rsc.eventMng.StopListening(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarn);
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);

            rsc.eventMng.StopListening(EventManager.EventType.WORM_VULNERABLE, BossVulnerable);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_INVULNERABLE, BossInvulnerable);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_CHARGED, BossHeadChargeChanged);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_DISCHARGED, BossHeadChargeChanged);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_ACTIVATED, BossHeadActivated);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_STUNNED, BossHeadStunned);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_DEACTIVATED, BossHeadDeactivated);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_PHASE_ENDED, BossPhaseEnded);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_DYING, BossDying);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_DIED, BossDied);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Player 1 update
        if (player1Controller.Active)
        {
            //Lives
            player1Lives.text = player1Controller.Lives.ToString();

            //Combo
            player1CurrentCombo.text = player1Stats.currentChain.ToString();
            player1ChainSlider.value = player1Stats.chainRemainingTime;

            //Hints
            player1ButtonHints.transform.position = rsc.camerasMng.currentCamera.WorldToScreenPoint(player1Controller.hintPoint.position);            
        }


        //Player 2 update
        if (player2Controller.Active)
        {
            //Lives
            player2Lives.text = player2Controller.Lives.ToString();

            //Combo
            player2CurrentCombo.text = player2Stats.currentChain.ToString();
            player2ChainSlider.value = player2Stats.chainRemainingTime;

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
        if (infectionZone.activeSelf)
        {
            currentInfectionNumber = rsc.enemyMng.bb.GetCurrentInfectionPercentage();

            int clearNumber = 100 - currentInfectionNumber;

            clearMainSlider.value = clearNumber;
            clearPercentageSlider.value = clearNumber;
            percentageBackgroundText.text = clearNumber + "%";
            percentageForegroundText.text = clearNumber + "%";
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
        rsc.audioMng.StartFx.Play();
        infoArea.SetActive(false);
        pauseGroup.SetActive(true);
        optionsGroup.SetActive(true);
        confirmationGroup.SetActive(false);
        continueBtnController.SetFirstTime();
        continueBtn.Select();
    }

    private void GameResumed(EventInfo eventInfo)
    {
        rsc.audioMng.StartFx.Play();
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
            StopCoroutine("ShowP1ChainAdd");
            StartCoroutine("ShowP1ChainAdd");

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
            StopCoroutine("ShowP2ChainAdd");
            StartCoroutine("ShowP2ChainAdd");

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
            StopCoroutine("ShowP1ChainBreak");
            StartCoroutine("ShowP1ChainBreak");
        }
        else
        {
            StopCoroutine("ShowP2ChainBreak");
            StartCoroutine("ShowP2ChainBreak");
        }
    }

    private IEnumerator ShowP1ChainBreak()
    {
        player1ChainTxt.color = Color.black;
        player1ChainPurpleFill.SetActive(true);
        yield return new WaitForSeconds(chainBreakBGDuration);
        player1ChainPurpleFill.SetActive(false);
        player1ChainTxt.color = Color.white;
    }

    private IEnumerator ShowP2ChainBreak()
    {
        player1ChainTxt.color = Color.black;
        player2ChainPurpleFill.SetActive(true);
        yield return new WaitForSeconds(chainBreakBGDuration);
        player2ChainPurpleFill.SetActive(false);
        player1ChainTxt.color = Color.white;
    }

    private void ColorPrewarn(EventInfo eventInfo)
    {
        ColorPrewarnEventInfo info = (ColorPrewarnEventInfo)eventInfo;

        nextColorPrewarnTime = info.prewarnSeconds;
        nextColorElapsedTime = 0f;

        //nextColorBackground.color = nextColorForeground.color;
        Color newColor = rsc.coloredObjectsMng.GetColor(info.newColor);
        nextColorForeground.color = newColor;
        nextColorHandle.color = newColor;

        if (info.prewarnSeconds > 0)
            nextColorSlider.value = 1f;
        else
            nextColorSlider.value = 0f;
    }

    private void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        currentChromaColor = info.newColor;
        currentColor = rsc.coloredObjectsMng.GetColor(info.newColor);
        nextColorBackground.color = currentColor;
        nextColorSlider.value = 0f;
    }

    private void LevelStarted(EventInfo eventInfo)
    {
        infectionAndNextColorZone.SetActive(true);

        //Set initial values
        float chainMaxTime = rsc.statsMng.GetCurrentLevelStats().chainMaxTime;
        player1ChainSlider.maxValue = chainMaxTime;
        player2ChainSlider.maxValue = chainMaxTime;

        currentChromaColor = rsc.colorMng.CurrentColor;
        currentColor = rsc.coloredObjectsMng.GetColor(rsc.colorMng.CurrentColor);
        nextColorPrewarnTime = rsc.colorMng.prewarningSeconds;
        nextColorElapsedTime = rsc.colorMng.ElapsedTime;

        float factor = nextColorElapsedTime / nextColorPrewarnTime;

        nextColorSlider.value = Mathf.Lerp(0f, 1f, factor);
        nextColorBackground.color = Color.Lerp(currentColor, Color.black, factor * 2);
        Color newColor = rsc.coloredObjectsMng.GetColor(rsc.colorMng.NextColor);
        nextColorForeground.color = newColor;
        nextColorHandle.color = newColor;

        switch (rsc.gameMng.CurrentLevel)
        {
            case GameManager.Level.LEVEL_01:
                infectionZone.SetActive(true);
                bossZone.SetActive(false);
                break;
            case GameManager.Level.LEVEL_BOSS:
                infectionZone.SetActive(false);
                bossZone.SetActive(true);
                wormBb = rsc.enemyMng.bb.worm;
                bodySegmentControllers = wormBb.bodySegmentControllers;
                bossUIState = BossUIState.INVULNERABLE;
                InitBossValues();
                break;
            default:
                break;
        }
    } 

    private void StartCutScene(EventInfo eventInfo)
    {
        CutSceneEventInfo info = (CutSceneEventInfo)eventInfo;

        playersArea.SetActive(false);
        skipHint.SetActive(info.skippeable);
    }

    private void CameraEnded(EventInfo eventInfo)
    {
        playersArea.SetActive(true);
        skipHint.SetActive(false);
    }

    private void ShowTutorial(EventInfo eventInfo)
    {    
        TutorialEventInfo info = (TutorialEventInfo)eventInfo;
        TutorialManager.Type type = info.type;

        Sprite sprite = rsc.tutorialMng.GetImageIfNotShown(type);

        if (sprite != null)
        {
            tutorialImg.sprite = sprite;
            tutorialGO.SetActive(true);
            infoArea.SetActive(false);
            rsc.eventMng.TriggerEvent(EventManager.EventType.TUTORIAL_OPENED, EventInfo.emptyInfo);
        }
    }

    private void HideTutorial(EventInfo eventInfo)
    {
        if (tutorialGO.activeSelf)
        {
            tutorialGO.SetActive(false);
            infoArea.SetActive(true);
            rsc.eventMng.TriggerEvent(EventManager.EventType.TUTORIAL_CLOSED, EventInfo.emptyInfo);
        }
    }

    private void ShowScore(EventInfo eventInfo)
    {
        if (!scoreGO.activeSelf)
        {
            rsc.eventMng.TriggerEvent(EventManager.EventType.SCORE_OPENING, EventInfo.emptyInfo);

            scoreGO.SetActive(true);
            infoArea.SetActive(false);

            if (rsc.gameInfo.numberOfPlayers == 1)
                StartCoroutine(ShowScoreSingleProgress());
            else
                StartCoroutine(ShowScoreMultiProgress());
        }
    }

    private IEnumerator ShowScoreSingleProgress()
    {
        scoreSinglePlayerImg.enabled = false;
        scoreSingleGradeImg.enabled = false;

        LevelStats levelStats = rsc.statsMng.GetCurrentLevelStats();
        int totalTime = rsc.statsMng.GetTotalTime();

        int currentChain = 0;
        int totalScore = 0;

        int currentAccuracy = 0;
        int percentScore = 0;

        int timeScore = 0;

        float elapsedTime = 0f;
        float factor = 0f;

        scoreSingleChain.text = totalScore.ToString();
        scoreSingleAccuracy.text = totalScore.ToString();
        scoreSingleTotal.text = totalScore.ToString();
        scoreSingleTime.text = levelStats.baseSeconds.ToString();

        scoreSingleGO.SetActive(true);

        yield return new WaitForSeconds(scoreInitialDelay);

        //Chain update
        elapsedTime += Time.deltaTime;
        factor = elapsedTime / scoreChainDuration;

        while(elapsedTime < scoreChainDuration)
        {
            currentChain = (int)Mathf.Lerp(0, player1Stats.maxChain, factor);
            totalScore = currentChain * levelStats.maxChainMultiplier;

            scoreSingleChain.text = currentChain.ToString();
            scoreSingleTotal.text = string.Format("{0:n0}", totalScore);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreChainDuration;
        }

        totalScore = player1Stats.maxChain * levelStats.maxChainMultiplier;
        scoreSingleChain.text = player1Stats.maxChain.ToString();
        scoreSingleTotal.text = string.Format("{0:n0}", player1Stats.maxChain * levelStats.maxChainMultiplier);
        yield return null;

        //Accuracy update
        elapsedTime = 0f;
        factor = 0f;

        while(elapsedTime < scoreAccuracyDuration)
        {
            currentAccuracy = (int)Mathf.Lerp(0, player1Stats.colorAccuracy, factor);
            percentScore = totalScore / 100 * currentAccuracy;

            scoreSingleAccuracy.text = currentAccuracy.ToString();
            scoreSingleTotal.text = string.Format("{0:n0}", percentScore);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreAccuracyDuration;
        }

        percentScore = totalScore / 100 * player1Stats.colorAccuracy;
        scoreSingleAccuracy.text = player1Stats.colorAccuracy.ToString();
        scoreSingleTotal.text = string.Format("{0:n0}", percentScore);
        yield return null;

        //Time update
        elapsedTime = 0f;
        factor = 0f;

        if (totalTime < levelStats.baseSeconds)
            scoreSingleTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.GREEN);
        else if (totalTime > levelStats.baseSeconds)
            scoreSingleTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.RED);

        while (elapsedTime < scoreTimeDuration)
        {
            int seconds = (int)Mathf.Lerp(levelStats.baseSeconds, totalTime, factor);
            timeScore = percentScore + ((levelStats.baseSeconds - seconds) * levelStats.secondMultiplier);

            scoreSingleTime.text = seconds.ToString();
            scoreSingleTotal.text = string.Format("{0:n0}", timeScore);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreTimeDuration;
        }

        timeScore = percentScore + ((levelStats.baseSeconds - totalTime) * levelStats.secondMultiplier);
        scoreSingleTime.text = totalTime.ToString();
        scoreSingleTotal.text = string.Format("{0:n0}", timeScore);

        //Player Image fading
        elapsedTime = 0f;
        factor = 0f;
        scoreSinglePlayerImg.enabled = true;
        scoreSinglePlayerImg.color = new Color(1, 1, 1, 0);
        scoreSinglePlayerImg.sprite = player1ScoreImages[(int)player1Stats.finalGrade];

        while(elapsedTime < showPlayerImgDuration)
        {
            float alpha = Mathf.Lerp(0, 1, factor);
            scoreSinglePlayerImg.color = new Color(1, 1, 1, alpha);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / showPlayerImgDuration;
        }

        scoreSinglePlayerImg.color = Color.white;

        //Grade Img Stamp
        elapsedTime = 0;
        factor = 0;
        scoreSingleGradeImg.enabled = true;
        scoreSingleGradeImg.transform.localScale = new Vector3(showGradeInitialScale, showGradeInitialScale, 1f);
        scoreSingleGradeImg.sprite = gradeScoreImages[(int)player1Stats.finalGrade];

        while (elapsedTime < showGradeImgDuration)
        {
            float scale = Mathf.Lerp(showGradeInitialScale, 1, factor);
            scoreSingleGradeImg.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / showGradeImgDuration;
        }

        scoreSingleGradeImg.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(scoreFinalDelay);

        scoreContinueHintGO.SetActive(true);
        rsc.eventMng.TriggerEvent(EventManager.EventType.SCORE_OPENED, EventInfo.emptyInfo);
    }

    private IEnumerator ShowScoreMultiProgress()
    {
        scoreMultiPlayerImgP1.enabled = false;
        scoreMultiGradeImgP1.enabled = false;
        scoreMultiPlayerImgP2.enabled = false;
        scoreMultiGradeImgP2.enabled = false;

        LevelStats levelStats = rsc.statsMng.GetCurrentLevelStats();
        int totalTime = rsc.statsMng.GetTotalTime();

        int currentChainP1 = 0;
        int totalScoreP1 = 0;
        int currentChainP2 = 0;
        int totalScoreP2 = 0;

        int currentAccuracyP1 = 0;
        int percentScoreP1 = 0;
        int currentAccuracyP2 = 0;
        int percentScoreP2 = 0;

        int timeScoreP1 = 0;
        int timeScoreP2 = 0;

        float elapsedTime = 0f;
        float factor = 0f;

        scoreMultiChainP1.text = totalScoreP1.ToString();
        scoreMultiAccuracyP1.text = totalScoreP1.ToString();
        scoreMultiTotalP1.text = totalScoreP1.ToString();
        scoreMultiChainP2.text = totalScoreP2.ToString();
        scoreMultiAccuracyP2.text = totalScoreP2.ToString();
        scoreMultiTotalP2.text = totalScoreP2.ToString();
        scoreMultiTime.text = levelStats.baseSeconds.ToString();

        scoreMultiGO.SetActive(true);

        yield return new WaitForSeconds(scoreInitialDelay);

        //Chain update
        elapsedTime += Time.deltaTime;
        factor = elapsedTime / scoreChainDuration;

        while (elapsedTime < scoreChainDuration)
        {
            currentChainP1 = (int)Mathf.Lerp(0, player1Stats.maxChain, factor);
            totalScoreP1 = currentChainP1 * levelStats.maxChainMultiplier;

            scoreMultiChainP1.text = currentChainP1.ToString();
            scoreMultiTotalP1.text = string.Format("{0:n0}", totalScoreP1);

            currentChainP2 = (int)Mathf.Lerp(0, player2Stats.maxChain, factor);
            totalScoreP2 = currentChainP2 * levelStats.maxChainMultiplier;

            scoreMultiChainP2.text = currentChainP2.ToString();
            scoreMultiTotalP2.text = string.Format("{0:n0}", totalScoreP2);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreChainDuration;
        }

        totalScoreP1 = player1Stats.maxChain * levelStats.maxChainMultiplier;
        scoreMultiChainP1.text = player1Stats.maxChain.ToString();
        scoreMultiTotalP1.text = string.Format("{0:n0}", player1Stats.maxChain * levelStats.maxChainMultiplier);

        totalScoreP2 = player2Stats.maxChain * levelStats.maxChainMultiplier;
        scoreMultiChainP2.text = player2Stats.maxChain.ToString();
        scoreMultiTotalP2.text = string.Format("{0:n0}", player2Stats.maxChain * levelStats.maxChainMultiplier);
        yield return null;

        //Accuracy update
        elapsedTime = 0f;
        factor = 0f;

        while (elapsedTime < scoreAccuracyDuration)
        {
            currentAccuracyP1 = (int)Mathf.Lerp(0, player1Stats.colorAccuracy, factor);
            percentScoreP1 = totalScoreP1 / 100 * currentAccuracyP1;

            scoreMultiAccuracyP1.text = currentAccuracyP1.ToString();
            scoreMultiTotalP1.text = string.Format("{0:n0}", percentScoreP1);

            currentAccuracyP2 = (int)Mathf.Lerp(0, player2Stats.colorAccuracy, factor);
            percentScoreP2 = totalScoreP2 / 100 * currentAccuracyP2;

            scoreMultiAccuracyP2.text = currentAccuracyP2.ToString();
            scoreMultiTotalP2.text = string.Format("{0:n0}", percentScoreP2);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreAccuracyDuration;
        }

        percentScoreP1 = totalScoreP1 / 100 * player1Stats.colorAccuracy;
        scoreMultiAccuracyP1.text = player1Stats.colorAccuracy.ToString();
        scoreMultiTotalP2.text = string.Format("{0:n0}", percentScoreP1);

        percentScoreP2 = totalScoreP2 / 100 * player2Stats.colorAccuracy;
        scoreMultiAccuracyP2.text = player2Stats.colorAccuracy.ToString();
        scoreMultiTotalP2.text = string.Format("{0:n0}", percentScoreP2);
        yield return null;

        //Time update
        elapsedTime = 0f;
        factor = 0f;

        if (totalTime < levelStats.baseSeconds)
            scoreMultiTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.GREEN);
        else if (totalTime > levelStats.baseSeconds)
            scoreMultiTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.RED);

        while (elapsedTime < scoreTimeDuration)
        {
            int seconds = (int)Mathf.Lerp(levelStats.baseSeconds, totalTime, factor);

            timeScoreP1 = percentScoreP1 + ((levelStats.baseSeconds - seconds) * levelStats.secondMultiplier);
            timeScoreP2 = percentScoreP2 + ((levelStats.baseSeconds - seconds) * levelStats.secondMultiplier);

            scoreMultiTime.text = seconds.ToString();
            scoreMultiTotalP1.text = string.Format("{0:n0}", timeScoreP1);
            scoreMultiTotalP2.text = string.Format("{0:n0}", timeScoreP2);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreTimeDuration;
        }

        timeScoreP1 = percentScoreP1 + ((levelStats.baseSeconds - totalTime) * levelStats.secondMultiplier);
        timeScoreP2 = percentScoreP2 + ((levelStats.baseSeconds - totalTime) * levelStats.secondMultiplier);
        scoreMultiTime.text = totalTime.ToString();
        scoreMultiTotalP1.text = string.Format("{0:n0}", timeScoreP1);
        scoreMultiTotalP2.text = string.Format("{0:n0}", timeScoreP2);

        //Player Image fading
        elapsedTime = 0f;
        factor = 0f;
        scoreMultiPlayerImgP1.enabled = true;
        scoreMultiPlayerImgP1.color = new Color(1, 1, 1, 0);
        scoreMultiPlayerImgP1.sprite = player1ScoreImages[(int)player1Stats.finalGrade];
        scoreMultiPlayerImgP2.enabled = true;
        scoreMultiPlayerImgP2.color = new Color(1, 1, 1, 0);
        scoreMultiPlayerImgP2.sprite = player2ScoreImages[(int)player2Stats.finalGrade];

        while (elapsedTime < showPlayerImgDuration)
        {
            float alpha = Mathf.Lerp(0, 1, factor);
            scoreMultiPlayerImgP1.color = new Color(1, 1, 1, alpha);
            scoreMultiPlayerImgP2.color = new Color(1, 1, 1, alpha);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / showPlayerImgDuration;
        }

        scoreMultiPlayerImgP1.color = Color.white;
        scoreMultiPlayerImgP2.color = Color.white;

        //Grade Img Stamp
        elapsedTime = 0;
        factor = 0;
        scoreMultiGradeImgP1.enabled = true;
        scoreMultiGradeImgP1.transform.localScale = new Vector3(showGradeInitialScale, showGradeInitialScale, 1f);
        scoreMultiGradeImgP1.sprite = gradeScoreImages[(int)player1Stats.finalGrade];
        scoreMultiGradeImgP2.enabled = true;
        scoreMultiGradeImgP2.transform.localScale = new Vector3(showGradeInitialScale, showGradeInitialScale, 1f);
        scoreMultiGradeImgP2.sprite = gradeScoreImages[(int)player2Stats.finalGrade];

        while (elapsedTime < showGradeImgDuration)
        {
            float scale = Mathf.Lerp(showGradeInitialScale, 1, factor);
            scoreMultiGradeImgP1.transform.localScale = new Vector3(scale, scale, 1f);
            scoreMultiGradeImgP2.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / showGradeImgDuration;
        }

        scoreMultiGradeImgP1.transform.localScale = Vector3.one;
        scoreMultiGradeImgP2.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(scoreFinalDelay);

        scoreContinueHintGO.SetActive(true);
        rsc.eventMng.TriggerEvent(EventManager.EventType.SCORE_OPENED, EventInfo.emptyInfo);
    }

    private void HideScore(EventInfo eventInfo)
    {
        if (scoreGO.activeSelf)
        {
            //scoreGO.SetActive(false);
            //infoArea.SetActive(true);
            rsc.eventMng.TriggerEvent(EventManager.EventType.SCORE_CLOSED, EventInfo.emptyInfo);
        }
    }

    private void FadeCurtain(EventInfo eventInfo)
    {
        FadeCurtainEventInfo info = (FadeCurtainEventInfo)eventInfo;

        if(info.fadeIn)
        {
            fadeCurtain.StartFadingToClear((info.useDefaultColor ? fadeCurtain.defaultFadeColor : info.fadeColor),
                                           (info.useDefaultTime ? fadeCurtain.defaultFadeSeconds : info.fadeTime));
        }
        else
        {
            fadeCurtain.StartFadingToColor((info.useDefaultColor ? fadeCurtain.defaultFadeColor : info.fadeColor),
                                           (info.useDefaultTime ? fadeCurtain.defaultFadeSeconds : info.fadeTime));
        }
    }

    #region Boss UI Management
    private void InitBossValues()
    {
        if (wormBb != null)
        {
            headSlider.maxValue = wormBb.headMaxChargeLevel;
            headSlider.value = wormBb.headCurrentChargeLevel;

            tail.color = darkGrey;

            SetBossDisabled();
        }
    }

    private void StopAllBossCoroutines()
    {
        //Stop all boss-related coroutines

        if (bodyBlinkCoroutine != null)
        {
            StopCoroutine(bodyBlinkCoroutine);
            bodyBlinkCoroutine = null;
        }

        if (headBlinkCoroutine != null)
        {
            StopCoroutine(headBlinkCoroutine);
            headBlinkCoroutine = null;
        }

        if (specialBlinkCoroutine != null)
        {
            StopCoroutine(specialBlinkCoroutine);
            specialBlinkCoroutine = null;
        }

        if (diedBlinkCoroutine != null)
        {
            StopCoroutine(diedBlinkCoroutine);
            diedBlinkCoroutine = null;
        }
    }

    private void SetBossDisabled()
    {
        StopAllBossCoroutines();

        bossUIState = BossUIState.INVULNERABLE;

        //Set boss elements
        //Sign
        bossSpecialSign.enabled = false;

        //Head
        head.enabled = true;
        head.sprite = headHollowImage;
        head.color = Color.black;
        headSliderGO.SetActive(true);

        //Body
        for (int i = 0; i < bodyParts.Length; ++i)
        {
            bodyParts[i].sprite = bodyFillImage;
            bodyParts[i].color = darkGrey;
        }
    }

    private void SetBossEnabled()
    {
        if (wormBb == null) return;

        StopAllBossCoroutines();

        switch (wormBb.head.HeadState)
        {
            case WormAIBehaviour.HeadSubState.DEACTIVATED:
                bossUIState = BossUIState.BODY_ACTIVATED;
                SetBodyActivated();
                break;

            case WormAIBehaviour.HeadSubState.ACTIVATED:
                bossUIState = BossUIState.HEAD_ACTIVATED;
                SetHeadActivated();
                break;

            case WormAIBehaviour.HeadSubState.KNOCKED_OUT:
                bossUIState = BossUIState.HEAD_STUNNED;
                SetHeadActivated();
                break;

            default:
                break;
        }
    }

    private void SetBodyActivated()
    {
        //Set boss elements
        //Sign
        bossSpecialSign.enabled = false;

        head.enabled = true;
        head.sprite = headHollowImage;
        head.color = Color.black;
        headSliderGO.SetActive(true);
        headSlider.value = wormBb.headCurrentChargeLevel;

        for (int i = 0; i < bodyParts.Length; ++i)
        {
            switch (bodySegmentControllers[i].BodyState)
            {
                case WormBodySegmentController.BodySubState.SETTING:
                case WormBodySegmentController.BodySubState.NORMAL:
                    bodyParts[i].sprite = bodyFillImage;
                    bodyParts[i].color = rsc.coloredObjectsMng.GetColor(bodySegmentControllers[i].Color);
                    break;

                case WormBodySegmentController.BodySubState.NORMAL_DISABLED:
                case WormBodySegmentController.BodySubState.DEACTIVATED:
                    bodyParts[i].sprite = bodyFillImage;
                    bodyParts[i].color = darkGrey;
                    break;

                case WormBodySegmentController.BodySubState.DESTROYED:
                    bodyParts[i].sprite = bodyHollowImage;
                    bodyParts[i].color = lightGrey;
                    break;

                default:
                    break;
            }
        }

        bodyBlinkCoroutine = StartCoroutine(BossBodyBlink());
    }

    private IEnumerator BossBodyBlink()
    {
        bool white = false;

        while (true)
        {
            for (int i = 0; i < bodyParts.Length; ++i)
            {
                if (bodySegmentControllers[i].BodyState != WormBodySegmentController.BodySubState.DEACTIVATED &&
                    bodySegmentControllers[i].BodyState != WormBodySegmentController.BodySubState.DESTROYED)
                {
                    if (bodySegmentControllers[i].Color == currentChromaColor)
                    {
                        if (white)
                            bodyParts[i].color = Color.white;
                        else
                            bodyParts[i].color = rsc.coloredObjectsMng.GetColor(bodySegmentControllers[i].Color);
                    }
                    else
                        bodyParts[i].color = rsc.coloredObjectsMng.GetColor(bodySegmentControllers[i].Color);
                }
            }

            white = !white;
            yield return new WaitForSeconds(bossBodyBlinkInterval);
        }
    }

    private void SetHeadActivated()
    {
        //Set boss elements
        head.sprite = headFillImage;
        head.color = Color.white;
        headSliderGO.SetActive(false);

        for (int i = 0; i < bodyParts.Length; ++i)
        {
            bodyParts[i].sprite = bodyFillImage;
            bodyParts[i].color = darkGrey;
        }

        if (bossUIState == BossUIState.HEAD_STUNNED)
        {
            headBlinkCoroutine = StartCoroutine(BossHeadBlink(bossHeadStunnedBlinkInterval));
            specialBlinkCoroutine = StartCoroutine(BossSpecialBlink());
        }
        else
        {
            headBlinkCoroutine = StartCoroutine(BossHeadBlink(bossHeadBlinkInterval));
        }
    }

    private IEnumerator BossHeadBlink(float interval)
    {
        bool visible = true;

        while (true)
        {
            head.enabled = visible;

            visible = !visible;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator BossSpecialBlink()
    {
        bool visible = true;

        while (true)
        {
            bossSpecialSign.enabled = visible;

            visible = !visible;
            yield return new WaitForSeconds(bossSpecialBlinkInterval);
        }
    }

    private void SetBossDying()
    {
        StopAllBossCoroutines();

        bossUIState = BossUIState.DESTROYED;

        //Set boss elements
        head.sprite = headHollowImage;
        head.color = lightGrey;
        headSliderGO.SetActive(false);

        for (int i = 0; i < bodyParts.Length; ++i)
        {
            bodyParts[i].sprite = bodyHollowImage;
            bodyParts[i].color = lightGrey;
        }

        tail.sprite = tailHollowImage;
        tail.color = lightGrey;

        diedBlinkCoroutine = StartCoroutine(BossDiedBlink());
    }

    private IEnumerator BossDiedBlink()
    {
        bool visible = true;

        while (true)
        {
            head.enabled = visible;
            for (int i = 0; i < bodyParts.Length; ++i)
                bodyParts[i].enabled = visible;

            tail.enabled = visible;

            visible = !visible;
            yield return new WaitForSeconds(bossDiedBlinkInterval);
        }
    }

    private void BossInvulnerable(EventInfo eventInfo)
    {
        Debug.Log("Boss Invulnerable");
        SetBossDisabled();      
    }

    private void BossVulnerable(EventInfo eventInfo)
    {
        Debug.Log("Boss Vulnerable");
        SetBossEnabled();       
    }

    private void BossHeadChargeChanged(EventInfo eventInfo)
    {
        Debug.Log("Boss Section Destroyed");
        headSlider.value = wormBb.headCurrentChargeLevel;
        SetBossEnabled();
    }

    private void BossHeadActivated(EventInfo eventInfo)
    {
        Debug.Log("Boss Head Activated");
        SetBossEnabled();
    }

    private void BossHeadStunned(EventInfo eventInfo)
    {
        Debug.Log("Boss Head Stunned");
        SetBossEnabled();
    }

    private void BossHeadDeactivated(EventInfo eventInfo)
    {
        Debug.Log("Boss Head Deactivated");   
        SetBossEnabled();
    }

    private void BossPhaseEnded(EventInfo eventInfo)
    {
        Debug.Log("Boss Phase Ended");
        SetBossDisabled();
    }

    private void BossDying(EventInfo eventInfo)
    {
        Debug.Log("Boss dying");
        SetBossDying();      
    }

    public void BossDied(EventInfo eventInfo)
    {
        Debug.Log("Boss died");
        StopAllBossCoroutines();
        bossZone.SetActive(false);
    }
    #endregion
}
