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
        if(blackboard.keyPressed)
            blackboard.animator.SetTrigger("KeyPressed");
    }

    public override PlayerBaseState Update()
    {
        if (SpecialPressed())
        {
            return blackboard.specialState;
        }
        else if (DashPressed())
        {
            //return blackboard.dashingState;
            return blackboard.speedBumpState;
        }
        else
        {            
            Turn();

            Shoot();

            if (Move())
            {
                return blackboard.movingState;
            }

            if (blackboard.keyPressed)
            {            
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
