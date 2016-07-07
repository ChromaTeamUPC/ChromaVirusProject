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

        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DASHING, EventInfo.emptyInfo);
        rsc.rumbleMng.AddContinousRumble(RumbleType.PLAYER_DASH, blackboard.player.Id, 1f, 0f);
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DASHED, EventInfo.emptyInfo);
        rsc.rumbleMng.RemoveContinousRumble(RumbleType.PLAYER_DASH);
    }

    public override PlayerBaseState Update()
    {      
        if(currentDashTime > blackboard.player.maxDashSeconds || blackboard.currentSpeed < blackboard.player.minDashSpeed)
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
            blackboard.moveVector = blackboard.GetScreenRelativeDirection(blackboard.moveVector);
            blackboard.horizontalDirection = blackboard.moveVector;
        }
        else
        {
            blackboard.horizontalDirection = blackboard.player.transform.TransformDirection(Vector3.forward);
        }

        dashPSRotator.rotation = Quaternion.LookRotation(blackboard.horizontalDirection);

    }
}
