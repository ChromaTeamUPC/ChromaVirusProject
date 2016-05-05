using UnityEngine;
using System.Collections;

public class PlayerLongIdleState : PlayerBaseState {

    public override void OnStateEnter()
    {
        Debug.Log("Long Idle state");
        //select random idle animation
        //play animation
    }

    public override PlayerBaseState Update()
    {
        /*bool keyPressed = false;

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

        if (keyPressed  or animation done)
        {
            return player.idleState;
        }
        else
        {
            return null;
        }*/

        return player.idleState;
    }
}
