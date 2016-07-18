using UnityEngine;
using System.Collections;

public class PlayerStats
{
    public uint currentCombo;
    public uint maxCombo;
    public uint enemiesKilledOk;
    public uint enemiesKilledWrong;

    public PlayerStats()
    {
        Reset();
    }

    public void Reset()
    {
        currentCombo = 0;
        maxCombo = 0;
        enemiesKilledOk = 0;
        enemiesKilledWrong = 0;
    }
}

public class StatsManager : MonoBehaviour 
{
    private float startTime;
    private float totalTime;
    public PlayerStats p1Stats;
    public PlayerStats p2Stats;

    void Awake()
    {
        p1Stats = new PlayerStats();
        p2Stats = new PlayerStats();
    }

	// Use this for initialization
	void Start () 
	{
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameFinished);
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_STARTED, LevelStarted);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameFinished);
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
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
        startTime = Time.time;
        totalTime = 0f;
    }

    private void LevelCleared(EventInfo eventInfo)
    {
        totalTime = Time.time - startTime;
    }

    private void GameFinished(EventInfo eventInfo)
    {
        //ComputeScore();
    }

    private void EnemyDied(EventInfo eventInfo)
    {
        EnemyDiedEventInfo info = (EnemyDiedEventInfo)eventInfo;

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

        if(info.killedSameColor)
        {
            stats.enemiesKilledOk++;
            stats.currentCombo++;

            if (stats.currentCombo > stats.maxCombo)
                stats.maxCombo = stats.currentCombo;
        }
        else
        {
            stats.enemiesKilledWrong++;
            stats.currentCombo = 0;
        }
    }

}
