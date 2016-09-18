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
    public float scoreFinalDelay = 0.2f;

    [Header("Score Items")]
    public GameObject scoreGO;
    public GameObject scoreContinueHintGO;
    public GameObject scoreSingleGO;
    public Text scoreSingleChain;
    public Text scoreSingleAccuracy;
    public Text scoreSingleTime;
    public Text scoreSingleTotal;
    public GameObject scoreMultiGO;
    public Text scoreMultiChainP1;
    public Text scoreMultiAccuracyP1;
    public Text scoreMultiTotalP1;
    public Text scoreMultiChainP2;
    public Text scoreMultiAccuracyP2;
    public Text scoreMultiTotalP2;
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

    [Header("Pause Items")]
    public GameObject pauseGroup;
    public GameObject optionsGroup;
    public GameObject confirmationGroup;
    public Button continueBtn;
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

        //Set initial values
        currentColor = rsc.coloredObjectsMng.GetColor(rsc.colorMng.CurrentColor);
        nextColorPrewarnTime = rsc.colorMng.prewarningSeconds;
        nextColorElapsedTime = rsc.colorMng.ElapsedTime;

        float factor = nextColorElapsedTime / nextColorPrewarnTime;

        nextColorSlider.value = Mathf.Lerp(0f, 1f, factor);
        nextColorBackground.color = Color.Lerp(currentColor, Color.black, factor * 2);
        nextColorForeground.color = rsc.coloredObjectsMng.GetColor(rsc.colorMng.NextColor);
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
        int currentChain = 0;
        int totalScore = 0;

        int currentAccuracy = 0;
        int percentScore = 0;

        int levelTime = rsc.statsMng.GetCurrentLevelBaseTime();
        int actualTime = rsc.statsMng.GetTotalTime();
        int timeScore = 0;

        float elapsedTime = 0f;
        float factor = 0f;

        scoreSingleChain.text = totalScore.ToString();
        scoreSingleAccuracy.text = totalScore.ToString();
        scoreSingleTime.text = levelTime.ToString();
        scoreSingleTotal.text = totalScore.ToString();

        scoreSingleGO.SetActive(true);

        yield return new WaitForSeconds(scoreInitialDelay);

        //Chain update
        elapsedTime += Time.deltaTime;
        factor = elapsedTime / scoreChainDuration;

        while(elapsedTime < scoreChainDuration)
        {
            currentChain = (int)Mathf.Lerp(0, rsc.statsMng.p1Stats.maxChain, factor);
            totalScore = currentChain * rsc.statsMng.maxChainMultiplier;

            scoreSingleChain.text = currentChain.ToString();
            scoreSingleTotal.text = string.Format("{0:n0}", totalScore);
            Debug.Log("Chain: " + currentChain + " // Total: " + totalScore);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreChainDuration;
        }

        totalScore = rsc.statsMng.p1Stats.maxChain * rsc.statsMng.maxChainMultiplier;
        scoreSingleChain.text = rsc.statsMng.p1Stats.maxChain.ToString();
        scoreSingleTotal.text = string.Format("{0:n0}", rsc.statsMng.p1Stats.maxChain * rsc.statsMng.maxChainMultiplier);
        yield return null;

        //Accuracy update
        elapsedTime = 0f;
        factor = 0f;

        while(elapsedTime < scoreAccuracyDuration)
        {
            currentAccuracy = (int)Mathf.Lerp(0, rsc.statsMng.p1Stats.colorAccuracy, factor);
            percentScore = totalScore / 100 * currentAccuracy;

            scoreSingleAccuracy.text = currentAccuracy.ToString();
            scoreSingleTotal.text = string.Format("{0:n0}", percentScore);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreAccuracyDuration;
        }

        percentScore = totalScore / 100 * rsc.statsMng.p1Stats.colorAccuracy;
        scoreSingleAccuracy.text = rsc.statsMng.p1Stats.colorAccuracy.ToString();
        scoreSingleTotal.text = string.Format("{0:n0}", percentScore);
        yield return null;

        //Time update
        elapsedTime = 0f;
        factor = 0f;

        if (actualTime < levelTime)
            scoreSingleTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.GREEN);
        else if (actualTime > levelTime)
            scoreSingleTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.RED);

        while (elapsedTime < scoreTimeDuration)
        {
            int seconds = (int)Mathf.Lerp(levelTime, actualTime, factor);
            timeScore = percentScore + ((levelTime - seconds) * rsc.statsMng.secondMultiplier);

            scoreSingleTime.text = seconds.ToString();
            scoreSingleTotal.text = string.Format("{0:n0}", timeScore);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreTimeDuration;
        }

        timeScore = percentScore + ((levelTime - actualTime) * rsc.statsMng.secondMultiplier);
        scoreSingleTime.text = actualTime.ToString();
        scoreSingleTotal.text = string.Format("{0:n0}", timeScore);

        yield return new WaitForSeconds(scoreFinalDelay);

        scoreContinueHintGO.SetActive(true);
        rsc.eventMng.TriggerEvent(EventManager.EventType.SCORE_OPENED, EventInfo.emptyInfo);
    }

    private IEnumerator ShowScoreMultiProgress()
    {
        int currentChainP1 = 0;
        int totalScoreP1 = 0;
        int currentChainP2 = 0;
        int totalScoreP2 = 0;

        int currentAccuracyP1 = 0;
        int percentScoreP1 = 0;
        int currentAccuracyP2 = 0;
        int percentScoreP2 = 0;

        int levelTime = rsc.statsMng.GetCurrentLevelBaseTime();
        int actualTime = rsc.statsMng.GetTotalTime();
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
        scoreMultiTime.text = levelTime.ToString();

        scoreMultiGO.SetActive(true);

        yield return new WaitForSeconds(scoreInitialDelay);

        //Chain update
        elapsedTime += Time.deltaTime;
        factor = elapsedTime / scoreChainDuration;

        while (elapsedTime < scoreChainDuration)
        {
            currentChainP1 = (int)Mathf.Lerp(0, rsc.statsMng.p1Stats.maxChain, factor);
            totalScoreP1 = currentChainP1 * rsc.statsMng.maxChainMultiplier;

            scoreMultiChainP1.text = currentChainP1.ToString();
            scoreMultiTotalP1.text = string.Format("{0:n0}", totalScoreP1);

            currentChainP2 = (int)Mathf.Lerp(0, rsc.statsMng.p2Stats.maxChain, factor);
            totalScoreP2 = currentChainP2 * rsc.statsMng.maxChainMultiplier;

            scoreMultiChainP2.text = currentChainP2.ToString();
            scoreMultiTotalP2.text = string.Format("{0:n0}", totalScoreP2);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreChainDuration;
        }

        totalScoreP1 = rsc.statsMng.p1Stats.maxChain * rsc.statsMng.maxChainMultiplier;
        scoreMultiChainP1.text = rsc.statsMng.p1Stats.maxChain.ToString();
        scoreMultiTotalP1.text = string.Format("{0:n0}", rsc.statsMng.p1Stats.maxChain * rsc.statsMng.maxChainMultiplier);

        totalScoreP2 = rsc.statsMng.p2Stats.maxChain * rsc.statsMng.maxChainMultiplier;
        scoreMultiChainP2.text = rsc.statsMng.p2Stats.maxChain.ToString();
        scoreMultiTotalP2.text = string.Format("{0:n0}", rsc.statsMng.p2Stats.maxChain * rsc.statsMng.maxChainMultiplier);
        yield return null;

        //Accuracy update
        elapsedTime = 0f;
        factor = 0f;

        while (elapsedTime < scoreAccuracyDuration)
        {
            currentAccuracyP1 = (int)Mathf.Lerp(0, rsc.statsMng.p1Stats.colorAccuracy, factor);
            percentScoreP1 = totalScoreP1 / 100 * currentAccuracyP1;

            scoreMultiAccuracyP1.text = currentAccuracyP1.ToString();
            scoreMultiTotalP1.text = string.Format("{0:n0}", percentScoreP1);

            currentAccuracyP2 = (int)Mathf.Lerp(0, rsc.statsMng.p2Stats.colorAccuracy, factor);
            percentScoreP2 = totalScoreP2 / 100 * currentAccuracyP2;

            scoreMultiAccuracyP2.text = currentAccuracyP2.ToString();
            scoreMultiTotalP2.text = string.Format("{0:n0}", percentScoreP2);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreAccuracyDuration;
        }

        percentScoreP1 = totalScoreP1 / 100 * rsc.statsMng.p1Stats.colorAccuracy;
        scoreMultiAccuracyP1.text = rsc.statsMng.p1Stats.colorAccuracy.ToString();
        scoreMultiTotalP2.text = string.Format("{0:n0}", percentScoreP1);

        percentScoreP2 = totalScoreP2 / 100 * rsc.statsMng.p2Stats.colorAccuracy;
        scoreMultiAccuracyP2.text = rsc.statsMng.p2Stats.colorAccuracy.ToString();
        scoreMultiTotalP2.text = string.Format("{0:n0}", percentScoreP2);
        yield return null;

        //Time update
        elapsedTime = 0f;
        factor = 0f;

        if (actualTime < levelTime)
            scoreMultiTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.GREEN);
        else if (actualTime > levelTime)
            scoreMultiTime.color = rsc.coloredObjectsMng.GetColor(ChromaColor.RED);

        while (elapsedTime < scoreTimeDuration)
        {
            int seconds = (int)Mathf.Lerp(levelTime, actualTime, factor);

            timeScoreP1 = percentScoreP1 + ((levelTime - seconds) * rsc.statsMng.secondMultiplier);
            timeScoreP2 = percentScoreP2 + ((levelTime - seconds) * rsc.statsMng.secondMultiplier);

            scoreMultiTime.text = seconds.ToString();
            scoreMultiTotalP1.text = string.Format("{0:n0}", timeScoreP1);
            scoreMultiTotalP2.text = string.Format("{0:n0}", timeScoreP2);

            yield return null;

            elapsedTime += Time.deltaTime;
            factor = elapsedTime / scoreTimeDuration;
        }

        timeScoreP1 = percentScoreP1 + ((levelTime - actualTime) * rsc.statsMng.secondMultiplier);
        timeScoreP2 = percentScoreP2 + ((levelTime - actualTime) * rsc.statsMng.secondMultiplier);
        scoreMultiTime.text = actualTime.ToString();
        scoreMultiTotalP1.text = string.Format("{0:n0}", timeScoreP1);
        scoreMultiTotalP2.text = string.Format("{0:n0}", timeScoreP2);

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
}
