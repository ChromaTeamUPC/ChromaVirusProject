using UnityEngine;
using System.Collections;

public class SpiderDyingAIState : SpiderAIBaseState
{
    private ChromaColor color;

    public SpiderDyingAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        spiderBlackboard.entity.mainCollider.enabled = false;
        spiderBlackboard.canReceiveDamage = false;
        spiderBlackboard.animationEnded = false;
        spiderBlackboard.animator.SetTrigger("die");

        color = spiderBlackboard.spider.color;

        //spiderBlackboard.agent.Stop();
        spiderBlackboard.agent.Stop();
        spiderBlackboard.agent.enabled = false;
        spiderBlackboard.rigidBody.isKinematic = false;
        spiderBlackboard.rigidBody.AddForce(spiderBlackboard.lastShotDirection * spiderBlackboard.entity.shotForceModifier, ForceMode.Impulse);

        ColorEventInfo.eventInfo.newColor = color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_DIED, ColorEventInfo.eventInfo);
    }

    public override void OnStateExit()
    {
        spiderBlackboard.rigidBody.isKinematic = true;
        spiderBlackboard.agent.enabled = true;
    }

    public override AIBaseState Update()
    {
        if (spiderBlackboard.animationEnded)
        {
            spiderBlackboard.spider.SpawnVoxels();
            rsc.poolMng.spiderPool.AddObject(spiderBlackboard.entityGO);
        }

        return null;
    }
}
