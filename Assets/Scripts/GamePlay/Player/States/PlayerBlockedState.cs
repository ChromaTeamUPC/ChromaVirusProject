using UnityEngine;
using System.Collections;

public class PlayerBlockedState : PlayerBaseState
{
    public override void OnStateEnter()
    {
        blackboard.blinkController.StopPreviousBlinkings();
        blackboard.horizontalDirection = Vector3.zero;
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

    public override void EnemyTouched() {}
}
