using UnityEngine;
using System.Collections;

public class SpiderAttackingPlayerAIState : SpiderAIActionsBaseState
{

    public SpiderAttackingPlayerAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override AIBaseState Update()
    {
        if (spiderBlackboard.barrelController != null && spiderBlackboard.barrelController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.spider.attractedToBarrelState;

        return base.Update();
    }
}
