using UnityEngine;
using System.Collections;

public class PlayerReceivingDamageState : PlayerBaseState {

    public override void OnStateEnter()
    {
        player.animator.SetTrigger("Hit");
        animationEnded = false;
        player.canTakeDamage = false;
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
    }

    public override PlayerBaseState Update()
    {
        if(animationEnded)
        {
            return player.idleState;
        }
        return null;
    }
}
