using UnityEngine;
using System.Collections;

public class SpiderSpawningAIState : SpiderAIBaseState {

    public SpiderSpawningAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        spiderBlackboard.agent.enabled = false;
        spiderBlackboard.entity.mainCollider.enabled = false;
        spiderBlackboard.entity.dyingCollider.SetActive(false);
        spiderBlackboard.canReceiveDamage = false;
        spiderBlackboard.spawnAnimationEnded = false;

        spiderBlackboard.animator.Rebind(); //Restart state machine
        spiderBlackboard.animator.SetInteger("spawnAnimation", (int)spiderBlackboard.spawnAnimation);

        ColorEventInfo.eventInfo.newColor = spiderBlackboard.spider.color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_SPAWNED, ColorEventInfo.eventInfo);
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        spiderBlackboard.agent.enabled = true;
        spiderBlackboard.entity.mainCollider.enabled = true;
        spiderBlackboard.canReceiveDamage = true;

        if (rsc.colorMng.CurrentColor != blackboard.entity.color)
            blackboard.entity.shields[(int)blackboard.entity.color].SetActive(true);

        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (!spiderBlackboard.spawnAnimationEnded)
            return null;
        else
        {
            //If spider is in no group, go to entry state
            if (spiderBlackboard.groupInfo == null)
                return spiderBlackboard.entryState;
            //Else if spider is leader, go to leading group state
            else if (spiderBlackboard.groupInfo.leader == spiderBlackboard.spider)
                return spiderBlackboard.leadingGroupState;
            //else return following group state
            else
                return spiderBlackboard.followingGroupState;

        }
       
    }

    public override void ColorChanged(ChromaColor newColor)
    {
        //Do nothing
    }
}
