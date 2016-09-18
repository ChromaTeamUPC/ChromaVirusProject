using UnityEngine;
using System.Collections;

public class PlayerLevelClearedState : PlayerBaseState
{
    private enum SubState
    {
        GOING_TO_DESTINY,
        LEVEL_END_ANIMATION,
        PLAYING_PS,
        WAITING
    }

    private SubState subState;
    private Vector3 destiny;
    private float elapsedTime;
    private bool madeInvisible;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        bb.horizontalDirection = Vector3.zero;
        bb.blinkController.StopPreviousBlinkings();
        bb.updateVerticalPosition = false;

        if (bb.destinationPoint != Vector3.zero)
        {
            destiny = bb.destinationPoint;
            destiny.y = bb.player.transform.position.y;
            subState = SubState.GOING_TO_DESTINY;
        }
        else
        {
            destiny = bb.player.transform.position;
            bb.animator.SetTrigger("LongIdle");
            bb.animationEnded = false;
            subState = SubState.LEVEL_END_ANIMATION;
        }

    }

    public override void OnStateExit()
    {
        bb.player.MakeVisible();
        bb.updateVerticalPosition = true;
    }

    public override PlayerBaseState Update()
    {
        switch (subState)
        {
            case SubState.GOING_TO_DESTINY:
                if (Vector3.Distance(bb.player.transform.position, destiny) > 0.1f)
                {
                    //force walking position
                    bb.animator.SetBool("Walking", true);

                    bb.player.transform.position = Vector3.MoveTowards(bb.player.transform.position, destiny, Time.deltaTime * bb.player.walkSpeed);
                }
                else
                {
                    bb.animator.SetTrigger("LongIdle");
                    bb.animationEnded = false;

                    subState = SubState.LEVEL_END_ANIMATION;
                }
                break;
            case SubState.LEVEL_END_ANIMATION:
                if(!bb.animationEnded)
                {
                    //Look to camera
                    Vector3 direction = bb.GetScreenRelativeDirection(Vector3.down);
                    LookAt(direction);
                }
                else
                {
                    bb.player.PlayBeamUp();
                    elapsedTime = 0f;
                    madeInvisible = false;
                    subState = SubState.PLAYING_PS;
                }
                break;

            case SubState.PLAYING_PS:
                if (elapsedTime >= 0.1 && !madeInvisible)
                {
                    madeInvisible = true;
                    bb.player.MakeInvisible();
                }
                else
                    elapsedTime += Time.deltaTime;
                if(!bb.player.IsBeamUpPlaying())
                {
                    rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_SCORE, EventInfo.emptyInfo);
                    subState = SubState.WAITING;
                }
                break;

            default:
                break;
        }
        return null;
    }

    //In this state the player can not move nor take damage
    public override void RetrieveInput() { }

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

    public override PlayerBaseState InfectionReceived(float damage)
    {
        return null;
    }

    public override PlayerBaseState EnemyTouched()
    {
        return null;
    }
}
