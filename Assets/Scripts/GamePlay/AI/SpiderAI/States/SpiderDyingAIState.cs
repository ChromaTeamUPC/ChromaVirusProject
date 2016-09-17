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
        base.OnStateEnter();
        spiderBlackboard.agent.Stop();
        spiderBlackboard.agent.enabled = false;

        color = spiderBlackboard.spider.color;

        blackboard.entity.DisableShields();

        rsc.rumbleMng.Rumble(0, 0.25f, 0f, 0.5f);

        EnemyDiedEventInfo.eventInfo.color = color;
        EnemyDiedEventInfo.eventInfo.infectionValue = SpiderAIBehaviour.infectionValue;
        EnemyDiedEventInfo.eventInfo.killerPlayer = spiderBlackboard.lastShotPlayer;
        EnemyDiedEventInfo.eventInfo.killedSameColor = spiderBlackboard.lastShotSameColor;
        EnemyDiedEventInfo.eventInfo.specialKill = spiderBlackboard.specialKill;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_DIED, EnemyDiedEventInfo.eventInfo);

        if (spiderBlackboard.lastShotSameColor)
        {
            spiderBlackboard.entity.mainCollider.enabled = false;
            spiderBlackboard.entity.dyingCollider.SetActive(true);
            spiderBlackboard.canReceiveDamage = false;
            spiderBlackboard.dieAnimationEnded = false;
            spiderBlackboard.animator.SetTrigger("die");

            movement = spiderBlackboard.lastShotDirection * spiderBlackboard.entity.shotForceModifier;

            EnemyExplosionController explosion = rsc.poolMng.enemyExplosionPool.GetObject();

            if (explosion != null)
            {
                explosion.transform.position = blackboard.entityGO.transform.position;
                explosion.Play(color);
            }
        }
        else
            spiderBlackboard.spider.SpawnVoxelsAndReturnToPool(false);
    }

    public override void OnStateExit()
    {
        spiderBlackboard.agent.enabled = true;
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        spiderBlackboard.spider.transform.position += movement * Time.deltaTime;

        if (spiderBlackboard.dieAnimationEnded)
        {
            spiderBlackboard.spider.SpawnVoxelsAndReturnToPool();
        }

        return null;
    }

    public override void ColorChanged(ChromaColor newColor)
    {
        //Do nothing
    }

    public override AIBaseState InstantKill()
    {
        return null;
    }
}
