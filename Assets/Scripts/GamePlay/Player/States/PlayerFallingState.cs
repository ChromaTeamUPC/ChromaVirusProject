using UnityEngine;
using System.Collections;

public class PlayerFallingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        //play falling animation
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
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

        if (blackboard.isGrounded)
        {
            return blackboard.idleState;
        }
        else
        {
            //keep falling
            blackboard.currentSpeed *= 0.95f;

            return null;
        }      
    }
}
