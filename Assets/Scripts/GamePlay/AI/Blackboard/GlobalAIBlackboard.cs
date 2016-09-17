using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalAIBlackboard
{
    EnemyManager enemyMng;

    public int zoneTotalInfectionLevel;
    public int zoneCurrentInfectionLevel;

    public int activeEnemies;
    //public int activeSpiders;
    public int activeVortex;
    public int activeTurrets;

    public List<DeviceController> activeDevices = new List<DeviceController>();
    public float timeSinceLastDeviceAttack;
    public int infectingDeviceSpiders;

    public float timeRemainingToNextPlayerAttack;

    public int attackingPlayerEnemies;
    public int attackingPlayerSpiders;
    public int attackingPlayerMosquitoes;
    public int activeMosquitoMainShots;
    public int activeMosquitoWeakShots;

    public float timeRemainingToNextDeviceInfect;

    public WormBlackboard worm;


    public void Init(EnemyManager eMng)
    {
        enemyMng = eMng;

        ResetValues();
    }

    public void ResetValues()
    {
        zoneTotalInfectionLevel = 100;
        zoneCurrentInfectionLevel = 100;

        activeEnemies = 0;
        //activeSpiders = 0;
        activeVortex = 0;
        activeTurrets = 0;

        activeDevices.Clear();
        timeSinceLastDeviceAttack = 0f;
        infectingDeviceSpiders = 0;

        timeRemainingToNextPlayerAttack = 0f;

        attackingPlayerEnemies = 0;
        attackingPlayerSpiders = 0;
        attackingPlayerMosquitoes = 0;
        activeMosquitoMainShots = 0;
        activeMosquitoWeakShots = 0;

        timeRemainingToNextDeviceInfect = enemyMng.timeBetweenDeviceInfects;

        worm = null;
    }

    public void SpiderStartsAttacking()
    {
        ++attackingPlayerEnemies;
        ++attackingPlayerSpiders;
        timeRemainingToNextPlayerAttack = enemyMng.timeBetweenPlayerAttacks;
    }

    public void SpiderStopsAttacking()
    {
        --attackingPlayerEnemies;
        --attackingPlayerSpiders;
    }

    public void SpiderStartsInfectingDevice()
    {
        ++infectingDeviceSpiders;
        timeRemainingToNextDeviceInfect = enemyMng.timeBetweenDeviceInfects;
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

    public void MosquitoMainShotSpawned()
    {
        ++activeMosquitoMainShots;
    }

    public void MosquitoMainShotDestroyed()
    {
        --activeMosquitoMainShots;
    }

    public void MosquitoWeakShotSpawned()
    {
        ++activeMosquitoWeakShots;
    }

    public void MosquitoWeakShotDestroyed()
    {
        --activeMosquitoWeakShots;
    }

    public void DecrementTimes()
    {
        if (timeRemainingToNextPlayerAttack > 0f)
        {
            timeRemainingToNextPlayerAttack -= Time.deltaTime;
            if (timeRemainingToNextPlayerAttack < 0f)
                timeRemainingToNextPlayerAttack = 0f;
        }

        if(timeRemainingToNextDeviceInfect > 0f && infectingDeviceSpiders < enemyMng.spidersInfectingDeviceThreshold)
        {
            timeRemainingToNextDeviceInfect -= Time.deltaTime;
            if (timeRemainingToNextDeviceInfect < 0f)
                timeRemainingToNextDeviceInfect = 0f;
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

    public int GetCurrentInfectionPercentage()
    {
        float total = zoneTotalInfectionLevel;
        float current = zoneCurrentInfectionLevel;

        foreach(DeviceController device in activeDevices)
        {
            total += DeviceController.globalInfectionValue;
            current += device.CurrentGlobalInfectionValue;
        }

        return (int)Mathf.Ceil((current * 100) / total);
    }
}
