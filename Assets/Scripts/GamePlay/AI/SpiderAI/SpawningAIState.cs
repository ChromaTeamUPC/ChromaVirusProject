using UnityEngine;
using System.Collections;

public class SpawningAIState : AIBaseState {

    public SpawningAIState(MonoBehaviour p): base (p) {}

    override public void OnStateEnter() { }

    override public void OnStateExit() { }

    override public AIBaseState Update()
    {
        if (!((SpiderAIBehaviour)parent).spawning)
            return nextState;

        return null;
    }
}
