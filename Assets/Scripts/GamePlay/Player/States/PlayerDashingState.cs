using UnityEngine;
using System.Collections;

public class PlayerDashingState : PlayerBaseState
{
    private Vector3 dashDirection;
    private float currentDashSpeed;
    private float currentDashTime;


    public override void OnStateEnter()
    {
        player.SetDashDirection();
        currentDashTime = 0f;
        player.currentSpeed = player.initialDashSpeed;
        player.SpawnDashParticles();

        player.animator.SetBool("Walking", true);
    }

    public override void OnStateExit()
    {
        player.currentSpeed = player.speed;
        if(!player.GetHorizontalDirectionFromInput())
            player.animator.SetBool("Walking", false);
    }

    public override PlayerBaseState Update()
    {      
        if(currentDashTime > player.maxDashTime || player.currentSpeed < player.minDashSpeed)
        {
            if (player.isGrounded)
                return player.idleState;
            else
                return player.fallingState;
        }

        currentDashTime += Time.fixedDeltaTime;
        player.currentSpeed -= player.dashDeceleration * Time.fixedDeltaTime;

        player.Turn();

        player.Shoot();

        return null;
    }
}
