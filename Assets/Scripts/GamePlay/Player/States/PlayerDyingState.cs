using UnityEngine;
using System.Collections;

public class PlayerDyingState : PlayerBaseState
{
    private enum SubState
    {
        WAITING_ANIMATION,
        WAITING_RUMBLE
    }

    private SubState subState;

    private float elapsedTime;

    public override void OnStateEnter()
    {
        EndColorMismatch(); //Ensure it is not active

        bb.player.StopTrail();
        bb.player.DisableUI();
        bb.shield.SetActive(false);
        bb.animator.SetTrigger("Die");
        bb.animationEnded = false;
        bb.player.PlayDieSound();

        subState = SubState.WAITING_ANIMATION;

        rsc.rumbleMng.AddContinousRumble(RumbleId.PLAYER_DYING, bb.player.Id, 0.2f, 0f);

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
        switch (subState)
        {
            case SubState.WAITING_ANIMATION:
                bb.currentSpeed *= 0.95f;

                if (bb.animationEnded)
                {
                    rsc.rumbleMng.RemoveContinousRumble(RumbleId.PLAYER_DYING);
                    rsc.rumbleMng.Rumble(bb.player.Id, 0.5f, 0.5f, 0.5f);
                   
                    bb.blinkController.StopPreviousBlinkings();
                    bb.player.SpawnVoxels();
                    bb.currentSpeed = 0;
                    bb.player.MakeInvisible();

                    elapsedTime = 0f;

                    subState = SubState.WAITING_RUMBLE;
                }
                break;

            case SubState.WAITING_RUMBLE:
                if (elapsedTime >= 0.5f)
                {
                    bb.alive = false;
                    bb.currentLives--;
                    if (bb.currentLives == 0)
                        bb.playing = false;

                    PlayerEventInfo.eventInfo.player = bb.player;
                    rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DIED, PlayerEventInfo.eventInfo);
                    //blackboard.player.gameObject.SetActive(false);
                    return null;
                }
                else
                    elapsedTime += Time.deltaTime;
                break;

            default:
                break;
        }

        return null;       
    }

    public override PlayerBaseState TakeDamage(float damage, PlayerBaseState nextStateIfDamaged = null, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }
}
