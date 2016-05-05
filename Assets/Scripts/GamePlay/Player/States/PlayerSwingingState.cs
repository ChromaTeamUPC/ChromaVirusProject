using UnityEngine;
using System.Collections;

public class PlayerSwingingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        Debug.Log("Swinging State");
        //play swinging animation
    }

    public override void OnStateExit() { }


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
        {
            return player.fallingState;
        }

        if (!player.isInBorder)
        {
            return player.idleState;
        }

        player.Turn();
        player.Move();

        return null;
    }
}
