using UnityEngine;
using System.Collections;

public class SpiderDyingAIState : SpiderAIBaseState
{
    private ChromaColor color;
    private Vector3 movement;

    public SpiderDyingAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        spiderBlackboard.entity.mainCollider.enabled = false;
        spiderBlackboard.canReceiveDamage = false;
        spiderBlackboard.animationEnded = false;
        spiderBlackboard.animator.SetTrigger("die");

        color = spiderBlackboard.spider.color;

        if (spiderBlackboard.groupInfo != null)
            spiderBlackboard.groupInfo.groupCount--;

        spiderBlackboard.agent.Stop();
        spiderBlackboard.agent.enabled = false;
        movement = spiderBlackboard.lastShotDirection * spiderBlackboard.entity.shotForceModifier;

        GameObject explosion = spiderBlackboard.explosions[(int)color];
        explosion.SetActive(true);

        ColorEventInfo.eventInfo.newColor = color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_DIED, ColorEventInfo.eventInfo);
    }

    public override void OnStateExit()
    {
        spiderBlackboard.agent.enabled = true;
    }

    public override AIBaseState Update()
    {
        spiderBlackboard.spider.transform.position += movement * Time.deltaTime;

        if (spiderBlackboard.animationEnded)
        {         
            spiderBlackboard.spider.SpawnVoxels();
            rsc.poolMng.spiderPool.AddObject(spiderBlackboard.entityGO);
        }

        return null;
    }
}
