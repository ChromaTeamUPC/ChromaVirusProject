using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        EndColorMismatch(); //Ensure it is not active

        blackboard.animationEnded = false;
        blackboard.animator.SetTrigger("Die");

        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DYING, PlayerEventInfo.eventInfo);
    }

    public override void OnStateExit()
    {
        blackboard.currentSpeed = blackboard.player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        blackboard.currentSpeed *= 0.95f;

        if (blackboard.animationEnded)
        {
            blackboard.blinkController.StopPreviousBlinkings();
            blackboard.player.SpawnVoxels();
            PlayerEventInfo.eventInfo.player = blackboard.player;
            rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DIED, PlayerEventInfo.eventInfo);
            blackboard.player.gameObject.SetActive(false);
            return null;
        }
        else
            return null;  
    }

    public override PlayerBaseState TakeDamage(int damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }
}
