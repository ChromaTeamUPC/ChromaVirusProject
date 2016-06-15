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
        //blackboard.fastMovementCharge -= blackboard.player.dashCost;
        SetDashDirection();
        currentDashTime = 0f;
        blackboard.currentSpeed = blackboard.player.initialDashSpeed;
        blackboard.player.SpawnDashParticles();
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
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
        if(blackboard.movePressed)
        {
            blackboard.moveVector = GetScreenRelativeDirection(blackboard.moveVector);
            blackboard.horizontalDirection = blackboard.moveVector;
        }
        else
        {
            blackboard.horizontalDirection = blackboard.player.transform.TransformDirection(Vector3.forward);
        }

        dashPSRotator.rotation = Quaternion.LookRotation(blackboard.horizontalDirection);

    }
}
