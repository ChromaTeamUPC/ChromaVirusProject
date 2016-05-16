using UnityEngine;
using System.Collections;

public class SelectPlayerExecutor: BaseExecutor
{
    private SelectPlayerAIAction selectAction;

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        selectAction = (SelectPlayerAIAction)act;
    }

    public override int Execute()
    {
        if((blackBoard.target == null) || (!blackBoard.target.activeSelf) || selectAction.overrideValidPlayer)
            blackBoard.target = rsc.enemyMng.SelectTarget(blackBoard.entityGO);
        return action.nextAction;
    }
}
