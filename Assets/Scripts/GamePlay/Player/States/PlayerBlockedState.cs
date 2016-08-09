using UnityEngine;
using System.Collections;

public class PlayerBlockedState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        bb.horizontalDirection = Vector3.zero;
        bb.blinkController.StopPreviousBlinkings();
    }
    //In this state the player can not move nor take damage
    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        return null;
    }

    public override PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        return null;
    }

    public override PlayerBaseState InfectionReceived(float damage, Vector3 origin)
    {
        return null;
    }

    public override PlayerBaseState EnemyTouched()
    {
        return null;
    }
}
