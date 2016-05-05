using UnityEngine;
using System.Collections;

public class PlayerFallingState : PlayerBaseState
{
    private float yOrigin;
    private bool firstFrame;

    public override void OnStateEnter()
    {
        Debug.Log("Falling state");
        //play falling animation
        yOrigin = player.transform.position.y;
        firstFrame = true;
    }

    public override void OnStateExit() { }


    public override PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        can he do a special?
        can he do a dash?
        can he turn?
        can he shoot?
        can he move?
        */

        if (firstFrame)
        {
            player.ctrl.Move(Vector3.zero); //To stop inertia
            firstFrame = false;
        }
        else
        {
            if (player.ctrl.SimpleMove(Vector3.zero))
            {
                float yDestiny = player.transform.position.y;
                float fallingDistance = yOrigin - yDestiny;

                if (fallingDistance < player.damageEveryXUnits)
                    return player.idleState;
                else
                {
                    float totalDamage = player.fallDamage * fallingDistance / player.damageEveryXUnits;
                    player.TakeDamage((int)totalDamage);
                    return player.idleState;
                }
            }
        }

        //keep falling
        return null;
    }
}
