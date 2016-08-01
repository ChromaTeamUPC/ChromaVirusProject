using UnityEngine;
using System.Collections;

public class WormAIDyingState : WormAIBaseState 
{
    private enum SubState
    {
        GOING_TO_ENTRY,
        ENTERING,
        FOLLOWING_PATH,
        EXITING
    }

    private SubState subState;

    private WormRoute route;
    private int WPIndex;
    private Vector3 currentWP;
    private Vector3 nextWP;
    private Transform head;
    private Quaternion lookRotation;

    public WormAIDyingState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        bb.Explode();
    }

    
    public override WormAIBaseState Update()
    {      
        return null;
    }

    public override WormAIBaseState ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        //do nothing
        return null;
    }
}
