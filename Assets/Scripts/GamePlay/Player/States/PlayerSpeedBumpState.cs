using UnityEngine;
using System.Collections;

public class PlayerSpeedBumpState : PlayerBaseState
{
    private float elapsedTime;
    private float speedReductionOn;

    public override void OnStateEnter()
    {
        elapsedTime = 0f;

        blackboard.currentSpeed = blackboard.player.fastMovingSpeed;

        speedReductionOn = blackboard.player.fastMovingMaxSeconds * blackboard.player.fastMovingSpeedReductionOn;
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
                
        //if(!Input.GetButton(blackboard.dash))
        if(!blackboard.controller.LeftTrigger.IsPressed)
        {
            return ReturnIdleOrMoving();
        }
        else
        {
            //blackboard.fastMovementCharge -= Time.deltaTime * blackboard.player.fastMovingCostXSecond;

            /*if(blackboard.fastMovementCharge <= 0f)
            {
                blackboard.fastMovementCharge = 0f;
                return ReturnIdleOrMoving();
            }*/

            if(elapsedTime >= blackboard.player.fastMovingMaxSeconds)
            {
                return ReturnIdleOrMoving();
            }
            else if (elapsedTime >= speedReductionOn)
            {
                blackboard.currentSpeed = Mathf.Lerp(blackboard.player.walkSpeed, blackboard.player.fastMovingSpeed, 1-((elapsedTime - speedReductionOn) / (blackboard.player.fastMovingMaxSeconds - speedReductionOn)));
                Debug.Log(blackboard.currentSpeed);
            }

            if (!blackboard.isGrounded)
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
