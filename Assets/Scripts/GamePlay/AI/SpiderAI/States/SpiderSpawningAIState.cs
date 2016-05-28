using UnityEngine;
using System.Collections;

public class SpiderSpawningAIState : SpiderAIBaseState {

    public SpiderSpawningAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        spiderBlackboard.agent.enabled = false;
        spiderBlackboard.entity.mainCollider.enabled = false;
        spiderBlackboard.canReceiveDamage = false;
        spiderBlackboard.animationEnded = false;

        spiderBlackboard.animator.Rebind(); //Restart state machine
        spiderBlackboard.animator.SetInteger("spawnAnimation", (int)spiderBlackboard.spawnAnimation);

        ColorEventInfo.eventInfo.newColor = spiderBlackboard.spider.color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_SPAWNED, ColorEventInfo.eventInfo);
    }

    public override void OnStateExit()
    {
        spiderBlackboard.agent.enabled = true;
        spiderBlackboard.entity.mainCollider.enabled = true;
        spiderBlackboard.canReceiveDamage = true;
    }

    public override AIBaseState Update()
    {
        if (!spiderBlackboard.animationEnded)
            return null;
        else
        {
            //If spider is in no group, go to entry state
            if (spiderBlackboard.groupInfo == null)
                return spiderBlackboard.spider.entryState;
            //Else if spider is leader, go to leading group state
            else if (spiderBlackboard.groupInfo.leader == spiderBlackboard.spider)
                return spiderBlackboard.spider.leadingGroupState;
            //else return following group state
            else
                return spiderBlackboard.spider.followingGroupState;

        }
       
    }
}
