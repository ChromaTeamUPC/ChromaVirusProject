using UnityEngine;
using System.Collections;

public class PlayerMovingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        bb.currentSpeed = bb.player.walkSpeed;
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

        if (ShouldFall())
        {
            return bb.fallingState;
        }
        else if (CanDoSpecial())
        {
            return bb.specialState;
        }
        else if (bb.dashPressed)
        {
            return bb.dashingState;
            //return blackboard.speedBumpState;
        }
        else if (bb.speedBumpPressed)
        {
            return bb.speedBumpState;
        }
        else if (!bb.movePressed)
        {
            return bb.idleState;
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
