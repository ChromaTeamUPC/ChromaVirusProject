using UnityEngine;
using System.Collections;

public class PlayerSpawningState : PlayerBaseState {

    public override void OnStateEnter()
    {
        Debug.Log("Spawning state");
        //trigger spawning animation
        player.canTakeDamage = false;
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
    }

    public override PlayerBaseState Update()
    {
        /*if spawning finished return idle
        else return null*/
        //return player.idleState;

        return player.idleState;
    }
}
