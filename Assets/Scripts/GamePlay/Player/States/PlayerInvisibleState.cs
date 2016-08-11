using UnityEngine;
using System.Collections;

public class PlayerInvisibleState : PlayerBaseState {

    public override void OnStateEnter()
    {
        bb.horizontalDirection = Vector3.zero;
        bb.blinkController.StopPreviousBlinkings();
        bb.updateVerticalPosition = false;
        bb.player.MakeInvisible();
    }

    public override void OnStateExit()
    {
        bb.updateVerticalPosition = true;
        bb.player.MakeVisible();
    }

    public override void RetrieveInput() { }

    //In this state the player can not move nor take damage
    public override PlayerBaseState TakeDamage(float damage, PlayerBaseState nextStateIfDamaged = null, bool whiteBlink = true)
    {
        return null;
    }

    public override PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        return null;
    }

    public override PlayerBaseState InfectionReceived(float damage, Vector3 origin, Vector2 infectionForces)
    {
        return null;
    }

    public override PlayerBaseState EnemyTouched()
    {
        return null;
    }
}
