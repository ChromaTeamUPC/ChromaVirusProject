using UnityEngine;
using System.Collections;

public class SpiderEntryAIState : SpiderAIActionsBaseState {

    public SpiderEntryAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override AIBaseState Update()
    {
        /*When spider is in this state could happen this:
        1-If a barrel of the same color is activated and in range:
            Go to AttractedToBarrel State
        2-If player is in range:
            Go to AttackPlayer State
        3-When actions list is finished:
            Go to AttackPlayer State

        If none of above keep same state
        */
        if (spiderBlackboard.capacitorController != null && spiderBlackboard.capacitorController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.attractedToBarrelState;

        if (spiderBlackboard.spider.CheckPlayersDistance())
            return spiderBlackboard.attackingPlayerState;

        int updateResult = UpdateExecution();

        if (updateResult == AIAction.LIST_FINISHED)
            return spiderBlackboard.attackingPlayerState;
        else
            return ProcessUpdateExecutionResult(updateResult);
    }
}
