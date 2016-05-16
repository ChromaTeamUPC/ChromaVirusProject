using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public GlobalAIBlackboard blackboard;

    //TODO: Jordi test, delete when finished
    private List<AIAction> jordiTestEntry01 = new List<AIAction>();
    private List<AIAction> jordiTestAttack01 = new List<AIAction>();

    //Action list for the vortex spawned spiders
    public List<AIAction> defaultSpiderEntry = new List<AIAction>();
    public List<AIAction> defaulSpiderAttack = new List<AIAction>();

    //Actions lists
    private List<AIAction> level01Z01SpiderEntry01 = new List<AIAction>();
    private List<AIAction> level01Z01SpiderEntry02 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry03 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry04 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry05 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry06 = new List<AIAction>();
    private List<AIAction> level01Z01spiderEntry07 = new List<AIAction>();

    private List<AIAction> level01Z02spiderEntry01a = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry01b = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry02a = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry02b = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry03a = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry03b = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry04a = new List<AIAction>();
    private List<AIAction> level01Z02spiderEntry04b = new List<AIAction>();

    private List<AIAction> level01Z03spiderEntry01a = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry01b = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry02a = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry02b = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry02c = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry03a = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry03b = new List<AIAction>();
    private List<AIAction> level01Z03spiderEntry04 = new List<AIAction>();

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
        Debug.Log("Enemies++ = " + blackboard.activeEnemies);
    }

    private void EnemyDied(EventInfo eventInfo)
    {
        --blackboard.activeEnemies;
        Debug.Log("Enemies-- = " + blackboard.activeEnemies);
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
            //TODO: Cano, asociate every possible plan with its Id (xxyy xx= level number, yy= zone number)
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
        jordiTestAttack01.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        jordiTestAttack01.Add(new SpiderBiteAIAction(2f));
        jordiTestAttack01.Add(new StandingIdleAIAction(1f));
        //END TODO


        //vortexSpiderEntry has no actions, spider spawns and attacs player
        defaulSpiderAttack.Add(new SelectPlayerAIAction());
        defaulSpiderAttack.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        defaulSpiderAttack.Add(new SpiderBiteAIAction(2f));
        defaulSpiderAttack.Add(new StandingIdleAIAction(1f));


        // LEVEL 01 ZONE 01-------------------------------------------------------------------------------------------------------------------------------

        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP01", 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP02", 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP03", 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("Z01WP06", 8f));
        level01Z01SpiderEntry01.Add(new SelectPlayerAIAction());
        level01Z01SpiderEntry01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 5f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 5f));
        level01Z01SpiderEntry01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f, true, 0f, 4));

        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP04", 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP05", 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP03", 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("Z01WP06", 8f));
        level01Z01SpiderEntry02.Add(new SelectPlayerAIAction());
        level01Z01SpiderEntry02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 5f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 5f));
        level01Z01SpiderEntry02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f, true, 0f, 4));

        level01Z01spiderEntry03.Add(new SelectPlayerAIAction());
        level01Z01spiderEntry03.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 3f));
        level01Z01spiderEntry03.Add(new MoveAIAction("player", 20.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 6f, false, 1f));
        level01Z01spiderEntry03.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderEntry03.Add(new SpiderBiteAIAction(2f, 1));
        level01Z01spiderEntry03.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 10f, false, 6f, 1));

        level01Z01spiderEntry04.Add(new MoveAIAction("Z01WP07", 10f, true));
        level01Z01spiderEntry04.Add(new SelectPlayerAIAction());
        level01Z01spiderEntry04.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderEntry04.Add(new SpiderBiteAIAction(2f, 1));

        level01Z01spiderEntry05.Add(new MoveAIAction("Z01WP08", 10f, true));
        level01Z01spiderEntry05.Add(new SelectPlayerAIAction());
        level01Z01spiderEntry05.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderEntry05.Add(new SpiderBiteAIAction(2f, 1));

        level01Z01spiderEntry06.Add(new SelectPlayerAIAction());
        level01Z01spiderEntry06.Add(new MoveAIAction("player", 6.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderEntry06.Add(new SpiderBiteAIAction(2f));

        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 10f, true, 5f));
        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 8f, true, 4f));
        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 6f, true, 4f));
        level01Z01spiderEntry07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 4f, true, 4f));
        level01Z01spiderEntry07.Add(new SelectPlayerAIAction());
        level01Z01spiderEntry07.Add(new MoveAIAction("player", 6.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderEntry07.Add(new SpiderBiteAIAction(2f));


        // LEVEL 01 ZONE 02-------------------------------------------------------------------------------------------------------------------------------

        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP01", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 45, 5f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP01", 12f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP01", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 5f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP02", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -135, 5f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP02", 12f));
        level01Z02spiderEntry01a.Add(new MoveAIAction("Z02WP02", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -45, 5f));

        level01Z02spiderEntry02a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -20, 10f));
        level01Z02spiderEntry02a.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderEntry02a.Add(new SelectPlayerAIAction());
        level01Z02spiderEntry02a.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 4f));
        level01Z02spiderEntry02a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderEntry02a.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderEntry02b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -20, 10f));
        level01Z02spiderEntry02b.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderEntry02b.Add(new SelectPlayerAIAction());
        level01Z02spiderEntry02b.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 180, 4f));
        level01Z02spiderEntry02b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderEntry02b.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderEntry03a.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderEntry03a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 7f));
        level01Z02spiderEntry03a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f));
        level01Z02spiderEntry03a.Add(new SelectPlayerAIAction());
        level01Z02spiderEntry03a.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 4f));
        level01Z02spiderEntry03a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderEntry03a.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderEntry03b.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderEntry03b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 7f));
        level01Z02spiderEntry03b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f));
        level01Z02spiderEntry03b.Add(new SelectPlayerAIAction());
        level01Z02spiderEntry03b.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 4f));
        level01Z02spiderEntry03b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderEntry03b.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderEntry04a.Add(new MoveAIAction("Z02WP02", 13f, true, 2f));
        level01Z02spiderEntry04a.Add(new SelectPlayerAIAction());
        level01Z02spiderEntry04a.Add(new MoveAIAction("player", 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderEntry04a.Add(new SpiderBiteAIAction(2f));

        level01Z02spiderEntry04b.Add(new MoveAIAction("Z02WP01", 13f, true, 2f));
        level01Z02spiderEntry04b.Add(new SelectPlayerAIAction());
        level01Z02spiderEntry04b.Add(new MoveAIAction("player", 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderEntry04b.Add(new SpiderBiteAIAction(2f));

        // LEVEL 01 ZONE 03-------------------------------------------------------------------------------------------------------------------------------

        level01Z03spiderEntry01a.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 8));

        level01Z03spiderEntry01b.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 10f, true, 3f));
        level01Z03spiderEntry01b.Add(new SelectPlayerAIAction());
        level01Z03spiderEntry01b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO, 0, 0, true, 6f));

        level01Z03spiderEntry02a.Add(new MoveAIAction("Z03WP03", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderEntry02a.Add(new SelectPlayerAIAction());
        level01Z03spiderEntry02a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 180, 5f));
        level01Z03spiderEntry02a.Add(new MoveAIAction("player", 8f));
        level01Z03spiderEntry02a.Add(new SpiderBiteAIAction(2f));
        level01Z03spiderEntry02a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f, true, 0, 1));

        level01Z03spiderEntry02b.Add(new MoveAIAction("Z03WP02", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderEntry02b.Add(new SelectPlayerAIAction());
        level01Z03spiderEntry02b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 5f));
        level01Z03spiderEntry02b.Add(new MoveAIAction("player", 8f));
        level01Z03spiderEntry02b.Add(new SpiderBiteAIAction(2f));
        level01Z03spiderEntry02b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 10f, true, 0, 1));

        level01Z03spiderEntry02c.Add(new MoveAIAction("Z03WP01", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderEntry02c.Add(new SelectPlayerAIAction());
        level01Z03spiderEntry02c.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 5f));
        level01Z03spiderEntry02c.Add(new MoveAIAction("player", 8f));
        level01Z03spiderEntry02c.Add(new SpiderBiteAIAction(2f));
        level01Z03spiderEntry02c.Add(new MoveAIAction("player", 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 10f, true, 0, 1));

        level01Z03spiderEntry03a.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 8, true, 2f));
        level01Z03spiderEntry03a.Add(new SelectPlayerAIAction());
        level01Z03spiderEntry03a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 5f));
        level01Z03spiderEntry03a.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -90, 6f, false));
        level01Z03spiderEntry03a.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -90, 6f, false));
        level01Z03spiderEntry03a.Add(new MoveAIAction("player", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderEntry03a.Add(new SpiderBiteAIAction(2f, 1));

        level01Z03spiderEntry03b.Add(new MoveAIAction("Z03WP05", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 8, true, 2f));
        level01Z03spiderEntry03b.Add(new SelectPlayerAIAction());
        level01Z03spiderEntry03b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 5f));
        level01Z03spiderEntry03b.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 90, 6f, false));
        level01Z03spiderEntry03b.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 90, 6f, false));
        level01Z03spiderEntry03b.Add(new MoveAIAction("player", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderEntry03b.Add(new SpiderBiteAIAction(2f, 1));



        //spiderCloseList01.Add(new MoveAIAction("WP02", 10f, true, 0, AIAction.LIST_FINISHED));
    }

    void CreateZonePlans() 
    {
        //TODO: JORDI TEST REMOVE WHEN DONE
        /*plan0101 = new ZonePlan();

        plan0101.enemiesThreshold = 1;
        List<WaveAction> z01wave01 = new List<WaveAction>();
        z01wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", jordiTestEntry01, jordiTestAttack01, 0f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.SKY));
        z01wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", jordiTestEntry01, jordiTestAttack01, 4f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.FLOOR));
        plan0101.sequentialWaves.Add(z01wave01);

        plan0102 = new ZonePlan();
        plan0102.enemiesThreshold = 3;

        List<WaveAction> z02wave01 = new List<WaveAction>();
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", jordiTestEntry01, jordiTestAttack01, 0f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.SKY));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", jordiTestEntry01, jordiTestAttack01, 4f, 1, 1f, SpiderAIBehaviour.SpawnAnimation.FLOOR));
        plan0102.sequentialWaves.Add(z02wave01);*/
        //END TODO

        //Init zone plans
        //plan0101
        plan0101 = new ZonePlan();

        plan0101.enemiesThreshold = 1;

        List<WaveAction> z01wave01 = new List<WaveAction>();
        z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01SpiderEntry01, defaulSpiderAttack, 0f, 6, 1f));
        plan0101.sequentialWaves.Add(z01wave01);

        List<WaveAction> z01wave02 = new List<WaveAction>();
        z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01SpiderEntry02, defaulSpiderAttack, 0f, 6, 1f));
        plan0101.sequentialWaves.Add(z01wave02);

        List<WaveAction> z01wave03 = new List<WaveAction>();
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP03", level01Z01spiderEntry03, defaulSpiderAttack, 0f, 3, 1f));
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderEntry03, defaulSpiderAttack, 3.5f, 3, 1f));
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP02", level01Z01spiderEntry03, defaulSpiderAttack, 3.5f, 3, 1f));
        plan0101.sequentialWaves.Add(z01wave03);

        List<WaveAction> z01wave04 = new List<WaveAction>();
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP03", level01Z01spiderEntry04, defaulSpiderAttack, 0f, 3, 1f));
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP03", level01Z01spiderEntry04, defaulSpiderAttack, 3.5f, 2, 1f));
        plan0101.sequentialWaves.Add(z01wave04);

        List<WaveAction> z01wave05 = new List<WaveAction>();
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderEntry05, defaulSpiderAttack, 0f, 3, 1f));
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderEntry05, defaulSpiderAttack, 3.5f, 2, 1f));
        plan0101.sequentialWaves.Add(z01wave05);

        List<WaveAction> z01wave06 = new List<WaveAction>();
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, defaulSpiderAttack));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, defaulSpiderAttack));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, defaulSpiderAttack));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderEntry06, defaulSpiderAttack));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderEntry06, defaulSpiderAttack, 1f));
        plan0101.sequentialWaves.Add(z01wave06);

        List<WaveAction> z01wave07 = new List<WaveAction>();
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderEntry07, defaulSpiderAttack, 0f, 3, 0.7f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderEntry07, defaulSpiderAttack, 2.6f, 3, 0.7f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderEntry07, defaulSpiderAttack, 2.6f, 3, 0.7f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP01", level01Z01spiderEntry07, defaulSpiderAttack, 2.6f, 3, 0.7f));
        plan0101.sequentialWaves.Add(z01wave07);

        ////plan0102---------------------------------------------------------------------------------------------------------------
        plan0102 = new ZonePlan();
        plan0102.enemiesThreshold = 3;

        List<WaveAction> z02wave01 = new List<WaveAction>();

        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 1.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 1.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 1.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderEntry01a, defaulSpiderAttack, 0.5f));
        plan0102.sequentialWaves.Add(z02wave01);

        List<WaveAction> z02wave02 = new List<WaveAction>();
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP02", level01Z02spiderEntry02a, defaulSpiderAttack, 0f, 3, 1f));
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP02", level01Z02spiderEntry02b, defaulSpiderAttack, 3.5f));
        plan0102.sequentialWaves.Add(z02wave02);

        List<WaveAction> z02wave03 = new List<WaveAction>();
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP03", level01Z02spiderEntry03a, defaulSpiderAttack, 0f, 3, 1f));
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP03", level01Z02spiderEntry03b, defaulSpiderAttack, 3.5f));
        plan0102.sequentialWaves.Add(z02wave03);

        List<WaveAction> z02wave04 = new List<WaveAction>();
        z02wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP04", level01Z02spiderEntry04a, defaulSpiderAttack, 0f, 2, 1f));
        z02wave04.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP04", level01Z02spiderEntry04a, defaulSpiderAttack, 2.5f, 2, 1f));
        plan0102.sequentialWaves.Add(z02wave04);

        List<WaveAction> z02wave05 = new List<WaveAction>();
        z02wave05.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP02", level01Z02spiderEntry04b, defaulSpiderAttack, 0f, 2, 1f));
        z02wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP02", level01Z02spiderEntry04b, defaulSpiderAttack, 2.5f, 2, 1f));
        plan0102.sequentialWaves.Add(z02wave05);

        List<WaveAction> z02wave06 = new List<WaveAction>();
        z02wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP04", level01Z02spiderEntry04a, defaulSpiderAttack));
        z02wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP04", level01Z02spiderEntry04a, defaulSpiderAttack, 0.5f, 3, 1f));
        plan0102.sequentialWaves.Add(z02wave06);

        List<WaveAction> z02wave07 = new List<WaveAction>();
        z02wave07.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP03", level01Z02spiderEntry04b, defaulSpiderAttack));
        z02wave07.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP03", level01Z02spiderEntry04b, defaulSpiderAttack, 0.5f, 3, 1f));
        plan0102.sequentialWaves.Add(z02wave07);

        List<WaveAction> z02wave08 = new List<WaveAction>();
        z02wave08.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP01", level01Z02spiderEntry04b, defaulSpiderAttack));
        z02wave08.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP01", level01Z02spiderEntry04b, defaulSpiderAttack, 0.5f, 3, 1f));
        plan0102.sequentialWaves.Add(z02wave08);

        //plan0103---------------------------------------------------------------------------------------------------------------

        plan0103 = new ZonePlan();
        plan0103.enemiesThreshold = 2;

        List<WaveAction> z03wave01 = new List<WaveAction>();
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP01", level01Z03spiderEntry01a, defaulSpiderAttack, 0f, 3, 0.7f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP01", level01Z03spiderEntry01a, defaulSpiderAttack, 2.4f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP01", level01Z03spiderEntry01a, defaulSpiderAttack, 0.7f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP01", level01Z03spiderEntry01a, defaulSpiderAttack, 0.3f));

        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP02", level01Z03spiderEntry01b, defaulSpiderAttack, 2f, 3, 0.7f));
        z03wave01.Add(new SpawnSpiderWaveAction(true, +0, "Z03SP02", level01Z03spiderEntry01b, defaulSpiderAttack, 1.7f, 2, 0.7f ));

        plan0103.sequentialWaves.Add(z03wave01);

        List<WaveAction> z03wave02 = new List<WaveAction>();
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP03", level01Z03spiderEntry02a, defaulSpiderAttack, 0f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderEntry02b, defaulSpiderAttack, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderEntry02c, defaulSpiderAttack, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP03", level01Z03spiderEntry02a, defaulSpiderAttack, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderEntry02b, defaulSpiderAttack, 1f, 2, 0.7f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderEntry02c, defaulSpiderAttack, 1f, 2, 0.7f));
        plan0103.sequentialWaves.Add(z03wave02);

        List<WaveAction> z03wave03 = new List<WaveAction>();
        z03wave03.Add(new SpawnSpiderWaveAction(true, +1, "Z03SP03", level01Z03spiderEntry03a, defaulSpiderAttack, 0f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave03);

        List<WaveAction> z03wave04 = new List<WaveAction>();
        z03wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z03SP01", level01Z03spiderEntry03b, defaulSpiderAttack, 1f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave04);

        List<WaveAction> z03wave05 = new List<WaveAction>();
        z03wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z03SP03", level01Z03spiderEntry03a, defaulSpiderAttack, 1f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave05);

        List<WaveAction> z03wave06 = new List<WaveAction>();
        z03wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z03SP01", level01Z03spiderEntry03b, defaulSpiderAttack, 1f, 3, 0.7f));
        plan0103.sequentialWaves.Add(z03wave06);
    }

}
