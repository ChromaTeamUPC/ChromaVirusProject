using UnityEngine;
using System.Collections;

public class SpiderBiteExecutor : BaseExecutor
{
    private SpiderBlackboard spiderBlackBoard;
    private SpiderBiteAIAction spiderBiteAction;

    private bool discardedAttack;

    public override void Init(EnemyBaseBlackboard bb)
    {
        base.Init(bb);
        spiderBlackBoard = (SpiderBlackboard)bb;      
    }

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        spiderBiteAction = (SpiderBiteAIAction)act;

        blackBoard.agent.Stop();

        if(spiderBlackBoard.timeSinceLastAttack > spiderBiteAction.minimumTimeSinceLastAttack)
        {
            blackBoard.animationEnded = false;
            blackBoard.animator.SetBool("walking", false);
            blackBoard.animator.SetTrigger("bite");
            spiderBlackBoard.timeSinceLastAttack = 0f;
            discardedAttack = false;
        }
        else
        {
            discardedAttack = true;
        }
    }

    public override int Execute()
    {
        if (discardedAttack || blackBoard.animationEnded)
            return action.nextAction;
        else
            return AIAction.ACTION_NOT_FINISHED;
    }
}
