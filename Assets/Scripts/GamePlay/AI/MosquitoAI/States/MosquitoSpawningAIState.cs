using UnityEngine;
using System.Collections;

public class MosquitoSpawningAIState : MosquitoAIBaseState {

    public MosquitoSpawningAIState(MosquitoBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        mosquitoBlackboard.mosquito.projector.enabled = false;
        mosquitoBlackboard.agent.enabled = false;
        mosquitoBlackboard.entity.mainCollider.enabled = false;
        mosquitoBlackboard.entity.dyingCollider.SetActive(false);
        mosquitoBlackboard.canReceiveDamage = false;
        mosquitoBlackboard.spawnAnimationEnded = false;

        mosquitoBlackboard.animator.Rebind(); //Restart state machine
        //mosquitoBlackboard.animator.SetInteger("spawnAnimation", (int)mosquitoBlackboard.spawnAnimation);

        ColorEventInfo.eventInfo.newColor = mosquitoBlackboard.mosquito.color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.ENEMY_SPAWNED, ColorEventInfo.eventInfo);
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        mosquitoBlackboard.mosquito.projector.enabled = true;
        mosquitoBlackboard.agent.enabled = true;
        mosquitoBlackboard.entity.mainCollider.enabled = true;
        mosquitoBlackboard.canReceiveDamage = true;

        if (rsc.colorMng.CurrentColor != blackboard.entity.color)
            blackboard.entity.shields[(int)blackboard.entity.color].SetActive(true);

        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (!mosquitoBlackboard.spawnAnimationEnded)
            return null;
        else
        {
            return mosquitoBlackboard.patrolingState;
        }
    }

    public override void ColorChanged(ChromaColor newColor)
    {
        //Do nothing
    }
}
