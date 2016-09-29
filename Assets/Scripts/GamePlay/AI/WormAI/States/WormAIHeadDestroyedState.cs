using UnityEngine;
using System.Collections;

public class WormAIHeadDestroyedState : WormAIBaseState
{
    private enum SubState
    {
        WAITING_HEAD,
        WAITING_BODY,
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

    private bool shouldTriggerMeteor;

    public WormAIHeadDestroyedState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        bb.SetWormInvulnerable();

        head.agent.enabled = false;
        destinyInRange = false;
        elapsedTime = 0f;

        head.explosion2SoundFx.Play();
        head.phaseExplosion.Play();
        head.animator.SetBool("Hit", true);

        WormEventInfo.eventInfo.wormBb = bb;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_PHASE_ENDED, WormEventInfo.eventInfo);

        shouldTriggerMeteor = bb.attacksEnabled && bb.MeteorAttackSettingsPhase.active && bb.MeteorAttackSettingsPhase.triggerAfterHeadDestroyed;

        subState = SubState.WAITING_HEAD;
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            //Wait for head Fx and animation
            case SubState.WAITING_HEAD:
                if(elapsedTime >= bb.headDestroyedWaitTime)
                {
                    if (!shouldTriggerMeteor)
                    {
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
                    }

                    head.animator.SetBool("Hit", false);

                    bb.ConsolidateBodyParts();
                    subState = SubState.WAITING_BODY;                  

                    elapsedTime = 0f;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
                break;

            //Wait for body parts destruction
            case SubState.WAITING_BODY:

                if(bb.wormCurrentPhase < bb.wormMaxPhases - 1 && !shouldTriggerMeteor)
                    headTrf.rotation = Quaternion.RotateTowards(headTrf.rotation, initialRotation, bb.headDestroyedLookRotationSpeed * Time.deltaTime);

                if (elapsedTime >= bb.headDestoryedBodyWaitTime)
                {
                    if (bb.wormCurrentPhase == bb.wormMaxPhases -1)
                        return head.dyingState;
                    else
                    {
                        head.StartNewPhase();
                        head.SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));
                        //rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);                     

                        if (shouldTriggerMeteor)
                        {
                            bb.meteorInmediate = true;
                            return head.meteorAttackState;
                        }
                        else
                        {
                            head.animator.SetBool("MouthOpen", true);
                            subState = SubState.JUMPING;
                        }
                    }
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

                        AttackActions();

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
}
