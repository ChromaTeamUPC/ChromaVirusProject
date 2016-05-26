using UnityEngine;
using System.Collections;

public class PlayerMovingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        player.currentSpeed = player.speed;
        //play moving animation
        player.animator.SetBool("Walking", true);
    }

    public override void OnStateExit()
    {
        //Stop trail
        player.animator.SetBool("Walking", false);
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

        if (!player.isGrounded)
        {
            return player.fallingState;
        }
        else if (player.SpecialPressed())
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

            if(!player.Move())
            {
                return player.idleState;
            }

            return null;
        }
    }
}
