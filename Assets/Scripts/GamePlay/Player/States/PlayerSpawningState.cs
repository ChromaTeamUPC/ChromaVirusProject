using UnityEngine;
using System.Collections;

public class PlayerSpawningState : PlayerBaseState {

    private float elapsedTime;

    public override void OnStateEnter()
    {
        blackboard.horizontalDirection = Vector3.zero;

        blackboard.animator.Rebind();
        //trigger spawning animation
        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNING, PlayerEventInfo.eventInfo);
        blackboard.player.StopTrail();
        blackboard.alive = true;
        blackboard.shield.SetActive(true);
        blackboard.currentSpeed = blackboard.player.walkSpeed;
    }

    public override void OnStateExit()
    {
        blackboard.animator.SetTrigger("KeyPressed");

        blackboard.player.StartTrail();

        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNED, PlayerEventInfo.eventInfo);
    }

    public override void RetrieveInput() { }

    public override PlayerBaseState Update()
    {
        /*if spawning finished return idle
        else return null*/
        //return player.idleState;      

        elapsedTime += Time.deltaTime;
        if (elapsedTime > blackboard.player.idleRandomAnimTime)
        {
            elapsedTime = 0f;
            blackboard.animator.SetTrigger("LongIdle");
        }

        return null;
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }
}
