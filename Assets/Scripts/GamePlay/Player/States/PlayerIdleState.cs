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

        if (!player.isGrounded)
        {
            return player.fallingState;
        }
        else if(player.isInBorder)
        {
            return player.swingingState;
        }
        else if (player.SpecialPressed())
        {
            return player.specialState;
        }
        else if (player.DashPressed())
        {
            return player.dashingState;
        }
        else
        {
            player.Turn();

            player.Shoot();

            if (player.Move())
            {
                return player.movingState;
            }

            if (player.keyPressed)
            {
                elapsedTime = 0f;
            }
            else
            {
                elapsedTime += Time.fixedDeltaTime;
                if (elapsedTime > player.idleRandomAnimTime)
                    return player.longIdleState;
            }  
        
            return null;

        }

        
    }
}
