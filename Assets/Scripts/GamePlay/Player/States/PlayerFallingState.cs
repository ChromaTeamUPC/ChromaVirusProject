using UnityEngine;
using System.Collections;

public class PlayerFallingState : PlayerBaseState
{
    int floorLayer;

    public PlayerFallingState(): base()
    {
        floorLayer = LayerMask.GetMask("Stage") | LayerMask.GetMask("Hexagon");
    }

    public override void OnStateEnter()
    {
        //play falling animation
        bb.player.StopTrail();
        bb.player.DisableUI();
        bb.animator.SetBool("Falling", true);
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

        if (Physics.Raycast(bb.player.transform.position, Vector3.down, 10, floorLayer))
        {
            bb.falling = false;
        }
        else
        {
            bb.falling = true;
        }

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
