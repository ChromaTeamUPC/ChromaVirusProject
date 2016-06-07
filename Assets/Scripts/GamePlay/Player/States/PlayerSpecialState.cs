using UnityEngine;
using System.Collections;

public class PlayerSpecialState : PlayerBaseState {

    public override void OnStateEnter()
    {
        //play special animation
    }

    public override PlayerBaseState Update()
    {
        //if special done, return idle state
        //else return null
        return blackboard.idleState;
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }
}
