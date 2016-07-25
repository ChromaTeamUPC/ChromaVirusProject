using UnityEngine;
using System.Collections;

public class WormAIBaseState
{
    public WormBlackboard blackboard;

    public WormAIBaseState(WormBlackboard bb)
    {
        blackboard = bb;
    }

    virtual public void Init() { }

    virtual public void OnStateEnter() { }

    virtual public void OnStateExit() { }

    virtual public WormAIBaseState Update()
    {
        return null;
    }
}
