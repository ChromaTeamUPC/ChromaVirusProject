using UnityEngine;
using System.Collections;

public class WormAIWanderingState : WormAIBaseState 
{
    private enum SubState
    {
        WAITING,
        ENTERING,
        FOLLOWING_PATH,
        EXITING,
        WAITING_FOR_TAIL,
        JUMPING,
        JUMP_EXITING
    }

    private SubState subState;

    private WormRoute route;
    private int WPIndex;
    private Vector3 currentWP;
    private Vector3 nextWP;
    private Vector3 currentWPUG;
    private Vector3 nextWPUG;
    private Vector3 lastPosition;

    private float t;
    private float duration = 4;
    private float shouldMove;
    private float actuallyMoved;

    private float elapsedTime;

    private bool angryEyes;
    private bool exitRumble;

    private HexagonController destiny;

    private float currentX;
    private Quaternion initialRotation;
    private float destinyInRangeDistance = 1f;
    private bool destinyInRange;
    private float speed;

    private Vector3 priorPosition;

    public WormAIWanderingState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        SetInitialState();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        head.NotWatchingPlayer();
    }

    private void SetInitialState()
    {
        bb.shouldMeteorBeTriggedAfterWandering = false;
        bb.meteorInmediate = false;
        angryEyes = false;
        exitRumble = false;
        head.NotWatchingPlayer();

        bb.SetWormVulnerable();

        elapsedTime = 0f;
        WPIndex = 0;
        destinyInRange = false;
        //Set initial substate, nav mesh layer and select a random route
        subState = SubState.WAITING;

        WormRoute newRoute;
        do
        {
            //int routeNum = Random.Range(0, head.routes.Length);
            int routeNum = Random.Range(bb.WanderingSettingsPhase.routeMinId, bb.WanderingSettingsPhase.routeMaxId + 1);
            newRoute = head.routes[routeNum];
        }
        while (route == newRoute);

        route = newRoute;
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.WAITING:
                if (elapsedTime >= bb.WanderingSettingsPhase.initialWaitTime)
                {
                    head.agent.enabled = false;

                    currentWP = route.wayPoints[WPIndex].transform.position;
                    nextWP = route.wayPoints[WPIndex + 1].transform.position;

                    currentWPUG = currentWP - bb.navMeshLayersDistance;
                    nextWPUG = nextWP - bb.navMeshLayersDistance;

                    headTrf.position = currentWPUG;
                    headTrf.LookAt(nextWPUG, Vector3.up);
                    bb.CalculateWorldEnterBezierPoints(headTrf);
                    head.SetVisible(true);

                    //Rotate head
                    Vector3 headUp = currentWPUG - nextWPUG;
                    headTrf.LookAt(currentWP, headUp);

                    t = 0;

                    HexagonController hexagon = route.wayPoints[WPIndex].GetComponent<HexagonController>();
                    hexagon.WormEnterExit();

                    EnterExitActions();
                    head.animator.SetBool("MouthOpen", true);

                    bb.isHeadOverground = true;
                    bb.applySinMovement = true;               

                    subState = SubState.ENTERING;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.ENTERING:
                if (t <= 2)
                {
                    shouldMove = Time.deltaTime * bb.WanderingSettingsPhase.wanderingSpeed;
                    actuallyMoved = 0;
                    lastPosition = headTrf.position;
                    Vector3 newPos = Vector3.zero;

                    while (actuallyMoved < shouldMove && t <= 2)
                    {
                        newPos = bb.GetEnterCurvePosition(currentWPUG, nextWP, t);
                        
                        actuallyMoved = (newPos - lastPosition).magnitude;

                        t += Time.deltaTime / duration;
                    }
                    headTrf.position = newPos;
                    headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));
                }
                else
                {
                    ++WPIndex;
                    currentWP = route.wayPoints[WPIndex].transform.position;

                    head.agent.areaMask = WormBlackboard.NAVMESH_FLOOR_LAYER;
                    head.agent.enabled = true;
                    head.agent.speed = bb.WanderingSettingsPhase.wanderingSpeed;
                    head.agent.SetDestination(currentWP);

                    head.animator.SetBool("MouthOpen", false);

                    bb.meteorInmediate = true;
                    priorPosition = headTrf.position;
                    elapsedTime = 0;

                    subState = SubState.FOLLOWING_PATH;
                }

                break;

            case SubState.FOLLOWING_PATH:

                if(bb.AboveAttackSettingsPhase.active && head.CheckPlayerInSight(bb.AboveAttackSettingsPhase.exposureMinHexagons, bb.AboveAttackSettingsPhase.exposureMaxHexagons, false)) 
                {
                    bb.aboveAttackCurrentExposureTime += Time.deltaTime;

                    if (!angryEyes)
                    {
                        angryEyes = true;
                        head.WatchingPlayer();
                    }
                }
                else
                {
                    if (angryEyes)
                    {
                        angryEyes = false;
                        head.NotWatchingPlayer();
                    }
                }

                if (bb.attacksEnabled && bb.AboveAttackSettingsPhase.active && bb.aboveAttackCurrentExposureTime >= bb.AboveAttackSettingsPhase.exposureTimeNeeded &&
                    bb.aboveAttackCurrentCooldownTime >= bb.AboveAttackSettingsPhase.cooldownTime &&
                    head.CheckPlayerInSight(bb.AboveAttackSettingsPhase.attackMinHexagons, bb.AboveAttackSettingsPhase.attackMaxHexagons, true))
                {
                    if (bb.playerInSight != null)
                    {
                        HexagonController destiny = bb.playerInSight.GetNearestHexagon();

                        if (destiny != null && destiny.isWormSelectable)
                        {
                            if (angryEyes)
                            {
                                angryEyes = false;
                                head.NotWatchingPlayer();
                            }
                            return head.aboveAttackState;
                        }
                    }                   
                }

                //Not reached next waypoint
                if (elapsedTime >= 6f)
                {
                    head.agent.enabled = false;

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
                    headTrf.rotation = Quaternion.LookRotation(nextPosition - startPosition, headTrf.up);

                    subState = SubState.JUMPING;
                }
                else
                {
                    elapsedTime += Time.deltaTime;

                    if (!head.agent.hasPath || (head.agent.hasPath && head.agent.remainingDistance <= 0.25))
                    {
                        if (WPIndex == route.wayPoints.Length - 2)
                        {
                            head.agent.enabled = false;

                            currentWP = route.wayPoints[WPIndex].transform.position;
                            nextWP = route.wayPoints[WPIndex + 1].transform.position;

                            currentWPUG = currentWP - bb.navMeshLayersDistance;
                            nextWPUG = nextWP - bb.navMeshLayersDistance;

                            headTrf.LookAt(nextWP, Vector3.up);

                            bb.CalculateWorldExitBezierPoints(headTrf);

                            t = 0;

                            HexagonController hexagon = route.wayPoints[WPIndex + 1].GetComponent<HexagonController>();
                            hexagon.WormEnterExit();

                            if (angryEyes)
                            {
                                angryEyes = false;
                                head.NotWatchingPlayer();
                            }

                            head.animator.SetBool("MouthOpen", true);
                            bb.meteorInmediate = false;

                            subState = SubState.EXITING;
                        }
                        else
                        {
                            elapsedTime = 0;
                            ++WPIndex;
                            currentWP = route.wayPoints[WPIndex].transform.position;
                            head.agent.SetDestination(currentWP);
                        }
                    }
                }

                break;

            case SubState.EXITING:
                if (t <= 2)
                {
                    shouldMove = Time.deltaTime * bb.WanderingSettingsPhase.wanderingSpeed;
                    actuallyMoved = 0;
                    lastPosition = headTrf.position;
                    Vector3 newPos = Vector3.zero;

                    if(t >= 1.6f && !exitRumble)
                    {
                        exitRumble = true;
                        EnterExitActions();
                    }

                    while (actuallyMoved < shouldMove && t <= 2)
                    {
                        newPos = bb.GetExitCurvePosition(currentWP, nextWPUG, t);                       
                        actuallyMoved = (newPos - lastPosition).magnitude;

                        t += Time.deltaTime / duration;
                    }
                    headTrf.position = newPos;
                    headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));
                }
                else
                {
                    SetUndergroundDirection();
                    SetHeadUnderground();
                    head.animator.SetBool("MouthOpen", false);

                    subState = SubState.WAITING_FOR_TAIL;
                }

                break;

            case SubState.WAITING_FOR_TAIL:

                //move head until tail is undeground
                if (!bb.tailReachedMilestone)
                {
                    MoveUndergroundDirection();
                }
                else
                {
                    bb.applySinMovement = false;

                    if(bb.shouldMeteorBeTriggedAfterWandering)
                    {
                        return head.meteorAttackState;
                    }                   
                    //If some random condition attack, else new wandering state
                    else if (bb.attacksEnabled && bb.BelowAttackSettingsPhase.active &&
                        Random.Range(0f, 1f) <= bb.BelowAttackSettingsPhase.chancesOfBelowAttackAfterWandering / 100)
                        return head.belowAttackState;
                    else
                        SetInitialState();
                }

                break;


            //Failsafe exit 
            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition), headTrf.up);

                if (!destinyInRange)
                {
                    float distanceToDestiny = (headTrf.position - destiny.transform.position).magnitude;
                    if (distanceToDestiny <= destinyInRangeDistance ||
                        headTrf.position.y < destiny.transform.position.y) //Safety check. When jump is too fast distance can never be less than range distance
                    {
                        destinyInRange = true;

                        JumpExitActions();

                        destiny.WormEnterExit();
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    SetHeadUnderground();
                    head.animator.SetBool("MouthOpen", false);

                    subState = SubState.JUMP_EXITING;
                }
                break;

            case SubState.JUMP_EXITING:
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

    public override WormAIBaseState ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (subState != SubState.FOLLOWING_PATH) return null;

        return head.ProcessShotImpact(shotColor, damage, player);
    }           
}
