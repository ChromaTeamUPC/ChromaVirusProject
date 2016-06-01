using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        player.EndColorMismatch(); //Ensure it is not active

        player.animationEnded = false;      
        player.canTakeDamage = false;
        player.animator.SetTrigger("Die");
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
        player.currentSpeed = player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        player.currentSpeed *= 0.95f;

        if (player.animationEnded)
        {
            player.SpawnVoxels();
            PlayerDiedEventInfo.eventInfo.player = player;
            rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DIED, PlayerDiedEventInfo.eventInfo);
            player.gameObject.SetActive(false);
            return null;
        }
        else
            return null;  
    }
}
