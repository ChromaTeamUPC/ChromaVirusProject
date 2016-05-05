using UnityEngine;
using System.Collections;

public class PlayerMovingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        Debug.Log("Moving state");
        //play moving animation
        //start trail
    }

    public override void OnStateExit()
    {
        //Stop trail
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

        if (!player.ctrl.isGrounded)
            return player.fallingState;

        if (player.SpecialPressed())
        {
            return player.specialState;
        }
        else if (player.DashPressed())
        {
            return player.dashingState;
        }
        else
        {
            player.Turn();
            player.Shoot();

            if (player.Move())
                return null;
            else
                return player.idleState;
        }
    }
}
