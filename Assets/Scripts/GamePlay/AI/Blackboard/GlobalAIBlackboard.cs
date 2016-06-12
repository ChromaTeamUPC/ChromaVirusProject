using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalAIBlackboard
{
    public int activeEnemies;
    public int activeSpiders;
    public int activeVortex;
    public int activeTurrets;

    public int activeDevices;
    public float timeSinceLastDeviceAttack;
    public HashSet<EnemyBaseAIBehaviour> enemiesAttackingDevice;

    public const float timeBetweenPlayerAttacks = 1f;
    public int attackingPlayerEnemies;
    public int attackingPlayerSpiders;
    public int attackingPlayerMosquitoes;
    public int activeMosquitoShots;

    public float timeRemainingToNextPlayerAttack;

    public void InitValues()
    {
        activeEnemies = 0;
        activeSpiders = 0;
        activeVortex = 0;
        activeTurrets = 0;

        activeDevices = 0;
        timeSinceLastDeviceAttack = 0f;
        enemiesAttackingDevice = new HashSet<EnemyBaseAIBehaviour>();

        attackingPlayerEnemies = 0;
        attackingPlayerSpiders = 0;
        attackingPlayerMosquitoes = 0;
        activeMosquitoShots = 0;
        timeRemainingToNextPlayerAttack = 0f;
    }

    public void SpiderStartsAttacking()
    {
        ++attackingPlayerEnemies;
        ++attackingPlayerSpiders;
        timeRemainingToNextPlayerAttack = timeBetweenPlayerAttacks;
    }

    public void SpiderStopsAttacking()
    {
        --attackingPlayerEnemies;
        --attackingPlayerSpiders;
    }

    public void MosquitoStartsAttacking()
    {
        ++attackingPlayerEnemies;
        ++attackingPlayerMosquitoes;
    }

    public void MosquitoStopsAttacking()
    {
        --attackingPlayerEnemies;
        --attackingPlayerMosquitoes;
    }

    public void MosquitoShotSpawned()
    {
        ++activeMosquitoShots;
    }

    public void MosquitoShotDestroyed()
    {
        --activeMosquitoShots;
    }

    public void DecrementTimes()
    {
        if (timeRemainingToNextPlayerAttack > 0f)
        {
            timeRemainingToNextPlayerAttack -= Time.deltaTime;
            if (timeRemainingToNextPlayerAttack < 0f)
                timeRemainingToNextPlayerAttack = 0f;
        }
    }
}
