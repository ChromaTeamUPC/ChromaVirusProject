using UnityEngine;
using System.Collections;

public class SpiderBiteExecutor : BaseExecutor
{
    private SpiderBiteAIAction spiderBiteAction;
    private SpiderAIBehaviour spider;

    private bool discardedAttack;

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        spiderBiteAction = (SpiderBiteAIAction)act;

        spider = (SpiderAIBehaviour)state.parent;

        state.agent.Stop();

        if(spider.timeSinceLastAttack > spiderBiteAction.minimumTimeSinceLastAttack)
        {
            spider.animator.SetTrigger("bite");
            spider.timeSinceLastAttack = 0f;
            spider.biting = true;
            discardedAttack = false;
        }
        else
        {
            discardedAttack = true;
        }
    }

    public override int Execute()
    {
        if (discardedAttack || !spider.biting)
            return action.nextAction;
        else
            return AIAction.ACTION_NOT_FINISHED;
    }
}
