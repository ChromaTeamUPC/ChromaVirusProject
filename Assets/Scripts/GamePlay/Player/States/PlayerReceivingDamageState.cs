using UnityEngine;
using System.Collections;

public class PlayerReceivingDamageState : PlayerBaseState {
    
    public override void OnStateEnter()
    {
        blackboard.animationEnded = false;
        blackboard.animator.SetTrigger("Hit");
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        blackboard.currentSpeed *= 0.95f;

        if(blackboard.animationEnded)
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
