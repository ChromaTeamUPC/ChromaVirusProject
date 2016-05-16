using UnityEngine;
using System.Collections;

public class SpiderAIActionsBaseState : AIActionsBaseState
{
    protected SpiderBiteExecutor spiderBiteExecutor;

    protected SpiderBlackboard spiderBlackboard;

    public SpiderAIActionsBaseState(SpiderBlackboard bb): base(bb)
    {
        spiderBlackboard = bb;

        spiderBiteExecutor = new SpiderBiteExecutor();
        spiderBiteExecutor.Init(spiderBlackboard);
    }


    override protected void UpdateExecutor()
    {
        if (actionsList.Count > 0)
        {
            AIAction action = actionsList[currentActionIndex];

            switch (action.actionType)
            {
                case AIAction.Type.SPIDER_BITE:
                    currentExecutor = spiderBiteExecutor;
                    currentExecutor.SetAction(action);
                    break;

                default:
                    base.UpdateExecutor();
                    break;
            }
        }     
    }
}
