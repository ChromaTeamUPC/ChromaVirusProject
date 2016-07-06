using UnityEngine;
using System.Collections;

public class PlayerIdleState : PlayerBaseState {

    private float elapsedTime;

    public override void OnStateEnter()
    {
        blackboard.horizontalDirection = Vector3.zero;
        elapsedTime = 0f;
    }

    public override PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        is he in a border?
        can he do a special?
        can he do a dash?
        can he charge a capacitor?
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
        else if (CanDoSpecial())
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
