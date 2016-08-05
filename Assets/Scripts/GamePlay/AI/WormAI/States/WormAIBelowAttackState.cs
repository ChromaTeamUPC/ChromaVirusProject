using UnityEngine;
using System.Collections;

public class WormAIBelowAttackState : WormAIBaseState
{
    private enum SubState
    {
        WAITING,
        WARNING_PLAYER,
        JUMPING,
        EXITING
    }

    private SubState subState;

    public WormAIBelowAttackState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override WormAIBaseState Update()
    {
        return base.Update();
    }
}
