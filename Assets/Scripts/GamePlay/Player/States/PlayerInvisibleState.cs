using UnityEngine;
using System.Collections;

public class PlayerInvisibleState : PlayerBaseState {

    public override void OnStateEnter()
    {
        blackboard.horizontalDirection = Vector3.zero;
        blackboard.blinkController.StopPreviousBlinkings();
        blackboard.updateVerticalPosition = false;
        blackboard.player.MakeInvisible();
    }

    public override void OnStateExit()
    {
        blackboard.updateVerticalPosition = true;
        blackboard.player.MakeVisible();
    }

    public override void RetrieveInput() { }

    //In this state the player can not move nor take damage
    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        return null;
    }

    public override PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        return null;
    }

    public override PlayerBaseState EnemyTouched()
    {
        return null;
    }
}
