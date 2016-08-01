using UnityEngine;
using System.Collections;

public class WormAIBaseState
{
    public WormBlackboard bb;

    public WormAIBaseState(WormBlackboard bb)
    {
        this.bb = bb;
    }

    virtual public void Init() { }

    virtual public void OnStateEnter() { }

    virtual public void OnStateExit() { }

    virtual public WormAIBaseState Update()
    {
        return null;
    }

    public virtual WormAIBaseState ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        return bb.worm.ProcessShotImpact(shotColor, damage, player);
    }
}
