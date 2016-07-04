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

        EnemyDiedEventInfo.eventInfo.color = color;
        EnemyDiedEventInfo.eventInfo.infectionValue = MosquitoAIBehaviour.infectionValue;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_DIED, EnemyDiedEventInfo.eventInfo);

        if (mosquitoBlackboard.lastShotSameColor)
        {
            /*mosquitoBlackboard.entity.mainCollider.enabled = false;
            mosquitoBlackboard.entity.dyingCollider.SetActive(true);
            mosquitoBlackboard.canReceiveDamage = false;
            mosquitoBlackboard.dieAnimationEnded = false;
            mosquitoBlackboard.animator.SetTrigger("die");

            movement = mosquitoBlackboard.lastShotDirection * mosquitoBlackboard.entity.shotForceModifier;*/

            GameObject explosion = mosquitoBlackboard.explosions[(int)color];
            explosion.SetActive(true);

            mosquitoBlackboard.mosquito.SpawnVoxelsAndReturnToPool(true);
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
        /*mosquitoBlackboard.mosquito.transform.position += movement * Time.deltaTime;

        if (mosquitoBlackboard.dieAnimationEnded)
        {
            mosquitoBlackboard.mosquito.SpawnVoxelsAndReturnToPool();
        }*/

        return null;
    }

    public override void ColorChanged(ChromaColor newColor)
    {
        //Do nothing
    }
}
