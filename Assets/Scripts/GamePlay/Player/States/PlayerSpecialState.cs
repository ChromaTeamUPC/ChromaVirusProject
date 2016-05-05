using UnityEngine;
using System.Collections;

public class PlayerSpecialState : PlayerBaseState {

    public override void OnStateEnter()
    {
        //play special animation
        player.canTakeDamage = false;
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
    }


    public override PlayerBaseState Update()
    {
        //if special done, return idle state
        //else return null
        return player.idleState;
    }
}
