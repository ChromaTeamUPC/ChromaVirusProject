using UnityEngine;
using System.Collections;

public class PlayerSpecialState : PlayerBaseState {

    public override void OnStateEnter()
    {
        blackboard.horizontalDirection = Vector3.zero;
        blackboard.animationEnded = false;
        blackboard.animator.SetTrigger("SpecialAttack");

        if(!rsc.debugMng.godMode)
            blackboard.player.SpendEnergy(blackboard.player.specialAttackNecessaryEnergy);
        blackboard.player.StartSpecialEnergyCharging();
    }

    public override PlayerBaseState Update()
    {
        if(blackboard.animationEnded)
        {
            blackboard.player.StopSpecialEnergyCharging();
            //play explosion

            return blackboard.idleState;
        }
        else
        {
            return null;
        }
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }

    public override PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        //can not take damage during this state
        return null;
    }

    public override void EnemyTouched()
    {
        //can not take damage during this state
        return;
    }

    public override void ColorMismatch()
    {
        //can not take damage during this state
        return;
    }
}
