using UnityEngine;
using System.Collections;

public class MosquitoAttackingPlayerAIState : MosquitoAIActionsBaseState
{
    public MosquitoAttackingPlayerAIState(MosquitoBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        rsc.enemyMng.bb.MosquitoStartsAttacking();
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        rsc.enemyMng.bb.MosquitoStopsAttacking();
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (blackboard.capacitorController != null && blackboard.capacitorController.currentColor == blackboard.entity.color)
            return mosquitoBlackboard.attractedToBarrelState;

        int updateResult = UpdateExecution();

        if (updateResult == AIAction.LIST_FINISHED)
            return mosquitoBlackboard.patrolingState;

        return ProcessUpdateExecutionResult(updateResult);
    }
}
