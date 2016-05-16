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

    public void InitValues()
    {
        activeEnemies = 0;
        activeSpiders = 0;
        activeVortex = 0;
        activeTurrets = 0;

        activeDevices = 0;
        timeSinceLastDeviceAttack = 0f;
        enemiesAttackingDevice = new HashSet<EnemyBaseAIBehaviour>();
    }
}
