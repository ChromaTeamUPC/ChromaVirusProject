using UnityEngine;
using System.Collections;

public class PlayerPushedState : PlayerBaseState
{
    private float startingY;

    public override void OnStateEnter()
    {
        bb.animationEnded = false;
        bb.animator.SetTrigger("Hit");

        rsc.rumbleMng.Rumble(bb.player.Id, 0.3f, 0.4f, 0.2f, 0.3f);
        Debug.Log("Entering player pushed state");

        startingY = bb.player.transform.position.y;

        Vector3 direction = bb.player.transform.position - bb.infectionOrigin;
        direction.y = 0;
        direction.Normalize();
        bb.horizontalDirection = direction;
        Debug.Log(bb.horizontalDirection);

        bb.currentSpeed = bb.infectionForces.x;
        bb.verticalVelocity = bb.infectionForces.y;

        bb.player.StopTrail();
        bb.player.DisableUI();
        bb.animator.SetBool("Falling", true);
    }

    public override void OnStateExit()
    {
        bb.currentSpeed = bb.player.walkSpeed;
        bb.player.StartTrail();
        bb.player.EnableUI();
        bb.animator.SetBool("Falling", false);
        Debug.Log("Exiting player pushed state");
    }

    public override PlayerBaseState Update()
    {
        Debug.Log("H speed:" + bb.currentSpeed);
        Debug.Log("V Velocity: " + bb.verticalVelocity);

        if (bb.verticalVelocity < 0 && (startingY >= bb.player.transform.position.y || bb.isGrounded))
        {
            if (bb.isGrounded)
                return bb.idleState;
            else
                return bb.fallingState;
        }
        return null;
    }

    public override void RetrieveInput() { }

    public override PlayerBaseState TakeDamage(float damage, PlayerBaseState nextStateIfDamaged = null, bool whiteBlink = true)
    {
        //can not take more damage during this state
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
