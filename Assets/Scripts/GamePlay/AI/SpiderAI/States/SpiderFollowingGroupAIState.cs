using UnityEngine;
using System.Collections;

public class SpiderFollowingGroupAIState : SpiderAIActionsBaseState
{
    public SpiderFollowingGroupAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override AIBaseState Update()
    {
        /*When spider is in this state could happen this:
        1-If a barrel of the same color is activated and in range:
            Go to AttractedToBarrel State
        2-If there is no lider
            Go to LeadingGroupState
        3-If enemies attacking player < threshold && minimum time has passed:
            Go to AttackPlayer State
        3-Any other case:
            Loop action list
        */
        if (spiderBlackboard.barrelController != null && spiderBlackboard.barrelController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.spider.attractedToBarrelState;

        if(spiderBlackboard.groupInfo.leader == null || !spiderBlackboard.groupInfo.leader.gameObject.activeSelf)
        {
            return spiderBlackboard.spider.leadingGroupState;
        }

        //TODO: check group count and attacking enemies

        int updateResult = UpdateExecution();

        if (updateResult == AIAction.LIST_FINISHED)
            return spiderBlackboard.spider.attackingPlayerState; //Should not happen because the list has to loop
        else
        {
            return ProcessUpdateExecutionResult(updateResult);
        }
    }
}
