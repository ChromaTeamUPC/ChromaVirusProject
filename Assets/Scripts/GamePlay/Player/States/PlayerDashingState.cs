using UnityEngine;
using System.Collections;

public class PlayerDashingState : PlayerBaseState
{
    private Vector3 dashDirection;
    private Transform dashPSRotator;

    private float currentDashSpeed;
    private float currentDashTime;

    public override void Init(PlayerBlackboard bb)
    {
        base.Init(bb);
        dashPSRotator = blackboard.player.transform.Find("ParticleSystems/DashPSRotation");
    }

    public override void OnStateEnter()
    {
        SetDashDirection();
        currentDashTime = 0f;
        blackboard.currentSpeed = blackboard.player.initialDashSpeed;
        blackboard.player.SpawnDashParticles();

        blackboard.animator.SetBool("Walking", true);
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
        if(!GetHorizontalDirectionFromInput())
            blackboard.animator.SetBool("Walking", false);
    }

    public override PlayerBaseState Update()
    {      
        if(currentDashTime > blackboard.player.maxDashTime || blackboard.currentSpeed < blackboard.player.minDashSpeed)
        {
            if (blackboard.isGrounded)
                return blackboard.idleState;
            else
                return blackboard.fallingState;
        }

        currentDashTime += Time.deltaTime;
        blackboard.currentSpeed -= blackboard.player.dashDeceleration * Time.deltaTime;

        Turn();

        //Shoot();

        return null;
    }

    private void SetDashDirection()
    {
        GetHorizontalDirectionFromInput();

        if (blackboard.horizontalDirection == Vector3.zero)
        {
            blackboard.horizontalDirection = blackboard.player.transform.TransformDirection(Vector3.forward);
        }

        dashPSRotator.rotation = Quaternion.LookRotation(blackboard.horizontalDirection);

    }
}
