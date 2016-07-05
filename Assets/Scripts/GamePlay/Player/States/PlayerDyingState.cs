using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        EndColorMismatch(); //Ensure it is not active

        blackboard.player.StopTrail();
        blackboard.shield.SetActive(false);
        blackboard.animator.SetTrigger("Die");
        blackboard.animationEnded = false;

        rsc.rumbleMng.AddContinousRumble(3, blackboard.player.Id, 0.25f, 0f);

        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DYING, PlayerEventInfo.eventInfo);
    }

    public override void OnStateExit()
    {
        //blackboard.shield.SetActive(true);
        //blackboard.player.StartTrail();
        //blackboard.currentSpeed = blackboard.player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        blackboard.currentSpeed *= 0.95f;

        if (blackboard.animationEnded)
        {
            rsc.rumbleMng.RemoveContinousRumble(3);
            rsc.rumbleMng.Rumble(blackboard.player.Id, 1f, 0.25f, 0.6f);

            blackboard.alive = false;
            blackboard.currentLives--;
            blackboard.blinkController.StopPreviousBlinkings();
            blackboard.player.SpawnVoxels();
            PlayerEventInfo.eventInfo.player = blackboard.player;
            rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DIED, PlayerEventInfo.eventInfo);
            //blackboard.player.gameObject.SetActive(false);
            return null;
        }
        else
            return null;  
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }
}
