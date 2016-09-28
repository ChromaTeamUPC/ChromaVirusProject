using UnityEngine;
using System.Collections;

public class PlayerSpeedBumpState : PlayerBaseState
{
    private float elapsedTime;
    private float speedReductionOn;

    public override void OnStateEnter()
    {
        elapsedTime = 0f;

        bb.currentSpeed = bb.player.fastMovingSpeed;

        speedReductionOn = bb.player.fastMovingMaxSeconds * bb.player.fastMovingSpeedReductionOn;
        bb.player.StartSpeedUp();
    }

    public override void OnStateExit()
    {
        bb.player.StopSpeedUp();
        bb.currentSpeed = bb.player.walkSpeed;
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
        if(!bb.controller.LeftBumper.IsPressed)
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

            if(elapsedTime >= bb.player.fastMovingMaxSeconds)
            {
                return ReturnIdleOrMoving();
            }
            else if (elapsedTime >= speedReductionOn)
            {
                bb.currentSpeed = Mathf.Lerp(bb.player.walkSpeed, bb.player.fastMovingSpeed, 1-((elapsedTime - speedReductionOn) / (bb.player.fastMovingMaxSeconds - speedReductionOn)));
            }

            if (ShouldFall())
            {
                return bb.fallingState;
            }
            else if (CanDoSpecial())
            {
                return bb.specialState;
            }
            else
            {
                Turn();

                Shoot();

                if (bb.movePressed)
                {
                    Move();
                }
                else
                {
                    return bb.idleState;
                }

                return null;
            }
        }
    }

    private PlayerBaseState ReturnIdleOrMoving()
    {
        if (bb.movePressed)
            return bb.movingState;
        else
            return bb.idleState;
    }
}
