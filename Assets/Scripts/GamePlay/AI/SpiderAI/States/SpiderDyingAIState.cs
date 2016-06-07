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
            
        ColorEventInfo.eventInfo.newColor = color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_DIED, ColorEventInfo.eventInfo);

        if (spiderBlackboard.lastShotSameColor)
        {
            spiderBlackboard.entity.mainCollider.enabled = false;
            spiderBlackboard.entity.dyingCollider.SetActive(true);
            spiderBlackboard.canReceiveDamage = false;
            spiderBlackboard.animationEnded = false;
            spiderBlackboard.animator.SetTrigger("die");

            movement = spiderBlackboard.lastShotDirection * spiderBlackboard.entity.shotForceModifier;

            GameObject explosion = spiderBlackboard.explosions[(int)color];
            explosion.SetActive(true);

            /*Vector3 pos = spiderBlackboard.spider.transform.position;
            ScriptObjectPool<EnergyVoxelController> pool = rsc.poolMng.energyVoxelPool;
            for (int i = 0; i < spiderBlackboard.spider.energyVoxelsSpawnedOnDie; ++i)
            {
                EnergyVoxelController voxel = pool.GetObject();
                if (voxel != null)
                {
                    voxel.transform.position = pos;
                    voxel.transform.rotation = Random.rotation;
                }
            }*/
        }
        else
            spiderBlackboard.spider.SpawnVoxelsAndReturnToPool();
    }

    public override void OnStateExit()
    {
        spiderBlackboard.agent.enabled = true;
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        spiderBlackboard.spider.transform.position += movement * Time.deltaTime;

        if (spiderBlackboard.animationEnded)
        {
            Vector3 pos = spiderBlackboard.spider.transform.position;
            ScriptObjectPool<EnergyVoxelController> pool = rsc.poolMng.energyVoxelPool;
            for (int i = 0; i < spiderBlackboard.spider.energyVoxelsSpawnedOnDie; ++i)
            {
                EnergyVoxelController voxel = pool.GetObject();
                if (voxel != null)
                {
                    voxel.transform.position = pos;
                    voxel.transform.rotation = Random.rotation;
                }
            }

            spiderBlackboard.spider.SpawnVoxelsAndReturnToPool();
        }

        return null;
    }
}
