using UnityEngine;
using System.Collections;

public class PlayerSpawningState : PlayerBaseState {

    private float elapsedTime;

    public override void OnStateEnter()
    {
        bb.player.MakeVisible();
        bb.horizontalDirection = Vector3.zero;

        bb.animator.Rebind();
        //trigger spawning animation
        PlayerEventInfo.eventInfo.player = bb.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNING, PlayerEventInfo.eventInfo);
        bb.player.StopTrail();
        bb.player.DisableUI();       
        bb.shield.SetActive(true);
        bb.currentSpeed = bb.player.walkSpeed;
    }

    public override void OnStateExit()
    {
        bb.player.StartTrail();
        bb.player.EnableUI();

        /*if(!bb.animationEnded)
            bb.animator.SetTrigger("KeyPressed");*/

        PlayerEventInfo.eventInfo.player = bb.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNED, PlayerEventInfo.eventInfo);
    }

    public override void RetrieveInput() { }

    public override PlayerBaseState Update()
    {
        /*if spawning finished return idle
        else return null*/
        //return player.idleState;      

        elapsedTime += Time.deltaTime;
        if (elapsedTime > bb.player.idleRandomAnimTime)
        {
            elapsedTime = 0f;
            bb.animator.SetTrigger("LongIdle");
            bb.animationEnded = false;
        }

        return null;
    }

    public override PlayerBaseState TakeDamage(float damage, PlayerBaseState nextStateIfDamaged = null, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }
}
