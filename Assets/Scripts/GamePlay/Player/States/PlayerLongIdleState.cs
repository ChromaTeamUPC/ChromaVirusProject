using UnityEngine;
using System.Collections;

public class PlayerLongIdleState : PlayerBaseState
{

    public override void OnStateEnter()
    {
        //select random idle animation
        bb.animator.SetTrigger("LongIdle");
        bb.animationEnded = false;
    }

    public override void OnStateExit()
    {
    }

    public override PlayerBaseState Update()
    {
        /*if (bb.KeyPressed)
        {
            bb.animator.SetTrigger("KeyPressed");
        }*/

        if (CanDoSpecial())
        {
            return bb.specialState;
        }
        else if (bb.dashPressed)
        {
            if (!bb.player.fastMoving)
                return bb.dashingState;
            else
                return bb.speedBumpState;
        }
        else if (bb.movePressed)
        {
            if (bb.player.fastMoving && bb.dashWasPressed)
                return bb.speedBumpState;
            else
                return bb.movingState;
        }
        else
        {
            CapacitorCharge();

            DisinfectDevice();

            Turn();

            Shoot();

            if (bb.KeyPressed)
            {
                return bb.idleState;
            }
        }

        if (bb.animationEnded)
        {
            return bb.idleState;
        }
        
        return null;
    }
}
