using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        player.EndColorMismatch(); //Ensure it is not active

        player.canTakeDamage = false;
        player.animator.SetTrigger("Die");
        player.animationEnded = false;      
    }

    public override void OnStateExit()
    {
        player.canTakeDamage = true;
    }

    public override PlayerBaseState Update()
    {
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
