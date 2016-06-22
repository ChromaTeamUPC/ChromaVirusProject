using UnityEngine;
using System.Collections;

public class SpiderInfectExecutor : BaseExecutor
{
    private SpiderBlackboard spiderBlackBoard;

    public override void Init(EnemyBaseBlackboard bb)
    {
        base.Init(bb);
        spiderBlackBoard = (SpiderBlackboard)bb;
    }

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);

        blackBoard.agent.Stop();

        blackBoard.attackAnimationEnded = false;
        blackBoard.attackAnimationTrigger = false;
        blackBoard.animator.SetBool("moving", false);
        blackBoard.animator.SetTrigger("bite");
    }

    public override int Execute()
    {
        if (blackBoard.attackAnimationEnded)
        {
            return action.nextAction;
        }
        else if (blackBoard.attackAnimationTrigger)
        {
            blackBoard.attackAnimationTrigger = false;

            blackBoard.deviceController.Infect();
        }

        return AIAction.ACTION_NOT_FINISHED;
    }
}
