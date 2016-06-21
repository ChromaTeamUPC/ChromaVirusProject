using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalAIBlackboard
{
    public int activeEnemies;
    //public int activeSpiders;
    public int activeVortex;
    public int activeTurrets;

    public List<DeviceController> activeDevices;
    public float timeSinceLastDeviceAttack;
    public int infectingDeviceSpiders;

    public const float timeBetweenPlayerAttacks = 1f;
    public int attackingPlayerEnemies;
    public int attackingPlayerSpiders;
    public int attackingPlayerMosquitoes;
    public int activeMosquitoShots;

    public float timeRemainingToNextPlayerAttack;

    public void Init()
    {
        activeDevices = new List<DeviceController>();

        ResetValues();
    }

    public void ResetValues()
    {

        activeEnemies = 0;
        //activeSpiders = 0;
        activeVortex = 0;
        activeTurrets = 0;

        activeDevices.Clear();
        timeSinceLastDeviceAttack = 0f;
        infectingDeviceSpiders = 0;

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

    public void SpiderStartsInfectingDevice()
    {
        ++infectingDeviceSpiders;
    }

    public void SpiderStopsInfectingDevice()
    {
        --infectingDeviceSpiders;
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

    public float GetTotalDevicesInfection()
    {
        float total = 0f;

        for (int i = 0; i < activeDevices.Count; ++i)
            total += activeDevices[i].CurrentInfectionAmount;

        return total;
    }

    public DeviceController GetRandomDevice()
    {
        return activeDevices[Random.Range(0, activeDevices.Count)];
    }
}
