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
            return spiderBlackboard.spider.entryState;
       
    }
}
