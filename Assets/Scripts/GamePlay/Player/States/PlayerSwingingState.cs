using UnityEngine;
using System.Collections;

public class PlayerSwingingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        //play swinging animation
        bb.horizontalDirection = Vector3.zero;
    }

    public override PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        is he in a border?
        can he do a special?
        can he do a dash?
        can he turn?
        can he shoot?
        can he move?
        */

        if (!bb.isGrounded)
        {
            return bb.fallingState;
        }
        else if (!bb.isInBorder)
        {
            return bb.idleState;
        }
        else
        {
            Turn();
            Move();

            return null;
        }
    }
}
