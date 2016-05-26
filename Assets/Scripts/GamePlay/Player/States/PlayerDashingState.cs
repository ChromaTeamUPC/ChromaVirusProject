using UnityEngine;
using System.Collections;

public class PlayerDashingState : PlayerBaseState
{
    private Vector3 dashDirection;
    private float currentDashSpeed;
    private float currentDashTime;


    public override void OnStateEnter()
    {
        //play dash animation
        player.SetDashDirection();
        //GetDashDirection();
        currentDashTime = 0f;
        //currentDashSpeed = player.initialDashSpeed;
        player.currentSpeed = player.initialDashSpeed;
    }

    public override void OnStateExit()
    {
        player.currentSpeed = player.speed;
    }

    public override PlayerBaseState Update()
    {
        /*if (currentDashTime > player.maxDashTime || currentDashSpeed < player.minDashSpeed)
        {
            player.ctrl.SimpleMove(Vector3.zero); //Force calculation of isGrounded
            if (player.ctrl.isGrounded)
                return player.idleState;
            else
                return player.fallingState;
        }

        currentDashTime += Time.deltaTime;

        player.ctrl.Move(dashDirection * currentDashSpeed * Time.deltaTime);
        currentDashSpeed -= player.dashDeceleration * Time.deltaTime;

        player.Turn();
        player.Shoot();

        return null;*/
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

    /*private void GetDashDirection()
    {

        Vector3 direction = player.GetMovingVector();

        if (direction != Vector3.zero)
        {
            //If moving, dash is in moving direction
            direction.Normalize();
            dashDirection = player.GetScreenRelativeDirection(direction);
        }
        else
        {
            //else dash is in forward direction
            dashDirection = player.transform.TransformDirection(Vector3.forward);
        }
    }*/
}
