using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        EndColorMismatch(); //Ensure it is not active

        bb.player.StopTrail();
        bb.player.DisableUI();
        bb.shield.SetActive(false);
        bb.animator.SetTrigger("Die");
        bb.animationEnded = false;

        rsc.rumbleMng.AddContinousRumble(RumbleType.PLAYER_DYING, bb.player.Id, 0.25f, 0f);

        PlayerEventInfo.eventInfo.player = bb.player;
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
        bb.currentSpeed *= 0.95f;

        if (bb.animationEnded)
        {
            rsc.rumbleMng.RemoveContinousRumble(RumbleType.PLAYER_DYING);
            rsc.rumbleMng.Rumble(bb.player.Id, 1f, 0.25f, 0.6f);

            bb.alive = false;
            bb.currentLives--;
            bb.blinkController.StopPreviousBlinkings();
            bb.player.SpawnVoxels();
            PlayerEventInfo.eventInfo.player = bb.player;
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
