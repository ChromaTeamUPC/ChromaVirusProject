using UnityEngine;
using System.Collections;

public class MosquitoAIActionsBaseState : AIActionsBaseState
{
    //protected SpiderBiteExecutor spiderBiteExecutor;

    protected MosquitoBlackboard mosquitoBlackboard;

    public MosquitoAIActionsBaseState(MosquitoBlackboard bb): base(bb)
    {
        mosquitoBlackboard = bb;

        //spiderBiteExecutor = new SpiderBiteExecutor();
        //spiderBiteExecutor.Init(spiderBlackboard);
    }


    override protected void UpdateExecutor()
    {
        if (actionsList.Count > 0)
        {
            AIAction action = actionsList[currentActionIndex];

            switch (action.actionType)
            {
                /*case AIAction.Type.SPIDER_BITE:
                    currentExecutor = spiderBiteExecutor;
                    currentExecutor.SetAction(action);
                    break;*/

                default:
                    base.UpdateExecutor();
                    break;
            }
        }
        else
            base.UpdateExecutor();
    }
}
