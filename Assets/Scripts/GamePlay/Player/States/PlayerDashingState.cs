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
        dashPSRotator = base.bb.player.transform.Find("ParticleSystems/DashPSRotation");
    }

    public override void OnStateEnter()
    {
        //blackboard.fastMovementCharge -= blackboard.player.dashCost;
        SetDashDirection();
        currentDashTime = 0f;
        bb.currentSpeed = bb.player.initialDashSpeed;
        bb.player.SpawnDashParticles();

        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DASHING, EventInfo.emptyInfo);
        rsc.rumbleMng.AddContinousRumble(RumbleId.PLAYER_DASH, bb.player.Id, 1f, 0f);
        rsc.camerasMng.AddContinousEffect(EffectId.PLAYER_DASH, bb.player.Id, 0f, Effects.MOTION_BLUR);
    }

    public override void OnStateExit()
    {
        bb.currentSpeed = bb.player.walkSpeed;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DASHED, EventInfo.emptyInfo);
        rsc.rumbleMng.RemoveContinousRumble(RumbleId.PLAYER_DASH, bb.player.Id);
        rsc.camerasMng.RemoveContinousEffect(EffectId.PLAYER_DASH, bb.player.Id);
    }

    public override PlayerBaseState Update()
    {      
        if(currentDashTime > bb.player.maxDashSeconds || bb.currentSpeed < bb.player.minDashSpeed)
        {
            if (bb.isGrounded)
                return bb.idleState;
            else
                return bb.fallingState;
        }

        currentDashTime += Time.deltaTime;
        bb.currentSpeed -= bb.player.dashDeceleration * Time.deltaTime;

        Turn();

        //Shoot();

        return null;
    }

    private void SetDashDirection()
    { 
        if(bb.movePressed)
        {
            bb.moveVector = bb.GetScreenRelativeDirection(bb.moveVector);
            bb.horizontalDirection = bb.moveVector;
        }
        else
        {
            bb.horizontalDirection = bb.player.transform.TransformDirection(Vector3.forward);
        }

        dashPSRotator.rotation = Quaternion.LookRotation(bb.horizontalDirection);

    }
}
