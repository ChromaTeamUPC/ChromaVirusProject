using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        player.canTakeDamage = false;
        player.animator.SetTrigger("Die");
        animationEnded = false;      
    }

    public override PlayerBaseState Update()
    {
        if (animationEnded)
        {
            player.voxelization.CalculateVoxelsGrid();
            player.voxelization.SpawnVoxels();
            PlayerDiedEventInfo.eventInfo.player = player;
            rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DIED, PlayerDiedEventInfo.eventInfo);
            player.gameObject.SetActive(false);
            return null;
        }
        else
            return null;  
    }
}
