using UnityEngine;
using System.Collections;

public class PlayerSpawningState : PlayerBaseState {

    public override void OnStateEnter()
    {
        //trigger spawning animation
        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNING, PlayerEventInfo.eventInfo);
        blackboard.alive = true;
        blackboard.shield.SetActive(true);
        blackboard.player.StartTrail();
        blackboard.currentSpeed = blackboard.player.walkSpeed;
    }

    public override PlayerBaseState Update()
    {
        /*if spawning finished return idle
        else return null*/
        //return player.idleState;

        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNED, PlayerEventInfo.eventInfo);

        return blackboard.idleState;
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }
}
