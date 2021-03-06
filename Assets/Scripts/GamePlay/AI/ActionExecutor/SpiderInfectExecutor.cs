﻿using UnityEngine;
using System.Collections;

public class SpiderInfectExecutor : BaseExecutor
{
    private SpiderBlackboard spiderBlackBoard; //disabled to avoid warnings. reenable if needed

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

            /*SpiderBolt bolt = rsc.poolMng.spiderBoltPool.GetObject();
            if (bolt != null)
            {
                bolt.transform.position = spiderBlackBoard.boltSpawnPoint.position;
                bolt.color = spiderBlackBoard.spider.color;
                bolt.damage = spiderBlackBoard.spider.biteDamage;
                bolt.origin = spiderBlackBoard.spider.transform.position;
                bolt.Spawn(false);
            }*/

            SpiderSparks sparks = rsc.poolMng.spiderSparksPool.GetObject();
            if (sparks != null)
            {
                sparks.transform.position = spiderBlackBoard.boltSpawnPoint.position;
            }

            blackBoard.deviceController.Infect();
        }

        return AIAction.ACTION_NOT_FINISHED;
    }
}
