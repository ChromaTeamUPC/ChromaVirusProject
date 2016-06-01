﻿using UnityEngine;
using System.Collections;

public class PlayerFallingState : PlayerBaseState
{
    private float yOrigin;
    private bool firstFrame;

    public override void OnStateEnter()
    {
        //play falling animation
        yOrigin = player.transform.position.y;
    }

    public override void OnStateExit()
    {
        player.currentSpeed = player.walkSpeed;
    }

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

        if(player.isGrounded)
        {
            float yDestiny = player.transform.position.y;
            float fallingDistance = yOrigin - yDestiny;

            if (fallingDistance >= player.damageEveryXUnits)
            {
                float totalDamage = player.fallDamage * fallingDistance / player.damageEveryXUnits;
                player.TakeDamage((int)totalDamage);               
            }

            return player.idleState;
        }

        //keep falling
        player.currentSpeed *= 0.95f;

        return null;       
    }
}
