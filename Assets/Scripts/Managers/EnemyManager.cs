using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour {

    //Actions lists
    private List<AIAction> level01Z01spiderDefault01 = new List<AIAction>();
    private List<AIAction> level01Z01spiderDefault02 = new List<AIAction>();
    private List<AIAction> level01Z01spiderDefault03 = new List<AIAction>();
    private List<AIAction> level01Z01spiderDefault04 = new List<AIAction>();
    private List<AIAction> level01Z01spiderDefault05 = new List<AIAction>();
    private List<AIAction> level01Z01spiderDefault06 = new List<AIAction>();
    private List<AIAction> level01Z01spiderDefault07 = new List<AIAction>();

    private List<AIAction> level01Z02spiderDefault01a = new List<AIAction>();
    private List<AIAction> level01Z02spiderDefault01b = new List<AIAction>();
    private List<AIAction> level01Z02spiderDefault02a = new List<AIAction>();
    private List<AIAction> level01Z02spiderDefault02b = new List<AIAction>();
    private List<AIAction> level01Z02spiderDefault03a = new List<AIAction>();
    private List<AIAction> level01Z02spiderDefault03b = new List<AIAction>();
    private List<AIAction> level01Z02spiderDefault04a = new List<AIAction>();
    private List<AIAction> level01Z02spiderDefault04b = new List<AIAction>();

    private List<AIAction> level01Z03spiderDefault01a = new List<AIAction>();
    private List<AIAction> level01Z03spiderDefault01b = new List<AIAction>();
    private List<AIAction> level01Z03spiderDefault02a = new List<AIAction>();
    private List<AIAction> level01Z03spiderDefault02b = new List<AIAction>();
    private List<AIAction> level01Z03spiderDefault02c = new List<AIAction>();
    private List<AIAction> level01Z03spiderDefault03a = new List<AIAction>();
    private List<AIAction> level01Z03spiderDefault03b = new List<AIAction>();
    private List<AIAction> level01Z03spiderDefault04 = new List<AIAction>();


    private List<AIAction> spiderCloseList01 = new List<AIAction>();
    //...

    //Example Zone Plans
    private ZonePlan plan0101;
    private ZonePlan plan0102;
    private ZonePlan plan0103;

    private ZonePlan currentPlan;
    private int currentPlanId;
    private int enemies;
    private int spawners;
    private int turrets;
    private int executingWaves;
    private int sequentialWavesIndex;

    void Awake()
    {
        Debug.Log("Enemy Manager created");
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

        rsc.eventMng.StartListening(EventManager.EventType.SPAWNER_ACTIVATED, SpawnerActivated);
        rsc.eventMng.StartListening(EventManager.EventType.SPAWNER_DESTROYED, SpawnerDestroyed);

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

            rsc.eventMng.StopListening(EventManager.EventType.SPAWNER_ACTIVATED, SpawnerActivated);
            rsc.eventMng.StopListening(EventManager.EventType.SPAWNER_DESTROYED, SpawnerDestroyed);

            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameOver);
        }

        Debug.Log("Color Manager destroyed");
    }

    private void EnemySpawned(EventInfo eventInfo)
    {
        ++enemies;
        Debug.Log("Enemies++ = " + enemies);
    }

    private void EnemyDied(EventInfo eventInfo)
    {
        --enemies;
        Debug.Log("Enemies-- = " + enemies);
    }

    private void TurretSpawned(EventInfo eventInfo)
    {
        ++turrets;
    }

    private void TurretDestroyed(EventInfo eventInfo)
    {
        --turrets;
    }

    private void SpawnerActivated(EventInfo eventInfo)
    {
        ++spawners;
    }

    private void SpawnerDestroyed(EventInfo eventInfo)
    {
        --spawners;
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

        enemies = 0;
        spawners = 0;
        turrets = 0;
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

        return (enemies + spawners + turrets + executingWaves == 0) && (sequentialWavesIndex == currentPlan.sequentialWaves.Count);
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
                if(enemies < currentPlan.enemiesThreshold && executingWaves == 0 && sequentialWavesIndex < currentPlan.sequentialWaves.Count)
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
                float toPlayer1 = (origin.transform.position - rsc.gameInfo.player1.transform.position).magnitude;
                float toPlayer2 = (origin.transform.position - rsc.gameInfo.player2.transform.position).magnitude;
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
        // LEVEL 01 ZONE 01-------------------------------------------------------------------------------------------------------------------------------

        level01Z01spiderDefault01.Add(new MoveAIAction("Z01WP01", 10f));
        level01Z01spiderDefault01.Add(new MoveAIAction("Z01WP02", 10f));
        level01Z01spiderDefault01.Add(new MoveAIAction("Z01WP03", 10f));
        level01Z01spiderDefault01.Add(new MoveAIAction("Z01WP06", 8f));
        level01Z01spiderDefault01.Add(new SelectTargetAIAction("player"));
        level01Z01spiderDefault01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 5f));
        level01Z01spiderDefault01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f));
        level01Z01spiderDefault01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 5f));
        level01Z01spiderDefault01.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f, true, 0f, 4));

        level01Z01spiderDefault02.Add(new MoveAIAction("Z01WP04", 10f));
        level01Z01spiderDefault02.Add(new MoveAIAction("Z01WP05", 10f));
        level01Z01spiderDefault02.Add(new MoveAIAction("Z01WP03", 10f));
        level01Z01spiderDefault02.Add(new MoveAIAction("Z01WP06", 8f));
        level01Z01spiderDefault02.Add(new SelectTargetAIAction("player"));
        level01Z01spiderDefault02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 5f));
        level01Z01spiderDefault02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f));
        level01Z01spiderDefault02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 5f));
        level01Z01spiderDefault02.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 10f, true, 0f, 4));

        level01Z01spiderDefault03.Add(new SelectTargetAIAction("player"));
        level01Z01spiderDefault03.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 3f));
        level01Z01spiderDefault03.Add(new MoveAIAction("player", 20.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 6f, false, 1f));
        level01Z01spiderDefault03.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderDefault03.Add(new SpiderBiteAIAction(2f, 1));
        level01Z01spiderDefault03.Add(new MoveAIAction("player", 7.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 10f, false, 6f, 1));

        level01Z01spiderDefault04.Add(new MoveAIAction("Z01WP07", 10f, true));
        level01Z01spiderDefault04.Add(new SelectTargetAIAction("player"));
        level01Z01spiderDefault04.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderDefault04.Add(new SpiderBiteAIAction(2f, 1));

        level01Z01spiderDefault05.Add(new MoveAIAction("Z01WP08", 10f, true));
        level01Z01spiderDefault05.Add(new SelectTargetAIAction("player"));
        level01Z01spiderDefault05.Add(new MoveAIAction("player", 8.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderDefault05.Add(new SpiderBiteAIAction(2f, 1));

        level01Z01spiderDefault06.Add(new SelectTargetAIAction("player"));
        level01Z01spiderDefault06.Add(new MoveAIAction("player", 6.0f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderDefault06.Add(new SpiderBiteAIAction(2f));

        level01Z01spiderDefault07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 10f, true, 5f));
        level01Z01spiderDefault07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 8f, true, 4f));
        level01Z01spiderDefault07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 6f, true, 4f));
        level01Z01spiderDefault07.Add(new MoveAIAction("Z01WP03", 14f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 4f, true, 4f));
        level01Z01spiderDefault07.Add(new SelectTargetAIAction("player"));
        level01Z01spiderDefault07.Add(new MoveAIAction("player", 6.0f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z01spiderDefault07.Add(new SpiderBiteAIAction(2f));


        // LEVEL 01 ZONE 02-------------------------------------------------------------------------------------------------------------------------------

        level01Z02spiderDefault01a.Add(new MoveAIAction("Z02WP01", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 45, 5f));
        level01Z02spiderDefault01a.Add(new MoveAIAction("Z02WP01", 12f));
        level01Z02spiderDefault01a.Add(new MoveAIAction("Z02WP01", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 5f));
        level01Z02spiderDefault01a.Add(new MoveAIAction("Z02WP02", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -135, 5f));
        level01Z02spiderDefault01a.Add(new MoveAIAction("Z02WP02", 12f));
        level01Z02spiderDefault01a.Add(new MoveAIAction("Z02WP02", 12f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -45, 5f));

        level01Z02spiderDefault02a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -20, 10f));
        level01Z02spiderDefault02a.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderDefault02a.Add(new SelectTargetAIAction("player"));
        level01Z02spiderDefault02a.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 4f));
        level01Z02spiderDefault02a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderDefault02a.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderDefault02b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, -20, 10f));
        level01Z02spiderDefault02b.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderDefault02b.Add(new SelectTargetAIAction("player"));
        level01Z02spiderDefault02b.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 180, 4f));
        level01Z02spiderDefault02b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderDefault02b.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderDefault03a.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderDefault03a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 7f));
        level01Z02spiderDefault03a.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f));
        level01Z02spiderDefault03a.Add(new SelectTargetAIAction("player"));
        level01Z02spiderDefault03a.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 4f));
        level01Z02spiderDefault03a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderDefault03a.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderDefault03b.Add(new MoveAIAction("Z02WP01", 15f));
        level01Z02spiderDefault03b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 135, 7f));
        level01Z02spiderDefault03b.Add(new MoveAIAction("Z02WP01", 15f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f));
        level01Z02spiderDefault03b.Add(new SelectTargetAIAction("player"));
        level01Z02spiderDefault03b.Add(new MoveAIAction("player", 6f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 4f));
        level01Z02spiderDefault03b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderDefault03b.Add(new SpiderBiteAIAction(2f, 3));

        level01Z02spiderDefault04a.Add(new MoveAIAction("Z02WP02", 13f, true, 2f));
        level01Z02spiderDefault04a.Add(new SelectTargetAIAction("player"));
        level01Z02spiderDefault04a.Add(new MoveAIAction("player", 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderDefault04a.Add(new SpiderBiteAIAction(2f));

        level01Z02spiderDefault04b.Add(new MoveAIAction("Z02WP01", 13f, true, 2f));
        level01Z02spiderDefault04b.Add(new SelectTargetAIAction("player"));
        level01Z02spiderDefault04b.Add(new MoveAIAction("player", 10f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z02spiderDefault04b.Add(new SpiderBiteAIAction(2f));

        // LEVEL 01 ZONE 03-------------------------------------------------------------------------------------------------------------------------------

        level01Z03spiderDefault01a.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 8));

        level01Z03spiderDefault01b.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -20, 10f, true, 3f));
        level01Z03spiderDefault01b.Add(new SelectTargetAIAction("player"));
        level01Z03spiderDefault01b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO, 0, 0, true, 6f));

        level01Z03spiderDefault02a.Add(new MoveAIAction("Z03WP03", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderDefault02a.Add(new SelectTargetAIAction("player"));
        level01Z03spiderDefault02a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 180, 5f));
        level01Z03spiderDefault02a.Add(new MoveAIAction("player", 8f));
        level01Z03spiderDefault02a.Add(new SpiderBiteAIAction(2f));
        level01Z03spiderDefault02a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 90, 10f, true, 0, 1));

        level01Z03spiderDefault02b.Add(new MoveAIAction("Z03WP02", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderDefault02b.Add(new SelectTargetAIAction("player"));
        level01Z03spiderDefault02b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 5f));
        level01Z03spiderDefault02b.Add(new MoveAIAction("player", 8f));
        level01Z03spiderDefault02b.Add(new SpiderBiteAIAction(2f));
        level01Z03spiderDefault02b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 10f, true, 0, 1));

        level01Z03spiderDefault02c.Add(new MoveAIAction("Z03WP01", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderDefault02c.Add(new SelectTargetAIAction("player"));
        level01Z03spiderDefault02c.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 0, 5f));
        level01Z03spiderDefault02c.Add(new MoveAIAction("player", 8f));
        level01Z03spiderDefault02c.Add(new SpiderBiteAIAction(2f));
        level01Z03spiderDefault02c.Add(new MoveAIAction("player", 8f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_WORLD_RELATIVE, 270, 10f, true, 0, 1));

        level01Z03spiderDefault03a.Add(new MoveAIAction("Z03WP07", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 8, true, 2f));
        level01Z03spiderDefault03a.Add(new SelectTargetAIAction("player"));
        level01Z03spiderDefault03a.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 5f));
        level01Z03spiderDefault03a.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -90, 6f, false));
        level01Z03spiderDefault03a.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, -90, 6f, false));
        level01Z03spiderDefault03a.Add(new MoveAIAction("player", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderDefault03a.Add(new SpiderBiteAIAction(2f, 1));

        level01Z03spiderDefault03b.Add(new MoveAIAction("Z03WP05", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 20, 8, true, 2f));
        level01Z03spiderDefault03b.Add(new SelectTargetAIAction("player"));
        level01Z03spiderDefault03b.Add(new MoveAIAction("player", 8f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 0, 5f));
        level01Z03spiderDefault03b.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 90, 6f, false));
        level01Z03spiderDefault03b.Add(new MoveAIAction("player", 20f, AIAction.FocusType.FIXED, AIAction.OffsetType.AROUND_ENEMY_RELATIVE, 90, 6f, false));
        level01Z03spiderDefault03b.Add(new MoveAIAction("player", 15f, AIAction.FocusType.CONTINUOUS, AIAction.OffsetType.POSITION_ZERO));
        level01Z03spiderDefault03b.Add(new SpiderBiteAIAction(2f, 1));



        //spiderCloseList01.Add(new MoveAIAction("WP02", 10f, true, 0, AIAction.LIST_FINISHED));
    }

    void CreateZonePlans() 
    {
        //Init zone plans
        //plan0101
        plan0101 = new ZonePlan();

        plan0101.enemiesThreshold = 1;

        List<WaveAction> z01wave01 = new List<WaveAction>();
        z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault01, null, null, 0f, 6, 1f));
        /*z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault01, null, null, 0.5f));
        z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault01, null, null, 0.5f));
        z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault01, null, null, 0.5f));
        z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault01, null, null, 0.5f));
        z01wave01.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault01, null, null, 0.5f));*/
        plan0101.sequentialWaves.Add(z01wave01);

        List<WaveAction> z01wave02 = new List<WaveAction>();
        z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault02, null, null, 0f, 6, 1f));
        /*z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault02, null, null, 0.5f));
        z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault02, null, null, 0.5f));
        z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault02, null, null, 0.5f));
        z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault02, null, null, 0.5f));
        z01wave02.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault02, null, null, 0.5f));*/
        plan0101.sequentialWaves.Add(z01wave02);

        List<WaveAction> z01wave03 = new List<WaveAction>();
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP03", level01Z01spiderDefault03, null, null, 0f, 3, 1f));
        /*z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP03", level01Z01spiderDefault03, null, null, 0.5f));
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP03", level01Z01spiderDefault03, null, null, 0.5f));*/

        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderDefault03, null, null, 3f, 3, 1f));
        /*z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderDefault03, null, null, 0.5f));
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderDefault03, null, null, 0.5f));*/

        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP02", level01Z01spiderDefault03, null, null, 3f, 3, 1f));
        /*z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP02", level01Z01spiderDefault03, null, null, 0.5f));
        z01wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP02", level01Z01spiderDefault03, null, null, 0.5f));*/
        plan0101.sequentialWaves.Add(z01wave03);

        /*List<WaveAction> z01wave04 = new List<WaveAction>();
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP03", level01Z01spiderDefault04, null, null));
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP03", level01Z01spiderDefault04, null, null, 0.5f));
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP03", level01Z01spiderDefault04, null, null, 0.5f));
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP03", level01Z01spiderDefault04, null, null, 0.5f));
        z01wave04.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP03", level01Z01spiderDefault04, null, null, 0.5f));
        plan0101.sequentialWaves.Add(z01wave04);

        List<WaveAction> z01wave05 = new List<WaveAction>();
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderDefault05, null, null));
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderDefault05, null, null, 0.5f));
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderDefault05, null, null, 0.5f));
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE,  "Z01SP01", level01Z01spiderDefault05, null, null, 0.5f));
        z01wave05.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderDefault05, null, null, 0.5f));
        plan0101.sequentialWaves.Add(z01wave05);

        List<WaveAction> z01wave06 = new List<WaveAction>();
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderDefault06, null, null));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault06, null, null, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault06, null, null, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderDefault06, null, null));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault06, null, null, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault06, null, null, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderDefault06, null, null));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault06, null, null, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault06, null, null, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP03", level01Z01spiderDefault06, null, null));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP01", level01Z01spiderDefault06, null, null, 1f));
        z01wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z01SP02", level01Z01spiderDefault06, null, null, 1f));
        plan0101.sequentialWaves.Add(z01wave06);

        List<WaveAction> z01wave07 = new List<WaveAction>();
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderDefault07, null, null));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        z01wave07.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z01SP01", level01Z01spiderDefault07, null, null, 0.3f));
        plan0101.sequentialWaves.Add(z01wave07);

        ////plan0102---------------------------------------------------------------------------------------------------------------
        plan0102 = new ZonePlan();
        plan0102.enemiesThreshold = 3;

        List<WaveAction> z02wave01 = new List<WaveAction>();
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderDefault01a, null, null));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderDefault01a, null, null, 1.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderDefault01a, null, null, 1.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderDefault01a, null, null, 1.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        z02wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP01", level01Z02spiderDefault01a, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave01);

        /*List<WaveAction> z02wave02 = new List<WaveAction>();
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP02", level01Z02spiderDefault02a, null, null));
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP02", level01Z02spiderDefault02a, null, null, 0.5f));
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z02SP02", level01Z02spiderDefault02a, null, null, 0.5f));
        z02wave02.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z02SP02", level01Z02spiderDefault02b, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave02);

        List<WaveAction> z02wave03 = new List<WaveAction>();
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP03", level01Z02spiderDefault03a, null, null));
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP03", level01Z02spiderDefault03a, null, null, 0.5f));
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z02SP03", level01Z02spiderDefault03a, null, null, 0.5f));
        z02wave03.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z02SP03", level01Z02spiderDefault03b, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave03);

        List<WaveAction> z02wave04 = new List<WaveAction>();
        z02wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP04", level01Z02spiderDefault04a, null, null));
        z02wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP04", level01Z02spiderDefault04a, null, null, 0.5f));
        z02wave04.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP04", level01Z02spiderDefault04a, null, null, 0.5f));
        z02wave04.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP04", level01Z02spiderDefault04a, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave04);

        List<WaveAction> z02wave05 = new List<WaveAction>();
        z02wave05.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP02", level01Z02spiderDefault04b, null, null));
        z02wave05.Add(new SpawnSpiderWaveAction(true, +2, "Z02SP02", level01Z02spiderDefault04b, null, null, 0.5f));
        z02wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP02", level01Z02spiderDefault04b, null, null, 0.5f));
        z02wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z02SP02", level01Z02spiderDefault04b, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave05);

        List<WaveAction> z02wave06 = new List<WaveAction>();
        z02wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP04", level01Z02spiderDefault04a, null, null));
        z02wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP04", level01Z02spiderDefault04a, null, null, 0.5f));
        z02wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP04", level01Z02spiderDefault04a, null, null, 0.5f));
        z02wave06.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP04", level01Z02spiderDefault04a, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave06);

        List<WaveAction> z02wave07 = new List<WaveAction>();
        z02wave07.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP03", level01Z02spiderDefault04b, null, null));
        z02wave07.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP03", level01Z02spiderDefault04b, null, null, 0.5f));
        z02wave07.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP03", level01Z02spiderDefault04b, null, null, 0.5f));
        z02wave07.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP03", level01Z02spiderDefault04b, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave07);

        List<WaveAction> z02wave08 = new List<WaveAction>();
        z02wave08.Add(new SpawnSpiderWaveAction(true, +4, "Z02SP01", level01Z02spiderDefault04b, null, null));
        z02wave08.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP01", level01Z02spiderDefault04b, null, null, 0.5f));
        z02wave08.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP01", level01Z02spiderDefault04b, null, null, 0.5f));
        z02wave08.Add(new SpawnSpiderWaveAction(true, +1, "Z02SP01", level01Z02spiderDefault04b, null, null, 0.5f));
        plan0102.sequentialWaves.Add(z02wave08);*/

        //plan0103---------------------------------------------------------------------------------------------------------------

        plan0103 = new ZonePlan();
        plan0103.enemiesThreshold = 2;

        List<WaveAction> z03wave01 = new List<WaveAction>();
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP01", level01Z03spiderDefault01a, null, null));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP01", level01Z03spiderDefault01a, null, null, 0.3f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP01", level01Z03spiderDefault01a, null, null, 0.3f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP01", level01Z03spiderDefault01a, null, null, 0.3f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP01", level01Z03spiderDefault01a, null, null, 0.3f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP01", level01Z03spiderDefault01a, null, null, 0.3f));

        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP02", level01Z03spiderDefault01b, null, null, 2f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP02", level01Z03spiderDefault01b, null, null, 0.3f));
        z03wave01.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP02", level01Z03spiderDefault01b, null, null, 0.3f));
        z03wave01.Add(new SpawnSpiderWaveAction(true, +0, "Z03SP02", level01Z03spiderDefault01b, null, null, 0.3f));
        z03wave01.Add(new SpawnSpiderWaveAction(true, +0, "Z03SP02", level01Z03spiderDefault01b, null, null, 0.3f));

        plan0103.sequentialWaves.Add(z03wave01);

        /*List<WaveAction> z03wave02 = new List<WaveAction>();
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP03", level01Z03spiderDefault02a, null, null));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.RED, "Z03SP03", level01Z03spiderDefault02a, null, null, 0.2f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderDefault02b, null, null, 1f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderDefault02b, null, null, 0.2f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderDefault02c, null, null, 1f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderDefault02c, null, null, 0.2f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP03", level01Z03spiderDefault02a, null, null, 1f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.YELLOW, "Z03SP03", level01Z03spiderDefault02a, null, null, 0.2f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderDefault02b, null, null, 1f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.GREEN, "Z03SP03", level01Z03spiderDefault02b, null, null, 0.2f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderDefault02c, null, null, 1f));
        z03wave02.Add(new SpawnSpiderWaveAction(ChromaColor.BLUE, "Z03SP03", level01Z03spiderDefault02c, null, null, 0.2f));
        plan0103.sequentialWaves.Add(z03wave02);

        List<WaveAction> z03wave03 = new List<WaveAction>();
        z03wave03.Add(new SpawnSpiderWaveAction(true, +1, "Z03SP03", level01Z03spiderDefault03a, null, null));
        z03wave03.Add(new SpawnSpiderWaveAction(true, +1, "Z03SP03", level01Z03spiderDefault03a, null, null, 0.3f));
        z03wave03.Add(new SpawnSpiderWaveAction(true, +1, "Z03SP03", level01Z03spiderDefault03a, null, null, 0.3f));
        plan0103.sequentialWaves.Add(z03wave03);

        List<WaveAction> z03wave04 = new List<WaveAction>();
        z03wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z03SP01", level01Z03spiderDefault03b, null, null, 1f));
        z03wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z03SP01", level01Z03spiderDefault03b, null, null, 0.3f));
        z03wave04.Add(new SpawnSpiderWaveAction(true, +2, "Z03SP01", level01Z03spiderDefault03b, null, null, 0.3f));
        plan0103.sequentialWaves.Add(z03wave04);

        List<WaveAction> z03wave05 = new List<WaveAction>();
        z03wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z03SP03", level01Z03spiderDefault03a, null, null, 1f));
        z03wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z03SP03", level01Z03spiderDefault03a, null, null, 0.3f));
        z03wave05.Add(new SpawnSpiderWaveAction(true, +3, "Z03SP03", level01Z03spiderDefault03a, null, null, 0.3f));
        plan0103.sequentialWaves.Add(z03wave05);

        List<WaveAction> z03wave06 = new List<WaveAction>();
        z03wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z03SP01", level01Z03spiderDefault03b, null, null, 1f));
        z03wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z03SP01", level01Z03spiderDefault03b, null, null, 0.3f));
        z03wave06.Add(new SpawnSpiderWaveAction(true, +4, "Z03SP01", level01Z03spiderDefault03b, null, null, 0.3f));
        plan0103.sequentialWaves.Add(z03wave06);*/
    }

}
