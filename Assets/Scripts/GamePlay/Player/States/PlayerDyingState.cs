using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        //Play dying animation? spawn voxels?
        player.canTakeDamage = false;

        player.voxelization.CalculateVoxelsGrid();
        //player.voxelization.SpawnVoxels();
        PlayerDiedEventInfo.eventInfo.player = player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DIED, PlayerDiedEventInfo.eventInfo);
        player.gameObject.SetActive(false);
    }
}
