using UnityEngine;
using System.Collections;

public class PlayerReceivingDamageState : PlayerBaseState {
    
    public override void OnStateEnter()
    {
        bb.animationEnded = false;
        bb.animator.SetTrigger("Hit");

        rsc.rumbleMng.Rumble(bb.player.Id, 0.3f, 0.4f, 0.2f, 0.3f);
    }

    public override void OnStateExit()
    {
        bb.currentSpeed = bb.player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        bb.currentSpeed *= 0.95f;
        

        if(bb.animationEnded)
        {
            return bb.idleState;
        }
        return null;
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take more damage during this state
        return null;
    }
}
