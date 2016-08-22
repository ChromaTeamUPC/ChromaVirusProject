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

    public WormAIHeadDestroyedState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        head.agent.enabled = false;

        destinyInRange = false;

        elapsedTime = 0f;
        head.phaseExplosion.Play();
        //TODO: animation?


        EnemyDiedEventInfo.eventInfo.infectionValue = 100 / bb.wormMaxPhases;
        EnemyDiedEventInfo.eventInfo.killerPlayer = bb.killerPlayer;
        EnemyDiedEventInfo.eventInfo.killedSameColor = true;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_DESTROYED, EnemyDiedEventInfo.eventInfo);

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
                if(bb.wormCurrentPhase < bb.wormMaxPhases - 1)
                    headTrf.rotation = Quaternion.RotateTowards(headTrf.rotation, initialRotation, bb.headDestroyedLookRotationSpeed * Time.deltaTime);

                if (elapsedTime >= bb.headDestoryedBodyWaitTime)
                {
                    if (bb.wormCurrentPhase == bb.wormMaxPhases -1)
                        return head.dyingState;
                    else
                    {
                        head.StartNewPhase();
                        head.SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);
                        subState = SubState.JUMPING;
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
                    if (distanceToDestiny <= destinyInRangeDistance)
                    {
                        destinyInRange = true;
                        WormEventInfo.eventInfo.wormBb = bb;
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);
                        destiny.WormEnterExit();
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    bb.isHeadOverground = false;
                    head.SetVisible(false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));

                if (bb.isTailUnderground)
                {
                    Vector3 pos = headTrf.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    headTrf.position = pos;

                    /*bb.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
                    bb.agent.enabled = true;
                    bb.agent.speed = bb.WanderingSettingsPhase.undergroundSpeed;
                    bb.agent.SetDestination(bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false)); //Back to entry in the underground
                    */
                    return head.wanderingState;
                }
                break;

            default:
                break;
        }

        return null;
    }


    private HexagonController GetExitHexagon()
    {
        Vector3 offset = headTrf.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * 2;
        Vector3 position = headTrf.position + offset;
        position.y = 0;

        Collider[] colliders = Physics.OverlapSphere(position, 1f, HexagonController.hexagonLayer);

        if (colliders.Length == 0) return null;

        HexagonController result = colliders[0].GetComponent<HexagonController>();
        float distance = (position - colliders[0].transform.position).sqrMagnitude;

        for (int i = 1; i < colliders.Length; ++i)
        {
            float newDistance = (colliders[i].transform.position - position).sqrMagnitude;

            if (newDistance > distance)
            {
                distance = newDistance;
                result = colliders[i].GetComponent<HexagonController>();
            }
        }

        return result;
    }
}
