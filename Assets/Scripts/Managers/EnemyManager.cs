using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    private const string playerTargetTxt = "player";
    private const string leaderTargetTxt = "leader";
    private const float spiderBiteDefaultWaitTime = 2f;
    private const float spiderIdleDefaultTime = 1f;

    public GlobalAIBlackboard blackboard;

    //TODO: Jordi test, delete when finished
    private List<AIAction> jordiTestEntry01 = new List<AIAction>();
    private List<AIAction> jordiTestAttack01 = new List<AIAction>();

    public List<AIAction> jordiTestLeader01 = new List<AIAction>();
    public List<AIAction> jordiTestFollower01_135_5 = new List<AIAction>();
    public List<AIAction> jordiTestFollower02_225_5 = new List<AIAction>();

    //Action list for the vortex spawned spiders
    public List<AIAction> defaultSpiderEntry = new List<AIAction>();
    public List<AIAction> defaultSpiderAttack = new List<AIAction>();
    public List<AIAction> defaultRedSpiderAttack = new List<AIAction>();
    public List<AIAction> defaultGreenSpiderAttack = new List<AIAction>();
    public List<AIAction> defaultBlueSpiderAttack = new List<AIAction>();
    public List<AIAction> defaultYellowSpiderAttack = new List<AIAction>();

    //Actions lists
    private List<AIAction> level01Z01SpiderEntry01 = new List<AIAction>();
    private List<AIAction> level01Z01SpiderEntry02 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry03 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry04 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry05 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry06 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry07 = new List<AIAction>();

    private List<AIAction> level01Z01spiderAttack03 = new List<AIAction>();
    private List<AIAction> level01Z01spiderAttack04 = new List<AIAction>();
    private List<AIAction> level01Z01spiderAttack05 = new List<AIAction>();
    private List<AIAction> level01Z01spiderAttack06 = new List<AIAction>();
    private List<AIAction> level01Z01spiderAttack07 = new List<AIAction>();

    private List<AIAction> level01Z02spiderEntry01a = new List<AIAction>();
    //private List<AIAction> level01Z02spiderEntry01b = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry02a = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry02b = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry03a = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry03b = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry04a = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry04b = new List<AIAction>();

    private List<AIAction> level01Z02spiderAttack02a = new List<AIAction>();
    private List<AIAction> level01Z02spiderAttack02b = new List<AIAction>();
    private List<AIAction> level01Z02spiderAttack03a = new List<AIAction>();
    private List<AIAction> level01Z02spiderAttack03b = new List<AIAction>();
    private List<AIAction> level01Z02spiderAttack04a = new List<AIAction>();
    private List<AIAction> level01Z02spiderAttack04b = new List<AIAction>();

    private List<AIAction> level01Z03spiderEntry01a = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry01b = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry02a = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry02b = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry02c = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry03a = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry03b = new List<AIAction>();

    private List<AIAction> level01Z03spiderAttack02a = new List<AIAction>();
    private List<AIAction> level01Z03spiderAttack02b = new List<AIAction>();
    private List<AIAction> level01Z03spiderAttack02c = new List<AIAction>();
    private List<AIAction> level01Z03spiderAttack03a = new List<AIAction>();
    private List<AIAction> level01Z03spiderAttack03b = new List<AIAction>();

    //...

    //Example Zone Plans
    private ZonePlan plan0101;
    private ZonePlan plan0102;
    private ZonePlan plan0103;

    private ZonePlan currentPlan;
    private int currentPlanId;
    private int executingWaves;
    private int sequentialWavesIndex;


    void Awake()
    {
        Debug.Log("Enemy Manager created");
        blackboard = new GlobalAIBlackboard();
        blackboard.InitValues();

        currentPlan = null;
        currentPlanId = -1;

        CreateAIActionLists();
        CreateZonePlans();
    }

    void Start()
    {     
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_SPAWNED, EnemySpawned);
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_DIED, EnemyDied);

        rsc.eventMng.StartListening(EventManager.EventType.TURRET_SPAWNED, TurretSpawned);
        rsc.eventMng.StartListening(EventManager.EventType.TURRET_DESTROYED, TurretDestroyed);

        rsc.eventMng.StartListening(EventManager.EventType.VORTEX_ACTIVATED, VortexActivated);
        rsc.eventMng.StartListening(EventManager.EventType.VORTEX_DESTROYED, VortexDestroyed);

        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameOver);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_SPAWNED, EnemySpawned);
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_DIED, EnemyDied);

            rsc.eventMng.StopListening(EventManager.EventType.TURRET_SPAWNED, TurretSpawned);
            rsc.eventMng.StopListening(EventManager.EventType.TURRET_DESTROYED, TurretDestroyed);

            rsc.eventMng.StopListening(EventManager.EventType.VORTEX_ACTIVATED, VortexActivated);
            rsc.eventMng.StopListening(EventManager.EventType.VORTEX_DESTROYED, VortexDestroyed);

            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameOver);
        }

        Debug.Log("Color Manager destroyed");
    }

    private void EnemySpawned(EventInfo eventInfo)
    {
        ++blackboard.activeEnemies;
        //Debug.Log("Enemies++ = " + blackboard.activeEnemies);
    }

    private void EnemyDied(EventInfo eventInfo)
    {
        --blackboard.activeEnemies;
        //Debug.Log("Enemies-- = " + blackboard.activeEnemies);
    }

    private void TurretSpawned(EventInfo eventInfo)
    {
        ++blackboard.activeTurrets;
    }

    private void TurretDestroyed(EventInfo eventInfo)
    {
        --blackboard.activeTurrets;
    }

    private void VortexActivated(EventInfo eventInfo)
    {
        ++blackboard.activeVortex;
        Debug.Log("Vortex++ = " + blackboard.activeVortex);
    }

    private void VortexDestroyed(EventInfo eventInfo)
    {
        --blackboard.activeVortex;
        Debug.Log("Vortex-- = " + blackboard.activeVortex);
    }

    private void GameOver(EventInfo eventInfo)
    {
        StopCurrentPlan();
    }

    public void StartPlan(int planId)
    {
        if(currentPlan != null)
        {
            Debug.Log("There is a current plan active");
            return;
        }

        switch(planId)
        {
            case 0101:
                currentPlan = plan0101;
                break;
            case 0102:
                currentPlan = plan0102;
                break;
            case 0103:
                currentPlan = plan0103;
                break;
        }

        currentPlanId = planId;

        blackboard.activeEnemies = 0;
        blackboard.activeTurrets = 0;
        blackboard.activeVortex = 0;

        executingWaves = 0;
        sequentialWavesIndex = 0;

        //Execute plan initial actions
        foreach(WaveAction action in currentPlan.initialActions)
        {
            action.Execute();
        }

        if (sequentialWavesIndex < currentPlan.sequentialWaves.Count)
        {
            StartCoroutine(CastWave(currentPlan.sequentialWaves[sequentialWavesIndex]));
        }
    }

    public void StopCurrentPlan()
    {
        if(currentPlan != null)
        {
            StopAllCoroutines();
            currentPlan = null;
            currentPlanId = -1;
        }
    }

    private IEnumerator CastWave(List<WaveAction> actions)
    {
        ++executingWaves;

        foreach(WaveAction action in actions)
        {
            if(action.InitialDelay > 0)
            {
                yield return new WaitForSeconds(action.InitialDelay);
            }
            action.Execute();
            while (action.Executing)
                yield return null;
        }

        --executingWaves;
    }

    private bool PlanEnded()
    {
        if (currentPlan == null)
            return true;

        return (blackboard.activeEnemies + blackboard.activeTurrets + blackboard.activeVortex + executingWaves == 0) && (sequentialWavesIndex == currentPlan.sequentialWaves.Count);
    }

    void Update()
    {
        if(currentPlan != null)
        {
            if(PlanEnded())
            {
                ZonePlanEndedInfo.eventInfo.planId = currentPlanId;
                rsc.eventMng.TriggerEvent(EventManager.EventType.ZONE_PLAN_FINISHED, ZonePlanEndedInfo.eventInfo);
                currentPlan = null;
                currentPlanId = -1;
            }
            else
            {
                if(blackboard.activeEnemies < currentPlan.enemiesThreshold && executingWaves == 0 && sequentialWavesIndex < currentPlan.sequentialWaves.Count)
                {
                    ++sequentialWavesIndex;
                    if(sequentialWavesIndex < currentPlan.sequentialWaves.Count)
                    {
                        StartCoroutine(CastWave(currentPlan.sequentialWaves[sequentialWavesIndex]));
                    }
                }
            }
        }
    }

    public GameObject SelectTarget(GameObject origin)
    {
        if(rsc.gameInfo.numberOfPlayers == 1)
            return rsc.gameInfo.player1;
        else
        {
            //If both players active, return the closest one
            if (rsc.gameInfo.player1Controller.Active && rsc.gameInfo.player2Controller.Active)
            {
                float toPlayer1 = Vector3.Distance(origin.transform.position, rsc.gameInfo.player1.transform.position);
                float toPlayer2 = Vector3.Distance(origin.transform.position, rsc.gameInfo.player2.transform.position);
                if (toPlayer2 > toPlayer1)
                    return rsc.gameInfo.player2;
                else
                    return rsc.gameInfo.player1;
            }
            else if (rsc.gameInfo.player2Controller.Active)
                return rsc.gameInfo.player2;
            else
                return rsc.gameInfo.player1;
        }
    }


    void CreateAIActionLists()
    {
        //TODO: JORDI TEST delete when finished
        jordiTestEntry01.Add(new MoveAIAction("Z01WP01", 10f));
        jordiTestEntry01.Add(new MoveAIAction("Z01WP07", 10f));
        jordiTestEntry01.Add(new MoveAIAction("Z01WP04", 10f, true, 0f, -3));

        jordiTestAttack01.Add(new SelectPlayerAIAction());
        jordiTestAttack01.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        jordiTestAttack01.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        jordiTestAttack01.Add(new StandingIdleAIAction(spiderIdleDefaultTime));


        //public List<AIAction> jordiTestLeader01 = new List<AIAction>();
        //public List<AIAction> jordiTestFollower01_135_5 = new List<AIAction>();
        //public List<AIAction> jordiTestFollower02_225_5 = new List<AIAction>();
        jordiTestLeader01.Add(new MoveAIAction("Z01WP01", 8f));
        //jordiTestLeader01.Add(new LookAtAIAction("Z01WP08"));
        //jordiTestLeader01.Add(new StandingIdleAIAction(0.25f));
        jordiTestLeader01.Add(new MoveAIAction("Z01WP08", 8f));
        //jordiTestLeader01.Add(new LookAtAIAction("Z01WP04"));
        //jordiTestLeader01.Add(new StandingIdleAIAction(0.25f));
        jordiTestLeader01.Add(new MoveAIAction("Z01WP04", 8f));
        //jordiTestLeader01.Add(new LookAtAIAction("Z01WP05"));
        //jordiTestLeader01.Add(new StandingIdleAIAction(0.25f));
        jordiTestLeader01.Add(new MoveAIAction("Z01WP05", 8f));
        //jordiTestLeader01.Add(new LookAtAIAction("Z01WP06"));
        //jordiTestLeader01.Add(new StandingIdleAIAction(0.25f));
        jordiTestLeader01.Add(new MoveAIAction("Z01WP06", 8f));
        //jordiTestLeader01.Add(new LookAtAIAction("Z01WP07"));
        //jordiTestLeader01.Add(new StandingIdleAIAction(0.25f));
        jordiTestLeader01.Add(new MoveAIAction("Z01WP07", 8f));
        //jordiTestLeader01.Add(new LookAtAIAction("Z01WP02"));
        //jordiTestLeader01.Add(new StandingIdleAIAction(0.25f));
        jordiTestLeader01.Add(new MoveAIAction("Z01WP02", 8f));
        //jordiTestLeader01.Add(new LookAtAIAction("Z01WP01"));
        //jordiTestLeader01.Add(new StandingIdleAIAction(0.25f));

        jordiTestFollower01_135_5.Add(new MoveAIAction(leaderTargetTxt, 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_FORWARD_RELATIVE, 135, 3));
        jordiTestFollower01_135_5.Add(new MoveAIAction(leaderTargetTxt, 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_FORWARD_RELATIVE, 135, 3, false, 0, 1));

        jordiTestFollower02_225_5.Add(new MoveAIAction(leaderTargetTxt, 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_FORWARD_RELATIVE, 225, 3));
        jordiTestFollower02_225_5.Add(new MoveAIAction(leaderTargetTxt, 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_FORWARD_RELATIVE, 225, 3, false, 0, 1));
        //END TODO


        //vortexSpiderEntry has no actions, spider spawns and attacs player

        //default spider attack: select player and go straight to it, bite, wait and repeat
        defaultSpiderAttack.Add(new SelectPlayerAIAction());
        defaultSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        defaultSpiderAttack.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        defaultSpiderAttack.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        // default spider attack by color
        defaultRedSpiderAttack.Add(new SelectPlayerAIAction());
        defaultRedSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 5));
        defaultRedSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        defaultRedSpiderAttack.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        defaultRedSpiderAttack.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        defaultGreenSpiderAttack.Add(new SelectPlayerAIAction());
        defaultGreenSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 5));
        defaultGreenSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        defaultGreenSpiderAttack.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        defaultGreenSpiderAttack.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        defaultBlueSpiderAttack.Add(new SelectPlayerAIAction());
        defaultBlueSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 180, 5));
        defaultBlueSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        defaultBlueSpiderAttack.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        defaultBlueSpiderAttack.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        defaultYellowSpiderAttack.Add(new SelectPlayerAIAction());
        defaultYellowSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 5));
        defaultYellowSpiderAttack.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        defaultYellowSpiderAttack.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        defaultYellowSpiderAttack.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        // LEVEL 01 ZONE 01-------------------------------------------------------------------------------------------------------------------------------

        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP01", 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP02", 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP03", 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP06", 8f));
        level01Z01SpiderEntry01.Add(new SelectPlayerAIAction());
        level01Z01SpiderEntry01.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 20, 5f));
        level01Z01SpiderEntry01.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 0, 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 5f));
        level01Z01SpiderEntry01.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 0, 10f, true, 0f, 4));
        
        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP04", 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP05", 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP03", 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP06", 8f));
        level01Z01SpiderEntry02.Add(new SelectPlayerAIAction());
        level01Z01SpiderEntry02.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 20, 5f));
        level01Z01SpiderEntry02.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 0, 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 5f));
        level01Z01SpiderEntry02.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 0, 10f, true, 0f, 4));

        level01Z01spiderEntry03.Add(new SelectPlayerAIAction());
        level01Z01spiderEntry03.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 0, 3f));

        level01Z01spiderAttack03.Add(new MoveAIAction(playerTargetTxt, 20.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 20, 6f, false, 1f));
        level01Z01spiderAttack03.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderAttack03.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z01spiderAttack03.Add(new StandingIdleAIAction(spiderIdleDefaultTime));
        level01Z01spiderAttack03.Add(new SelectPlayerAIAction());
        level01Z01spiderAttack03.Add(new MoveAIAction(playerTargetTxt, 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 10f, false, 6f));

        level01Z01spiderEntry04.Add(new MoveAIAction("Z01WP07", 10f, true, 0f, AIAction.LIST_FINISHED));

        level01Z01spiderAttack04.Add(new SelectPlayerAIAction());
        level01Z01spiderAttack04.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderAttack04.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z01spiderAttack04.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        level01Z01spiderEntry05.Add(new MoveAIAction("Z01WP08", 10f, true, 0f, AIAction.LIST_FINISHED));

        level01Z01spiderAttack05.Add(new SelectPlayerAIAction());
        level01Z01spiderAttack05.Add(new MoveAIAction(playerTargetTxt, 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderAttack05.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z01spiderAttack05.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        level01Z01spiderAttack06.Add(new SelectPlayerAIAction());
        level01Z01spiderAttack06.Add(new MoveAIAction(playerTargetTxt, 6.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderAttack06.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z01spiderAttack06.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 10f, true, 5f));
        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 8f, true, 4f));
        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 6f, true, 4f));
        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 4f, true, 4f, AIAction.LIST_FINISHED));

        level01Z01spiderAttack07.Add(new SelectPlayerAIAction());
        level01Z01spiderAttack07.Add(new MoveAIAction(playerTargetTxt, 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderAttack07.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z01spiderAttack07.Add(new StandingIdleAIAction(spiderIdleDefaultTime));
        level01Z01spiderAttack07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 10f, true, 5f));
        level01Z01spiderAttack07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 8f, true, 4f));
        level01Z01spiderAttack07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 6f, true, 4f));
        level01Z01spiderAttack07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 4f, true, 4f));


        // LEVEL 01 ZONE 02-------------------------------------------------------------------------------------------------------------------------------

        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP01", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 45, 5f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP01", 12f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP01", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 5f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP02", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -135, 5f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP02", 12f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP02", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -45, 5f));

        level01Z02spiderEntry02a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -20, 10f));
        level01Z02spiderEntry02a.Add(new MoveAIAction("Z02WP01", 15f, true, 0f, AIAction.LIST_FINISHED));

        level01Z02spiderAttack02a.Add(new SelectPlayerAIAction());
        level01Z02spiderAttack02a.Add(new MoveAIAction(playerTargetTxt, 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 4f));
        level01Z02spiderAttack02a.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderAttack02a.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z02spiderAttack02a.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        level01Z02spiderEntry02b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -20, 10f));
        level01Z02spiderEntry02b.Add(new MoveAIAction("Z02WP01", 15f, true, 0f, AIAction.LIST_FINISHED));

        level01Z02spiderAttack02b.Add(new SelectPlayerAIAction());
        level01Z02spiderAttack02b.Add(new MoveAIAction(playerTargetTxt, 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 180, 4f));
        level01Z02spiderAttack02b.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderAttack02b.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z02spiderAttack02b.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        level01Z02spiderEntry03a.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderEntry03a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 7f));
        level01Z02spiderEntry03a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f, true, 0f, AIAction.LIST_FINISHED));

        level01Z02spiderAttack03a.Add(new SelectPlayerAIAction());
        level01Z02spiderAttack03a.Add(new MoveAIAction(playerTargetTxt, 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 4f));
        level01Z02spiderAttack03a.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderAttack03a.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z02spiderAttack03a.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        level01Z02spiderEntry03b.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderEntry03b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 7f));
        level01Z02spiderEntry03b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f, true, 0f, AIAction.LIST_FINISHED));

        level01Z02spiderAttack03b.Add(new SelectPlayerAIAction());
        level01Z02spiderAttack03b.Add(new MoveAIAction(playerTargetTxt, 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 4f));
        level01Z02spiderAttack03b.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderAttack03b.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z02spiderAttack03b.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        level01Z02spiderEntry04a.Add(new MoveAIAction("Z02WP02", 13f, true, 2f, AIAction.LIST_FINISHED));

        level01Z02spiderAttack04a.Add(new SelectPlayerAIAction());
        level01Z02spiderAttack04a.Add(new MoveAIAction(playerTargetTxt, 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderAttack04a.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z02spiderAttack04a.Add(new StandingIdleAIAction(spiderIdleDefaultTime));
        level01Z02spiderAttack04a.Add(new MoveAIAction("Z02WP02", 13f, true, 2f));

        level01Z02spiderEntry04b.Add(new MoveAIAction("Z02WP01", 13f, true, 2f, AIAction.LIST_FINISHED));

        level01Z02spiderAttack04b.Add(new SelectPlayerAIAction());
        level01Z02spiderAttack04b.Add(new MoveAIAction(playerTargetTxt, 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderAttack04b.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z02spiderAttack04b.Add(new StandingIdleAIAction(spiderIdleDefaultTime));
        level01Z02spiderAttack04b.Add(new MoveAIAction("Z02WP01", 13f, true, 2f));

        // LEVEL 01 ZONE 03-------------------------------------------------------------------------------------------------------------------------------

        level01Z03spiderEntry01a.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 20, 8));

        level01Z03spiderEntry01b.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -20, 10f, true, 3f));
        level01Z03spiderEntry01b.Add(new SelectPlayerAIAction());
        level01Z03spiderEntry01b.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO, 0, 0, true, 6f));

        //-----------
        level01Z03spiderEntry02a.Add(new MoveAIAction("Z03WP03", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO, 0, 0f, true, 0f, AIAction.LIST_FINISHED));

        level01Z03spiderAttack02a.Add(new SelectPlayerAIAction());
        level01Z03spiderAttack02a.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 180, 5f));
        level01Z03spiderAttack02a.Add(new MoveAIAction(playerTargetTxt, 8f));
        level01Z03spiderAttack02a.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z03spiderAttack02a.Add(new StandingIdleAIAction(spiderIdleDefaultTime));
        level01Z03spiderAttack02a.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f, true, 0));

        //-----------
        level01Z03spiderEntry02b.Add(new MoveAIAction("Z03WP02", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO, 0, 0f, true, 0f, AIAction.LIST_FINISHED));

        level01Z03spiderAttack02b.Add(new SelectPlayerAIAction());
        level01Z03spiderAttack02b.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 5f));
        level01Z03spiderAttack02b.Add(new MoveAIAction(playerTargetTxt, 8f));
        level01Z03spiderAttack02b.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z03spiderAttack02b.Add(new StandingIdleAIAction(spiderIdleDefaultTime));
        level01Z03spiderAttack02b.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 10f, true, 0));

        //-----------
        level01Z03spiderEntry02c.Add(new MoveAIAction("Z03WP01", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO, 0, 0f, true, 0f, AIAction.LIST_FINISHED));

        level01Z03spiderAttack02c.Add(new SelectPlayerAIAction());
        level01Z03spiderAttack02c.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 5f));
        level01Z03spiderAttack02c.Add(new MoveAIAction(playerTargetTxt, 8f));
        level01Z03spiderAttack02c.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z03spiderAttack02c.Add(new StandingIdleAIAction(spiderIdleDefaultTime));
        level01Z03spiderAttack02c.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 10f, true, 0));

        //-----------
        level01Z03spiderEntry03a.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 20, 8, true, 2f, AIAction.LIST_FINISHED));

        level01Z03spiderAttack03a.Add(new SelectPlayerAIAction());
        level01Z03spiderAttack03a.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 0, 5f));
        level01Z03spiderAttack03a.Add(new MoveAIAction(playerTargetTxt, 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -90, 6f, false));
        level01Z03spiderAttack03a.Add(new MoveAIAction(playerTargetTxt, 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, -90, 6f, false));
        level01Z03spiderAttack03a.Add(new MoveAIAction(playerTargetTxt, 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderAttack03a.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z03spiderAttack03a.Add(new StandingIdleAIAction(spiderIdleDefaultTime));

        //-----------
        level01Z03spiderEntry03b.Add(new MoveAIAction("Z03WP05", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 20, 8, true, 2f, AIAction.LIST_FINISHED));

        level01Z03spiderAttack03b.Add(new SelectPlayerAIAction());
        level01Z03spiderAttack03b.Add(new MoveAIAction(playerTargetTxt, 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 0, 5f));
        level01Z03spiderAttack03b.Add(new MoveAIAction(playerTargetTxt, 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 90, 6f, false));
        level01Z03spiderAttack03b.Add(new MoveAIAction(playerTargetTxt, 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_AGENT_RELATIVE, 90, 6f, false));
        level01Z03spiderAttack03b.Add(new MoveAIAction(playerTargetTxt, 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderAttack03b.Add(new SpiderBiteAIAction(spiderBiteDefaultWaitTime));
        level01Z03spiderAttack03b.Add(new StandingIdleAIAction(spiderIdleDefaultTime));



        //spiderCloseList01.Add(new MoveAIAction("WP02", 10f, true, 0, AIAction.LIST_FINISHED));
    }

    void CreateZonePlans() 
    {
        ////TODO: JORDI TEST REMOVE WHEN DONE
        plan0101 = new ZonePlan();

        plan0101.enemiesThreshold = 1;
        List<WaveAction> z01wave01 = new List<WaveAction>();
        z01wave01.Add(new SpawnSpiderGroupWaveAction("Z01SP01", jordiTestLeader01, defaultSpiderAttack, SpawnSpiderGroupWaveAction.FormationType.TEST, new int[] { ChromaColorInfo.CURRENT_COLOR_OFFSET, ChromaColorInfo.CURRENT_COLOR_OFFSET, ChromaColorInfo.CURRENT_COLOR_OFFSET }));
        //z01wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", jordiTestEntry01, jordiTestAttack01, 0f, 3, 1f, SpiderAIBehaviour.SpawnAnimation.FLOOR));
        //z01wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", jordiTestEntry01, jordiTestAttack01, 2.5f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.FLOOR));
        //z01wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", jordiTestEntry01, jordiTestAttack01, 0.95f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.FLOOR));
        plan0101.sequentialWaves.Add(z01wave01);

        plan0102 = new ZonePlan();
        plan0102.enemiesThreshold = 3;

        List<WaveAction> z02wave01 = new List<WaveAction>();
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", jordiTestEntry01, jordiTestAttack01, 0f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.SKY));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", jordiTestEntry01, jordiTestAttack01, 4f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.FLOOR));
        plan0102.sequentialWaves.Add(z02wave01);

        plan0103 = new ZonePlan();
        plan0103.enemiesThreshold = 3;

        List<WaveAction> z03wave01 = new List<WaveAction>();
        //z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP01", jordiTestEntry01, jordiTestAttack01, 0f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.SKY));
        //z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP01", jordiTestEntry01, jordiTestAttack01, 4f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.FLOOR));
        plan0103.sequentialWaves.Add(z02wave01);

        ////END TODO

        //Init zone plans
        //plan0101
        /*plan0101 = new ZonePlan();

        plan0101.enemiesThreshold = 1;
        List<WaveAction> z01wave01 = new List<WaveAction>();
        z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01SpiderEntry01, defaultSpiderAttack, 0f, 6, 1f));
        plan0101.sequentialWaves.Add(z01wave01);

        List<WaveAction> z01wave02 = new List<WaveAction>();
        z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01SpiderEntry02, defaultSpiderAttack, 0f, 6, 1f));
        plan0101.sequentialWaves.Add(z01wave02);
        List<WaveAction> z01wave03 = new List<WaveAction>();
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP03", level01Z01spiderEntry03, level01Z01spiderAttack03, 0f, 3, 1f));
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderEntry03, level01Z01spiderAttack03, 1f, 3, 1f));
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP02", level01Z01spiderEntry03, level01Z01spiderAttack03, 1f, 3, 1f));
        plan0101.sequentialWaves.Add(z01wave03);

        List<WaveAction> z01wave04 = new List<WaveAction>();
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP03", level01Z01spiderEntry04, defaultYellowSpiderAttack, 0f, 3, 1f));
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP03", level01Z01spiderEntry04, defaultGreenSpiderAttack, 3.5f, 2, 1f));
        plan0101.sequentialWaves.Add(z01wave04);

        List<WaveAction> z01wave05 = new List<WaveAction>();
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderEntry05, level01Z01spiderAttack05, 0f, 3, 1f));
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderEntry05, level01Z01spiderAttack05, 3.5f, 2, 1f));
        plan0101.sequentialWaves.Add(z01wave05);

        List<WaveAction> z01wave06 = new List<WaveAction>();
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, level01Z01spiderAttack06));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, level01Z01spiderAttack06));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, level01Z01spiderAttack06));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, level01Z01spiderAttack06));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, level01Z01spiderAttack06, 1f));
        plan0101.sequentialWaves.Add(z01wave06);

        List<WaveAction> z01wave07 = new List<WaveAction>();
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderEntry07, defaultBlueSpiderAttack, 0f, 3, 0.7f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderEntry07, defaultGreenSpiderAttack, 0.7f, 3, 0.7f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderEntry07, defaultRedSpiderAttack, 0.7f, 3, 0.7f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP01", level01Z01spiderEntry07, defaultYellowSpiderAttack, 0.7f, 3, 0.7f));
        plan0101.sequentialWaves.Add(z01wave07);


        ////plan0102---------------------------------------------------------------------------------------------------------------
        plan0102 = new ZonePlan();
        plan0102.enemiesThreshold = 3;
        
        //List<WaveAction> z02wave01 = new List<WaveAction>();

        //WAVE TO DELETE
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 1.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 1.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 1.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderEntry01a, defaultSpiderAttack, 0.5f));
        //plan0102.sequentialWaves.Add(z02wave01);
        
        /*List<WaveAction> z02wave02 = new List<WaveAction>();
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP02", level01Z02spiderEntry02a, level01Z02spiderAttack02a, 0f, 3, 1f));
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP02", level01Z02spiderEntry02b, level01Z02spiderAttack02b, 3.5f));
        plan0102.sequentialWaves.Add(z02wave02);

        List<WaveAction> z02wave03 = new List<WaveAction>();
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP03", level01Z02spiderEntry03a, level01Z02spiderAttack03a, 0f, 3, 1f));
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP03", level01Z02spiderEntry03b, level01Z02spiderAttack03b, 3.5f));
        plan0102.sequentialWaves.Add(z02wave03);

        List<WaveAction> z02wave04 = new List<WaveAction>();
        z02wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP04", level01Z02spiderEntry04a, level01Z02spiderAttack04a, 0f, 2, 1f));
        z02wave04.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP04", level01Z02spiderEntry04a, level01Z02spiderAttack04a, 2.5f, 2, 1f));
        plan0102.sequentialWaves.Add(z02wave04);

        List<WaveAction> z02wave05 = new List<WaveAction>();
        z02wave05.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP02", level01Z02spiderEntry04b, level01Z02spiderAttack04b, 0f, 2, 1f));
        z02wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP02", level01Z02spiderEntry04b, level01Z02spiderAttack04b, 2.5f, 2, 1f));
        plan0102.sequentialWaves.Add(z02wave05);

        List<WaveAction> z02wave06 = new List<WaveAction>();
        z02wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP04", level01Z02spiderEntry04a, level01Z02spiderAttack04a));
        z02wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP04", level01Z02spiderEntry04a, level01Z02spiderAttack04a, 0.5f, 3, 1f));
        plan0102.sequentialWaves.Add(z02wave06);

        List<WaveAction> z02wave07 = new List<WaveAction>();
        z02wave07.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP03", level01Z02spiderEntry04b, level01Z02spiderAttack04b));
        z02wave07.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP03", level01Z02spiderEntry04b, level01Z02spiderAttack04b, 0.5f, 3, 1f));
        plan0102.sequentialWaves.Add(z02wave07);

        List<WaveAction> z02wave08 = new List<WaveAction>();
        z02wave08.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP01", level01Z02spiderEntry04b, level01Z02spiderAttack04b));
        z02wave08.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP01", level01Z02spiderEntry04b, level01Z02spiderAttack04b, 0.5f, 3, 1f));
        plan0102.sequentialWaves.Add(z02wave08);

        //plan0103---------------------------------------------------------------------------------------------------------------

        plan0103 = new ZonePlan();
        plan0103.enemiesThreshold = 2;

        List<WaveAction> z03wave01 = new List<WaveAction>();
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP01", level01Z03spiderEntry01a, defaultSpiderAttack, 0f, 3, 0.7f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP01", level01Z03spiderEntry01a, defaultSpiderAttack, 2.4f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP01", level01Z03spiderEntry01a, defaultSpiderAttack, 0.7f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP01", level01Z03spiderEntry01a, defaultSpiderAttack, 0.3f));

        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP02", level01Z03spiderEntry01b, defaultSpiderAttack, 2f, 3, 0.7f));
        z03wave01.Add(new SpawnSpiderWaveAction(true, +0, "Z03SP02", level01Z03spiderEntry01b, defaultSpiderAttack, 1.7f, 2, 0.7f));

        plan0103.sequentialWaves.Add(z03wave01);

        List<WaveAction> z03wave02 = new List<WaveAction>();
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP03", level01Z03spiderEntry02a, level01Z03spiderAttack02a, 0f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderEntry02b, level01Z03spiderAttack02b, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderEntry02c, level01Z03spiderAttack02c, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP03", level01Z03spiderEntry02a, level01Z03spiderAttack02a, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderEntry02b, level01Z03spiderAttack02b, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderEntry02c, level01Z03spiderAttack02c, 1f, 2, 0.7f));
        plan0103.sequentialWaves.Add(z03wave02);

        List<WaveAction> z03wave03 = new List<WaveAction>();
        z03wave03.Add(new SpawnSpiderWaveAction(true, +1, "Z03SP03", level01Z03spiderEntry03a, level01Z03spiderAttack03a, 0f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave03);

        List<WaveAction> z03wave04 = new List<WaveAction>();
        z03wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z03SP01", level01Z03spiderEntry03b, level01Z03spiderAttack03b, 1f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave04);

        List<WaveAction> z03wave05 = new List<WaveAction>();
        z03wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z03SP03", level01Z03spiderEntry03a, level01Z03spiderAttack03a, 1f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave05);

        List<WaveAction> z03wave06 = new List<WaveAction>();
        z03wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z03SP01", level01Z03spiderEntry03b, level01Z03spiderAttack03b, 1f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave06);*/
    }

}
