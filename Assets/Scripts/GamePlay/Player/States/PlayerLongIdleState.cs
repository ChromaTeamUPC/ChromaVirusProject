using UnityEngine;
using System.Collections;

public class PlayerLongIdleState : PlayerBaseState
{

    public override void OnStateEnter()
    {
        //select random idle animation
        blackboard.animator.SetTrigger("LongIdle");
        blackboard.animationEnded = false;
    }

    public override void OnStateExit()
    {
    }

    public override PlayerBaseState Update()
    {
        if (CanDoSpecial())
        {
            return blackboard.specialState;
        }
        else if (blackboard.dashPressed)
        {
            return blackboard.dashingState;
            //return blackboard.speedBumpState;
        }
        else if (blackboard.speedBumpPressed)
        {
            return blackboard.speedBumpState;
        }
        else if (blackboard.movePressed)
        {
            return blackboard.movingState;
        }
        else
        {
            CapacitorCharge();

            DisinfectDevice();

            Turn();

            Shoot();

            if (blackboard.KeyPressed)
            {
                blackboard.animator.SetTrigger("KeyPressed");
                return blackboard.idleState;
            }
        }

        if (blackboard.animationEnded)
        {
            return blackboard.idleState;
        }
        
        return null;
    }
}
