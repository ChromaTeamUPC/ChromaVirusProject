using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIBaseState
{
    protected EnemyBaseBlackboard blackboard;

    public AIBaseState(EnemyBaseBlackboard bb)
    {
        blackboard = bb;
    }

    virtual public void Init() { }

    virtual public void OnStateEnter() {}

    virtual public void OnStateExit() { }

    virtual public AIBaseState Update()
    {
        return null;
    }

    virtual public AIBaseState ImpactedByShot(ChromaColor shotColor, int damage, Vector3 direction, PlayerController player)
    {
        return blackboard.entity.ProcessShotImpact(shotColor, damage, direction, player);
    }

    virtual public AIBaseState ImpactedByBarrel(ChromaColor barrelColor, int damage)
    {
        return blackboard.entity.ProcessBarrelImpact(barrelColor, damage);
    }
}

