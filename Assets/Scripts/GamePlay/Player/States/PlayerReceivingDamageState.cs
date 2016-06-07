using UnityEngine;
using System.Collections;

public class PlayerReceivingDamageState : PlayerBaseState {

    //private const float maxDuration = 0.5f; //to avoid bug where player is stuck in this state
    //private float duration;

    public override void OnStateEnter()
    {
        blackboard.animationEnded = false;
        blackboard.animator.SetTrigger("Hit");
        //duration = 0f;
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        blackboard.currentSpeed *= 0.95f;

        //duration += Time.deltaTime;
        if(blackboard.animationEnded /*|| duration >= maxDuration*/)
        {
            return blackboard.idleState;
        }
        return null;
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take more damage during this state
        return null;
    }
}
