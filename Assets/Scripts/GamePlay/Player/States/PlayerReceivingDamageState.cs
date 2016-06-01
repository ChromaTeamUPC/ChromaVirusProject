using UnityEngine;
using System.Collections;

public class PlayerReceivingDamageState : PlayerBaseState {

    private const float maxDuration = 0.5f; //to avoid bug where player is stuck in this state
    private float duration;

    public override void OnStateEnter()
    {       
        player.animationEnded = false;
        player.canTakeDamage = false;
        player.animator.SetTrigger("Hit");
        duration = 0f;
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
        player.currentSpeed = player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        player.currentSpeed *= 0.95f;

        duration += Time.deltaTime;
        if(player.animationEnded || duration >= maxDuration)
        {
            return player.idleState;
        }
        return null;
    }
}
