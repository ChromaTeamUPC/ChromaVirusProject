using UnityEngine;
using System.Collections;

public class PlayerLongIdleState : PlayerBaseState {

    private int idleHash, idle2Hash;
    private int loopedTimes;

    public override void OnStateEnter()
    {
        Debug.Log("Long Idle state");

        //select random idle animation
        player.animator.SetTrigger("LongIdle");
        animationEnded = false;
    }

    public override PlayerBaseState Update()
    {
        
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

        if (keyPressed)
        {
            player.animator.SetTrigger("KeyPressed");
            return player.idleState;
        }
        else if (animationEnded)
        {
            return player.idleState;
        }
        else
        {
            return null;
        }
    }
}
