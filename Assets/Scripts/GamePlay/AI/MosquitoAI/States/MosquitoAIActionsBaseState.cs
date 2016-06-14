using UnityEngine;
using System.Collections;

public class MosquitoAIActionsBaseState : AIActionsBaseState
{
    protected MosquitoShotExecutor mosquitoShotExecutor;

    protected MosquitoBlackboard mosquitoBlackboard;

    public MosquitoAIActionsBaseState(MosquitoBlackboard bb): base(bb)
    {
        mosquitoBlackboard = bb;

        mosquitoShotExecutor = new MosquitoShotExecutor();
        mosquitoShotExecutor.Init(mosquitoBlackboard);
    }


    override protected void UpdateExecutor()
    {
        if (actionsList.Count > 0)
        {
            AIAction action = actionsList[currentActionIndex];

            switch (action.actionType)
            {
                case AIAction.Type.MOSQUITO_ATTACK:
                    currentExecutor = mosquitoShotExecutor;
                    currentExecutor.SetAction(action);
                    break;

                default:
                    base.UpdateExecutor();
                    break;
            }
        }
        else
            base.UpdateExecutor();
    }
}
