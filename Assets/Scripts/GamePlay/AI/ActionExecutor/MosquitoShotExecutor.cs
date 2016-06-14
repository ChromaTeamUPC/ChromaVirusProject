using UnityEngine;
using System.Collections;

public class MosquitoShotExecutor : BaseExecutor
{
    private MosquitoBlackboard mosquitoBlackboard;
    private MosquitoShotAIAction shotAction;

    public override void Init(EnemyBaseBlackboard bb)
    {
        base.Init(bb);
        mosquitoBlackboard = (MosquitoBlackboard)bb;
    }

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        shotAction = (MosquitoShotAIAction)act;

        blackBoard.agent.Stop();

        //Start shot animation
        blackBoard.attackAnimationEnded = false;
        blackBoard.attackAnimationTrigger = false;
    }

    public override int Execute()
    {
        return base.Execute();
    }
}
