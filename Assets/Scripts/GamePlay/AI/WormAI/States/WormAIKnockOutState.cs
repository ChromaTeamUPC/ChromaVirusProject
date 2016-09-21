using UnityEngine;
using System.Collections;

public class WormAIKnockOutState : WormAIBaseState
{
    private enum SubState
    {
        KNOCKED_OUT,
        MOVING_HEAD,
        JUMPING,
        EXITING
    }
    private SubState subState;

    private float elapsedTime;
    private HexagonController destiny;

    private float currentX;
    private Vector3 lastPosition;
    private Quaternion initialRotation;
    private float destinyInRangeDistance = 1f;
    private bool destinyInRange;
    private float speed;
    private bool knockOutTutorialTriggered = false;

    public WormAIKnockOutState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        head.agent.enabled = false;
        destinyInRange = false;
        elapsedTime = 0f;

        head.SpawnEnergyVoxels();
        head.HeadKnockOut();

        head.animator.SetBool("Stunned", true);

        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_VULNERABLE, EventInfo.emptyInfo);

        if (!knockOutTutorialTriggered)
        {
            knockOutTutorialTriggered = true;
            TutorialEventInfo.eventInfo.type = TutorialManager.Type.HIT_BOSS_HEAD;
            rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
        }

        subState = SubState.KNOCKED_OUT;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        head.animator.SetBool("Stunned", false);

        head.DeactivateHead();
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.KNOCKED_OUT:
                if (elapsedTime >= bb.HealthSettingsPhase.knockOutTime)
                {
                    //Restore
                    head.ResetPhase();

                    destiny = GetExitHexagon();
                    bb.CalculateParabola(headTrf.position, destiny.transform.position);
                    speed = (headTrf.position - destiny.transform.position).magnitude / bb.headDestroyedJumpDuration;

                    //Calculate start point and prior point
                    currentX = bb.GetJumpXGivenY(0, false);
                    Vector3 startPosition = bb.GetJumpPositionGivenY(0, false);
                    headTrf.position = startPosition;

                    lastPosition = bb.GetJumpPositionGivenX(currentX);

                    float fakeNextX = currentX + Time.deltaTime * 2;
                    Vector3 nextPosition = bb.GetJumpPositionGivenX(fakeNextX);
                    initialRotation = Quaternion.LookRotation(nextPosition - startPosition, headTrf.up);

                    head.animator.SetBool("Stunned", false);
                    head.animator.SetBool("MouthOpen", true);

                    elapsedTime = 0f;

                    rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_INVULNERABLE, EventInfo.emptyInfo);
                    subState = SubState.MOVING_HEAD;
                }
                else
                    elapsedTime += Time.deltaTime;
                break;

            case SubState.MOVING_HEAD:
                headTrf.rotation = Quaternion.RotateTowards(headTrf.rotation, initialRotation, bb.headDestroyedLookRotationSpeed * Time.deltaTime);

                if (elapsedTime >= bb.headDestoryedBodyWaitTime)
                {
                    subState = SubState.JUMPING;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition), headTrf.up);

                headTrf.Rotate(new Vector3(0, 0, bb.headDestroyedRotationSpeed * Time.deltaTime));

                if (!destinyInRange)
                {
                    float distanceToDestiny = (headTrf.position - destiny.transform.position).magnitude;
                    if (distanceToDestiny <= destinyInRangeDistance ||
                        headTrf.position.y < destiny.transform.position.y) //Safety check. When jump is too fast distance can never be less than range distance
                    {
                        destinyInRange = true;
                        WormEventInfo.eventInfo.wormBb = bb;
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);
                        destiny.WormEnterExit();
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    SetHeadUnderground();
                    head.animator.SetBool("MouthOpen", false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));

                if (bb.tailReachedMilestone)
                {
                    Vector3 pos = headTrf.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    headTrf.position = pos;

                    return head.wanderingState;
                }
                break;

            default:
                break;
        }       

        return null;
    }

    public override WormAIBaseState ImpactedBySpecial(float damage, PlayerController player)
    {
        if (subState != SubState.KNOCKED_OUT) return null;

        bb.killerPlayer = player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_INVULNERABLE, EventInfo.emptyInfo);
        return head.headDestroyedState;
    }
}
