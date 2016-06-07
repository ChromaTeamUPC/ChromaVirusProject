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
            blackBoard.animationTrigger = false;
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
        {
            return action.nextAction;
        }
        else if (blackBoard.animationTrigger)
        {
            SpiderBolt bolt = rsc.poolMng.spiderBoltPool.GetObject();
            if (bolt != null)
            {
                bolt.transform.position = spiderBlackBoard.boltSpawnPoint.position;
                bolt.color = spiderBlackBoard.spider.color;
                bolt.damage = spiderBlackBoard.spider.biteDamage;
                bolt.origin = spiderBlackBoard.spider.transform.position;
                bolt.Spawn();
            }
        }
        
        return AIAction.ACTION_NOT_FINISHED;
    }
}
