using UnityEngine;
using System.Collections;

public class PlayerReceivingDamageState : PlayerBaseState {

    public override void OnStateEnter()
    {
        //play damaged animation
        //calculate destination position
        player.canTakeDamage = false;
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
    }

    public override PlayerBaseState Update()
    {
        //if animation finished  and position reached return to idle
        //else return null
        return player.idleState;
    }
}
