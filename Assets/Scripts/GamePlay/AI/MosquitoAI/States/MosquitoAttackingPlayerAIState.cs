using UnityEngine;
using System.Collections;

public class MosquitoAttackingPlayerAIState : MosquitoAIActionsBaseState
{
    public MosquitoAttackingPlayerAIState(MosquitoBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        rsc.enemyMng.blackboard.MosquitoStartsAttacking();
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        rsc.enemyMng.blackboard.MosquitoStopsAttacking();
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (blackboard.barrelController != null && blackboard.barrelController.currentColor == blackboard.entity.color)
            return mosquitoBlackboard.attractedToBarrelState;

        int updateResult = UpdateExecution();

        if (updateResult == AIAction.LIST_FINISHED)
            return mosquitoBlackboard.patrolingState;

        return ProcessUpdateExecutionResult(updateResult);
    }
}
