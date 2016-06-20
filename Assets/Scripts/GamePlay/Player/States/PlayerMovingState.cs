using UnityEngine;
using System.Collections;

public class PlayerMovingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
        Move();
    }

    public override void OnStateExit()
    {
    }


    public override PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        can he do a special?
        can he do a dash?
        can he turn?
        can he shoot?
        can he move?
        */

        if (!blackboard.isGrounded)
        {
            return blackboard.fallingState;
        }
        else if (CanDoSpecial())
        {
            return blackboard.specialState;
        }
        else if (blackboard.dashPressed)
        {
            return blackboard.dashingState;
            //return blackboard.speedBumpState;
        }
        else if (!blackboard.movePressed)
        {
            return blackboard.idleState;
        }
        else
        {
            CapacitorCharge();

            DisinfectDevice();

            Turn();

            Shoot();

            Move();           

            return null;
        }
    }
}
