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
    private Quaternion lookRotation;

    public WormAIDyingState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        EnemyDiedEventInfo.eventInfo.infectionValue = 0;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_DIED, EnemyDiedEventInfo.eventInfo);

        bb.Explode();
    }

    
    public override WormAIBaseState Update()
    {      
        return null;
    }

    public override void PlayerTouched(PlayerController player, Vector3 origin)
    {
        //do nothing
    }

    public override bool CanSpawnMinion()
    {
        return false;
    }
}
