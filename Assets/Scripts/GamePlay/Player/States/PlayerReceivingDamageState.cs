using UnityEngine;
using System.Collections;

public class PlayerReceivingDamageState : PlayerBaseState {

    public override void OnStateEnter()
    {
        player.animator.SetTrigger("Hit");
        player.animationEnded = false;
        player.canTakeDamage = false;
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
    }

    public override PlayerBaseState Update()
    {
        if(player.animationEnded)
        {
            return player.idleState;
        }
        return null;
    }
}
