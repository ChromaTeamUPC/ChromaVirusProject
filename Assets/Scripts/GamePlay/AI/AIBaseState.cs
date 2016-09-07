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

    virtual public void ColorChanged(ChromaColor newColor)
    {
        blackboard.entity.ProcessColorChanged(newColor);
    }

    virtual public AIBaseState ImpactedByShot(ChromaColor shotColor, float damage, Vector3 direction, PlayerController player)
    {
        return blackboard.entity.ProcessShotImpact(shotColor, damage, direction, player);
    }

    virtual public AIBaseState ImpactedBySpecial(float damage, Vector3 direction, PlayerController player)
    {
        return blackboard.entity.ProcessSpecialImpact(damage, direction, player);
    }

    virtual public AIBaseState ImpactedByBarrel(ChromaColor barrelColor, float damage, Vector3 direction, PlayerController player)
    {
        return blackboard.entity.ProcessBarrelImpact(barrelColor, damage, direction, player);
    }

    virtual public AIBaseState ImpactedByHexagon()
    {
        return blackboard.entity.ProcessHexagonImpact();
    }
}

