using UnityEngine;
using System.Collections;

public class PlayerIdleState : PlayerBaseState {

    private float elapsedTime;

    public override void OnStateEnter()
    {
        bb.falling = false;
        bb.horizontalDirection = Vector3.zero;
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
                elapsedTime = 0f;
            }
            else
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > bb.player.idleRandomAnimTime)
                    return bb.longIdleState;
            }  
        
            return null;

        }        
    }
}
