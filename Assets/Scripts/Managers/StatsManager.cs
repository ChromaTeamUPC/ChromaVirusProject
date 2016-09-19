using UnityEngine;
using System.Collections;

public class PlayerStats
{
    private int playerId;
    ChromaColor lastKillColor;

    public int currentIncrement;
    private int specialComboIncrement;

    public float chainMaxTime;
    public float chainRemainingTime;

    public int currentChain;
    public int maxChain;

    public int enemiesKilledOk;
    public int enemiesKilledWrong;

    public int colorAccuracy;

    public PlayerStats(int id, float chainMax, int specialComboInc)
    {
        playerId = id;
        chainMaxTime = chainMax;
        specialComboIncrement = specialComboInc;
        Reset();
    }

    public void Reset()
    {
        enemiesKilledOk = 0;
        enemiesKilledWrong = 0;

        currentIncrement = 0;
        chainRemainingTime = 0f;
        currentChain = 0;
        maxChain = 0;
    }

    public void UpdateChainTime()
    {
        if (chainRemainingTime > 0)
        {
            chainRemainingTime -= Time.deltaTime;

            if(chainRemainingTime <= 0)
            {
                chainRemainingTime = 0;
                currentIncrement = 0;
                currentChain = 0;
                ComboEventInfo.eventInfo.playerId = playerId;
                rsc.eventMng.TriggerEvent(EventManager.EventType.COMBO_BREAK, ComboEventInfo.eventInfo);
            }
        }
    }

    public void EnemyKilledOk(ChromaColor color, bool specialKill)
    {
        ++enemiesKilledOk;

        int total;

        if(specialKill)
        {
            total = specialComboIncrement;

            //To force start a chain
            if (currentIncrement == 0)
            {
                ++currentIncrement;
                lastKillColor = color;
            }
        }
        else
        {
            //If it was a chain ongoing
            if (currentIncrement > 0)
            {
                if (color == lastKillColor)
                {
                    ++currentIncrement;
                }
                else
                {
                    currentIncrement = 1;
                    lastKillColor = color;
                }
            }
            //Start chain
            else
            {
                ++currentIncrement;
                lastKillColor = color;
            }

            total = currentIncrement;
        }

        currentChain += total;
        if (currentChain > maxChain)
            maxChain = currentChain;

        chainRemainingTime = chainMaxTime;

        ComboEventInfo.eventInfo.playerId = playerId;
        ComboEventInfo.eventInfo.comboColor = lastKillColor;
        ComboEventInfo.eventInfo.comboAdd = total;
        rsc.eventMng.TriggerEvent(EventManager.EventType.COMBO_ADD, ComboEventInfo.eventInfo);
    }

    public void EnemyKilledWrong()
    {
        ++enemiesKilledWrong;
        BreakChain();
    }

    public void BreakChain()
    {
        currentChain = 0;
        currentIncrement = 0;
        chainRemainingTime = 0;

        ComboEventInfo.eventInfo.playerId = playerId;
        rsc.eventMng.TriggerEvent(EventManager.EventType.COMBO_BREAK, ComboEventInfo.eventInfo);
    }

    public void ComputeAccuracy()
    {
        if (enemiesKilledOk + enemiesKilledWrong > 0)
            colorAccuracy = Mathf.RoundToInt((float)enemiesKilledOk / (float)(enemiesKilledOk + enemiesKilledWrong) * 100);
        else
            colorAccuracy = 0;
    }
}

public class StatsManager : MonoBehaviour 
{
    private float startTime;
    private float totalTime;
    public PlayerStats p1Stats;
    public PlayerStats p2Stats;

    [Header("Chain Settings")]
    public float chainMaxTime = 5f;
    public int specialKillsChainIncrement = 3;

    [Header("Computed Score Settings")]
    public int maxChainMultiplier = 1000;

    public int level01BaseSeconds = 300;
    public int levelBossBaseSeconds = 300;

    public int secondMultiplier = 1000;

    private bool updateChainTime = false;

    void Awake()
    {
        p1Stats = new PlayerStats(1, chainMaxTime, specialKillsChainIncrement);
        p2Stats = new PlayerStats(2, chainMaxTime, specialKillsChainIncrement);
    }

	// Use this for initialization
	void Start ()
	{
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_LOADED, LevelLoaded);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_REACHED, ZoneStarted);
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZoneFinished);
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_DESTROYED, WormHeadDestroyed); 
        rsc.eventMng.StartListening(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDied);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_LOADED, LevelLoaded);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_REACHED, ZoneStarted);
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZoneFinished);
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_DESTROYED, WormHeadDestroyed);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDied);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        }
    }

    void Update()
    {
        if (updateChainTime && rsc.enemyMng.AreEnemies())
        {
            p1Stats.UpdateChainTime();
            p2Stats.UpdateChainTime();
        }
    }

    public int GetCurrentLevelBaseTime()
    {
        switch (rsc.gameMng.CurrentLevel)
        {
            case GameManager.Level.LEVEL_01:
                return level01BaseSeconds;

            case GameManager.Level.LEVEL_BOSS:
                return levelBossBaseSeconds;

            default:
                return level01BaseSeconds;
        }
    }

	public int GetTotalTime()
    {
        return Mathf.RoundToInt(totalTime);
    }

	private void GameReset(EventInfo eventInfo)
    {     
        p1Stats.Reset();
        p2Stats.Reset();

        updateChainTime = false;
        startTime = 0f;
        totalTime = 0f;
    }

    private void LevelLoaded(EventInfo eventInfo)
    {
        p1Stats.Reset();
        p2Stats.Reset();

        updateChainTime = false;
        startTime = 0f;
        totalTime = 0f;
    }

    private void LevelStarted(EventInfo eventInfo)
    {
        updateChainTime = true;
        startTime = Time.time;
    }

    private void LevelCleared(EventInfo eventInfo)
    {
        updateChainTime = false;
        totalTime = Time.time - startTime;

        p1Stats.ComputeAccuracy();
        p2Stats.ComputeAccuracy();
    }

    private void ZoneStarted(EventInfo eventInfo)
    {
        updateChainTime = true;
    }

    private void ZoneFinished(EventInfo eventInfo)
    {
        updateChainTime = false;
    }

    private void EnemyDied(EventInfo eventInfo)
    {
        EnemyDiedEventInfo info = (EnemyDiedEventInfo)eventInfo;

        if (info.killerPlayer != null)
        {
            PlayerStats stats;

            switch (info.killerPlayer.Id)
            {
                case 1:
                    stats = p1Stats;
                    break;

                case 2:
                    stats = p2Stats;
                    break;

                default:
                    stats = p1Stats;
                    break;
            }

            if (info.killedSameColor)
            {
                stats.EnemyKilledOk(info.color, info.specialKill);
            }
            else
            {
                stats.EnemyKilledWrong();
            }
        }
    }

    private void WormHeadDestroyed(EventInfo eventInfo)
    {
        WormEventInfo info = (WormEventInfo)eventInfo;

        if (info.wormBb.killerPlayer != null)
        {
            PlayerStats stats;

            switch (info.wormBb.killerPlayer.Id)
            {
                case 1:
                    stats = p1Stats;
                    break;

                case 2:
                    stats = p2Stats;
                    break;

                default:
                    stats = p1Stats;
                    break;
            }

            stats.EnemyKilledOk(ChromaColorInfo.Random, true);
        }
    }

    private void PlayerDied(EventInfo eventInfo)
    {
        PlayerEventInfo info = (PlayerEventInfo)eventInfo;

        PlayerStats stats;

        switch (info.player.Id)
        {
            case 1:
                stats = p1Stats;
                break;

            case 2:
                stats = p2Stats;
                break;

            default:
                stats = p1Stats;
                break;
        }

        stats.BreakChain();
    }
}
