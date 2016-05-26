using UnityEngine;
using System.Collections;

public class PlayerLongIdleState : PlayerBaseState
{

    public override void OnStateEnter()
    {
        //select random idle animation
        player.animator.SetTrigger("LongIdle");
        player.animationEnded = false;
    }

    public override void OnStateExit()
    {
        if(player.keyPressed)
            player.animator.SetTrigger("KeyPressed");
    }

    public override PlayerBaseState Update()
    {
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
            /*bool keyPressed = false;

            if (player.Turn())
                keyPressed = true;

            if (player.Shoot())
                keyPressed = true;

            if (player.UpdateHorizontalDirectionFromInput())
                return player.movingState;
            //if (player.Move())
            //   return player.movingState;

            if (keyPressed)
            {
                player.animator.SetTrigger("KeyPressed");
                return player.idleState;
            }*/

            player.Turn();

            player.Shoot();

            if (player.Move())
            {
                return player.movingState;
            }

            if (player.keyPressed)
            {            
                return player.idleState;
            }
        }

        if (player.animationEnded)
        {
            return player.idleState;
        }
        
        return null;
    }
}
