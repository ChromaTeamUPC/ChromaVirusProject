using UnityEngine;
using System.Collections;

public class PlayerStats
{
    private int playerId;
    ChromaColor lastKillColor;
    public uint chain;
    public float chainMaxTime;
    private uint specialComboIncrement;
    public float comboRemainingTime;

    public uint currentCombo;
    public uint maxCombo;
    public uint enemiesKilledOk;
    public uint enemiesKilledWrong;

    public PlayerStats(int id, float chainMax, uint specialComboInc)
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

        chain = 0;
        comboRemainingTime = 0f;
        currentCombo = 0;
        maxCombo = 0;
    }

    public void UpdateComboTime()
    {
        if (comboRemainingTime > 0)
        {
            comboRemainingTime -= Time.deltaTime;

            if(comboRemainingTime <= 0)
            {
                comboRemainingTime = 0;
                chain = 0;
                currentCombo = 0;
                ComboEventInfo.eventInfo.playerId = playerId;
                rsc.eventMng.TriggerEvent(EventManager.EventType.COMBO_BREAK, ComboEventInfo.eventInfo);
            }
        }
    }

    public void EnemyKilledOk(ChromaColor color, bool specialKill)
    {
        ++enemiesKilledOk;

        uint total;

        if(specialKill)
        {
            total = specialComboIncrement;

            //To force start a chain
            if (chain == 0)
            {
                ++chain;
                lastKillColor = color;
            }
        }
        else
        {
            //If it was a chain ongoing
            if (chain > 0)
            {
                if (color == lastKillColor)
                {
                    ++chain;
                }
                else
                {
                    chain = 1;
                    lastKillColor = color;
                }
            }
            //Start chain
            else
            {
                ++chain;
                lastKillColor = color;
            }

            total = chain;
        }

        currentCombo += total;
        if (currentCombo > maxCombo)
            maxCombo = currentCombo;

        comboRemainingTime = chainMaxTime;

        ComboEventInfo.eventInfo.playerId = playerId;
        ComboEventInfo.eventInfo.comboColor = lastKillColor;
        ComboEventInfo.eventInfo.comboAdd = total;
        rsc.eventMng.TriggerEvent(EventManager.EventType.COMBO_ADD, ComboEventInfo.eventInfo);
    }

    public void EnemyKilledWrong()
    {
        ++enemiesKilledWrong;
        BreakCombo();
    }

    public void BreakCombo()
    {
        currentCombo = 0;
        chain = 0;
        comboRemainingTime = 0;

        ComboEventInfo.eventInfo.playerId = playerId;
        rsc.eventMng.TriggerEvent(EventManager.EventType.COMBO_BREAK, ComboEventInfo.eventInfo);
    }
}

public class StatsManager : MonoBehaviour 
{
    private float startTime;
    private float totalTime;
    public PlayerStats p1Stats;
    public PlayerStats p2Stats;

    public float chainMaxTime = 5f;
    public uint specialKillsComboIncrement = 3;
    private bool updateComboTime = false;

    void Awake()
    {
        p1Stats = new PlayerStats(1, chainMaxTime, specialKillsComboIncrement);
        p2Stats = new PlayerStats(2, chainMaxTime, specialKillsComboIncrement);
    }

	// Use this for initialization
	void Start ()
	{
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_REACHED, ZoneStarted);
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZoneFinished);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameFinished);
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_DESTROYED, EnemyDied); //Same management as enemydied
        rsc.eventMng.StartListening(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDied); //Same management as enemydied
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_REACHED, ZoneStarted);
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZoneFinished);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameFinished);
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_DESTROYED, EnemyDied);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDied);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        }
    }

    void Update()
    {
        if (updateComboTime && rsc.enemyMng.AreEnemies())
        {
            p1Stats.UpdateComboTime();
            p2Stats.UpdateComboTime();
        }
    }

	
	private void GameReset(EventInfo eventInfo)
    {     
        p1Stats.Reset();
        p2Stats.Reset();

        startTime = 0f;
        totalTime = 0f;
    }

    private void LevelStarted(EventInfo eventInfo)
    {
        updateComboTime = true;
        startTime = Time.time;
        totalTime = 0f;
    }

    private void LevelCleared(EventInfo eventInfo)
    {
        updateComboTime = false;
        totalTime = Time.time - startTime;
    }

    private void ZoneStarted(EventInfo eventInfo)
    {
        updateComboTime = true;
    }

    private void ZoneFinished(EventInfo eventInfo)
    {
        updateComboTime = false;
    }

    private void GameFinished(EventInfo eventInfo)
    {
        //ComputeScore();
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

        stats.BreakCombo();
    }
}
