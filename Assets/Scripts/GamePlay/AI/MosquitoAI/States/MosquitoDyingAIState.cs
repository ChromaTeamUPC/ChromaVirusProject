using UnityEngine;
using System.Collections;

public class MosquitoDyingAIState : MosquitoAIBaseState
{
    private ChromaColor color;
    private Vector3 movement;

    public MosquitoDyingAIState(MosquitoBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        mosquitoBlackboard.agent.Stop();
        mosquitoBlackboard.agent.enabled = false;

        color = mosquitoBlackboard.mosquito.color;

        blackboard.entity.DisableShields();

        rsc.rumbleMng.Rumble(0, 0.25f, 0f, 0.5f);

        EnemyDiedEventInfo.eventInfo.color = color;
        EnemyDiedEventInfo.eventInfo.infectionValue = MosquitoAIBehaviour.infectionValue;
        EnemyDiedEventInfo.eventInfo.killerPlayer = mosquitoBlackboard.lastShotPlayer;
        EnemyDiedEventInfo.eventInfo.killedSameColor = mosquitoBlackboard.lastShotSameColor;
        EnemyDiedEventInfo.eventInfo.specialKill = mosquitoBlackboard.specialKill;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_DIED, EnemyDiedEventInfo.eventInfo);

        if (mosquitoBlackboard.lastShotSameColor)
        {
            mosquitoBlackboard.entity.mainCollider.enabled = false;
            mosquitoBlackboard.entity.dyingCollider.SetActive(true);
            mosquitoBlackboard.canReceiveDamage = false;
            mosquitoBlackboard.dieAnimationEnded = false;
            mosquitoBlackboard.animator.SetTrigger("die");

            movement = mosquitoBlackboard.lastShotDirection * mosquitoBlackboard.entity.shotForceModifier;

            EnemyExplosionController explosion = rsc.poolMng.enemyExplosionPool.GetObject();

            if (explosion != null)
            {
                explosion.transform.position = blackboard.entityGO.transform.position;
                explosion.Play(color);
            }
        }
        else
            mosquitoBlackboard.mosquito.SpawnVoxelsAndReturnToPool(false);
    }

    public override void OnStateExit()
    {
        mosquitoBlackboard.agent.enabled = true;
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        mosquitoBlackboard.mosquito.transform.position += movement * Time.deltaTime;

        if (mosquitoBlackboard.dieAnimationEnded)
        {
            mosquitoBlackboard.mosquito.SpawnVoxelsAndReturnToPool();
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
