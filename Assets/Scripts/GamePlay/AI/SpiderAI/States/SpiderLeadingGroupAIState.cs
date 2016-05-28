using UnityEngine;
using System.Collections;

public class SpiderLeadingGroupAIState : SpiderAIActionsBaseState
{
    public SpiderLeadingGroupAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        spiderBlackboard.groupInfo.leader = spiderBlackboard.spider;
        currentActionIndex = spiderBlackboard.groupInfo.leaderActionIndex;
        UpdateExecutor();
    }

    public override void OnStateExit()
    {
        spiderBlackboard.groupInfo.leader = null;
    }

    public override AIBaseState Update()
    {
        /*When spider is in this state could happen this:
        1-If a barrel of the same color is activated and in range:
            Go to AttractedToBarrel State
        2-If spider is the last in the group and enemies attacking player < threshold:
            Go to AttackPlayer State
        3-Any other case:
            Loop action list
        */
        if (spiderBlackboard.barrelController != null && spiderBlackboard.barrelController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.spider.attractedToBarrelState;

        //TODO: check group count and attacking enemies

        int updateResult = UpdateExecution();

        if (updateResult == AIAction.LIST_FINISHED)
            return spiderBlackboard.spider.attackingPlayerState; //Should not happen because the list has to loop
        else
        {
            AIBaseState result = ProcessUpdateExecutionResult(updateResult);
            spiderBlackboard.groupInfo.leaderActionIndex = currentActionIndex;
            return result;
        }
    }
}
