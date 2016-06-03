using UnityEngine;
using System.Collections;

public class PlayerIdleState : PlayerBaseState {

    private float elapsedTime;

    public override void OnStateEnter()
    {
        elapsedTime = 0f;
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

        if (!blackboard.isGrounded)
        {
            return blackboard.fallingState;
        }
        else if(blackboard.isInBorder)
        {
            return blackboard.swingingState;
        }
        else if (SpecialPressed())
        {
            return blackboard.specialState;
        }
        else if (DashPressed())
        {
            return blackboard.dashingState;
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
                elapsedTime = 0f;
            }
            else
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > blackboard.player.idleRandomAnimTime)
                    return blackboard.longIdleState;
            }  
        
            return null;

        }        
    }
}
