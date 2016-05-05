using UnityEngine;
using System.Collections;

public class PlayerIdleState : PlayerBaseState {

    private float elapsedTime;

    public override void OnStateEnter()
    {
        Debug.Log("Idle state");
        elapsedTime = 0f;
        //play idle state
    }

    public override void OnStateExit()
    { }


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

        if (!player.ctrl.isGrounded)
            return player.fallingState;

        if(player.isInBorder)
        {
            return player.swingingState;
        }

        bool keyPressed = false;

        if (player.SpecialPressed())
        {
            return player.specialState;
        }
        else if (player.DashPressed())
        {
            return player.dashingState;
        }
        else
        {
            if (player.Turn())
                keyPressed = true;

            if (player.Shoot())
                keyPressed = true;

            if (player.Move())
                return player.movingState;
        }

        if(keyPressed)
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
