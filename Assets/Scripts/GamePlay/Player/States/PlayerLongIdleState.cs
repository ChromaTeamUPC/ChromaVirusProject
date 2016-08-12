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
        if (bb.KeyPressed)
        {
            bb.animator.SetTrigger("KeyPressed");
        }

        if (CanDoSpecial())
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
        else if (bb.movePressed)
        {
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
