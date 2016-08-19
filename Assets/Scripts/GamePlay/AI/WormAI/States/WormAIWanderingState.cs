using UnityEngine;
using System.Collections;

public class WormAIWanderingState : WormAIBaseState 
{
    private enum SubState
    {
        GOING_TO_ENTRY,
        ENTERING,
        FOLLOWING_PATH,
        EXITING,
        WAITING_FOR_TAIL
    }

    private SubState subState;

    private WormRoute route;
    private int WPIndex;
    private Vector3 currentWP;
    private Vector3 nextWP;
    private Vector3 currentWPUG;
    private Vector3 nextWPUG;
    private Vector3 lastPosition;
    private Quaternion lookRotation;

    private int curveNum;
    private float t;
    private float duration = 4;
    private float shouldMove;
    private float actuallyMoved;

    public WormAIWanderingState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        SetInitialState();
    }

    private void SetInitialState()
    {
        WPIndex = 0;
        //Set initial substate, nav mesh layer and select a random route
        subState = SubState.GOING_TO_ENTRY;

        WormRoute newRoute;
        do
        {
            int routeNum = Random.Range(0, bb.head.routes.Length);
            newRoute = bb.head.routes[routeNum];
        }
        while (route == newRoute);

        route = newRoute;

        bb.head.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
        bb.head.agent.enabled = true;
        bb.head.agent.speed = bb.undergroundSpeed;
        bb.head.agent.SetDestination(route.wayPoints[WPIndex].transform.position - bb.navMeshLayersDistance);
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.GOING_TO_ENTRY:
                if(!bb.head.agent.hasPath || (bb.head.agent.hasPath && bb.head.agent.remainingDistance <= 0.25))
                {
                    bb.head.agent.enabled = false;

                    currentWP = route.wayPoints[WPIndex].transform.position;
                    nextWP = route.wayPoints[WPIndex +1].transform.position;

                    currentWPUG = currentWP - bb.navMeshLayersDistance;
                    nextWPUG = nextWP - bb.navMeshLayersDistance;

                    headTrf.position = currentWPUG;
                    headTrf.LookAt(nextWPUG, Vector3.up);
                    bb.CalculateWorldEnterBezierPoints(bb.head.transform);
                    bb.head.SetVisible(true);

                    //Rotate head
                    Vector3 headUp = currentWPUG - nextWPUG;
                    headTrf.LookAt(currentWP, headUp);

                    t = 0;

                    HexagonController hexagon = route.wayPoints[WPIndex].GetComponent<HexagonController>();
                    hexagon.WormEnterExit();

                    bb.isHeadOverground = true;
                    bb.applySinMovement = true;
                    subState = SubState.ENTERING;
                }
                break;

            case SubState.ENTERING:
                if (t <= 2)
                {
                    shouldMove = Time.deltaTime * bb.wanderingSpeed;
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

                    bb.head.agent.areaMask = WormBlackboard.NAVMESH_FLOOR_LAYER;
                    bb.head.agent.enabled = true;
                    bb.head.agent.speed = bb.wanderingSpeed;
                    bb.head.agent.SetDestination(currentWP);

                    subState = SubState.FOLLOWING_PATH;
                }

                break;

            case SubState.FOLLOWING_PATH:

                if(bb.head.CheckPlayerInSight()) 
                {
                    bb.aboveAttackCurrentExposureTime += Time.deltaTime;
                    //Debug.Log("Player in sight: " + bb.aboveAttackCurrentExposureTime);
                }

                if(bb.aboveAttackCurrentExposureTime >= bb.aboveAttackExposureTimeNeeded &&
                    bb.aboveAttackCurrentCooldownTime <= 0f)
                {
                    return bb.head.aboveAttackState;
                }

                if (!bb.head.agent.hasPath || (bb.head.agent.hasPath && bb.head.agent.remainingDistance <= 0.25))
                {
                    if (WPIndex == route.wayPoints.Length - 2)
                    {
                        bb.head.agent.enabled = false;

                        currentWP = route.wayPoints[WPIndex].transform.position;
                        nextWP = route.wayPoints[WPIndex + 1].transform.position;

                        currentWPUG = currentWP - bb.navMeshLayersDistance;
                        nextWPUG = nextWP - bb.navMeshLayersDistance;

                        headTrf.LookAt(nextWP, Vector3.up);

                        bb.CalculateWorldExitBezierPoints(bb.head.transform);

                        t = 0;

                        HexagonController hexagon = route.wayPoints[WPIndex + 1].GetComponent<HexagonController>();
                        hexagon.WormEnterExit();

                        subState = SubState.EXITING;
                    }
                    else
                    {
                        ++WPIndex;
                        currentWP = route.wayPoints[WPIndex].transform.position;
                        bb.head.agent.SetDestination(currentWP);
                    }
                }
                break;

            case SubState.EXITING:
                if (t <= 2)
                {
                    shouldMove = Time.deltaTime * bb.wanderingSpeed;
                    actuallyMoved = 0;
                    lastPosition = headTrf.position;
                    Vector3 newPos = Vector3.zero;

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

                    bb.isHeadOverground = false;
                    subState = SubState.WAITING_FOR_TAIL;
                }

                break;

            case SubState.WAITING_FOR_TAIL:
                //move head until tail is undeground
                if(!bb.isTailUnderground)
                {
                    MoveUndergroundDirection();
                }
                else
                {
                    bb.applySinMovement = false;
                    //If some random condition attack, else new wandering state
                    if (Random.Range(0f, 1f) <= bb.chancesOfBelowAttackAfterWandering / 100)
                        return bb.head.belowAttackState;
                    else
                        SetInitialState();
                }

                break;

            default:
                break;
        }

        return null;
    }
}
