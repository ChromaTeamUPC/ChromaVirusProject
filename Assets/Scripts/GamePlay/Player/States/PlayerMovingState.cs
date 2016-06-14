using UnityEngine;
using System.Collections;

public class PlayerMovingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
        blackboard.animator.SetBool("Walking", true);
    }

    public override void OnStateExit()
    {
        blackboard.animator.SetBool("Walking", false);
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
        else if (SpecialPressed())
        {
            return blackboard.specialState;
        }
        else if (DashPressed())
        {
            return blackboard.dashingState;
            //return blackboard.speedBumpState;
        }
        else
        {
            Turn();

            Shoot();

            if(!Move())
            {
                return blackboard.idleState;
            }

            return null;
        }
    }
}
