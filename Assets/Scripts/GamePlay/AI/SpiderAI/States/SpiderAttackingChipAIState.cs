using UnityEngine;
using System.Collections;

public class SpiderAttackingChipAIState : SpiderAIBaseState {

    public SpiderAttackingChipAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        rsc.enemyMng.blackboard.SpiderStartsInfectingDevice();
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        rsc.enemyMng.blackboard.SpiderStopsInfectingDevice();
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (spiderBlackboard.barrelController != null && spiderBlackboard.barrelController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.attractedToBarrelState;

        return null;
    }
}
