using UnityEngine;
using System.Collections;

public class SelectTargetExecutor: BaseExecutor
{
    public override int Execute()
    {
        state.target = rsc.enemyMng.SelectTarget(state.parent.gameObject);
        return action.nextAction;
    }
}
