using UnityEngine;
using System.Collections;

public class PlayerSpeedBumpState : PlayerBaseState
{
    private float elapsedTime;

    public override void OnStateEnter()
    {
        elapsedTime = 0f;

        blackboard.currentSpeed = blackboard.player.fastMovingSpeed;
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        /*If stop pressing button
            if elapsed time < dash threshold
                dash
            else if moving
                moving
            else
                idle
        */
        elapsedTime += Time.deltaTime;
                
        if(!Input.GetButton(blackboard.dash))
        {
            if (elapsedTime < blackboard.player.dashDetectionThreshold && blackboard.fastMovementCharge >= blackboard.player.dashCost)
                return blackboard.dashingState;
            else
                return ReturnIdleOrMoving();
        }
        else
        {
            blackboard.fastMovementCharge -= Time.deltaTime * blackboard.player.fastMovingCostXSecond;

            if(blackboard.fastMovementCharge <= 0f)
            {
                blackboard.fastMovementCharge = 0f;
                return ReturnIdleOrMoving();
            }

            else if (!blackboard.isGrounded)
            {
                return blackboard.fallingState;
            }
            else if (CanDoSpecial())
            {
                return blackboard.specialState;
            }
            else
            {
                Turn();

                Shoot();

                if (blackboard.movePressed)
                {
                    Move();
                }
                else
                {
                    return blackboard.idleState;
                }

                return null;
            }
        }
    }

    private PlayerBaseState ReturnIdleOrMoving()
    {
        if (blackboard.movePressed)
            return blackboard.movingState;
        else
            return blackboard.idleState;
    }
}
