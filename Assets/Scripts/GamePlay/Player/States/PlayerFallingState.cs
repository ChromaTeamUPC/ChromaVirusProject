using UnityEngine;
using System.Collections;

public class PlayerFallingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        //play falling animation
        bb.player.StopTrail();
        bb.player.DisableUI();
        bb.animator.SetBool("Falling", true);

        bb.falling = true;
    }

    public override void OnStateExit()
    {
        bb.currentSpeed = bb.player.walkSpeed;
        bb.player.StartTrail();
        bb.player.EnableUI();
        bb.animator.SetBool("Falling", false);
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

        if (bb.isGrounded)
        {
            return bb.idleState;
        }
        else
        {
            //keep falling
            bb.currentSpeed *= 0.95f;

            return null;
        }      
    }
}
