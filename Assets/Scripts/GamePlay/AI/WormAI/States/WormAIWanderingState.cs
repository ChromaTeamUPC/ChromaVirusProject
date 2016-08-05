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
    private Transform head;
    private Vector3 lastPosition;
    private Quaternion lookRotation;
    private Vector3 undergroundDirection;

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

        head = bb.headTrf.transform;

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
            int routeNum = Random.Range(0, bb.worm.routes.Length);
            newRoute = bb.worm.routes[routeNum];
        }
        while (route == newRoute);

        route = newRoute;

        bb.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
        bb.agent.enabled = true;
        bb.agent.speed = bb.undergroundSpeed;
        bb.agent.SetDestination(route.wayPoints[WPIndex].transform.position - bb.navMeshLayersDistance);
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.GOING_TO_ENTRY:
                if(!bb.agent.hasPath || (bb.agent.hasPath && bb.agent.remainingDistance <= 0.25))
                {
                    bb.agent.enabled = false;

                    currentWP = route.wayPoints[WPIndex].transform.position;
                    nextWP = route.wayPoints[WPIndex +1].transform.position;

                    currentWPUG = currentWP - bb.navMeshLayersDistance;
                    nextWPUG = nextWP - bb.navMeshLayersDistance;

                    head.position = currentWPUG;
                    head.LookAt(nextWPUG, Vector3.up);
                    bb.CalculateWorldEnterBezierPoints(bb.wormGO.transform);
                    bb.worm.SetVisible(true);

                    //Rotate head
                    Vector3 headUp = currentWPUG - nextWPUG;
                    head.LookAt(currentWP, headUp);

                    curveNum = 0;
                    t = 0;

                    subState = SubState.ENTERING;
                }
                break;

            case SubState.ENTERING:
                if (curveNum <= 1)
                {
                    shouldMove = Time.deltaTime * bb.floorSpeed;
                    actuallyMoved = 0;
                    lastPosition = head.position;
                    Vector3 newPos = Vector3.zero;

                    while (actuallyMoved < shouldMove && t <= 1)
                    {
                        if (curveNum == 0)
                        {
                            newPos = bb.BezierCubic(currentWPUG, bb.worldEnterBezierCtrl11, bb.worldEnterBezierCtrl12, bb.worldEnterBezierEnd1Start2, t);
                        }
                        else
                        {
                            newPos = bb.BezierCubic(bb.worldEnterBezierEnd1Start2, bb.worldEnterBezierCtrl21, bb.worldEnterBezierCtrl22, nextWP, t);
                        }
                        actuallyMoved = (newPos - lastPosition).magnitude;

                        t += Time.deltaTime / duration;
                    }
                    head.position = newPos;
                    head.LookAt(head.position + (head.position - lastPosition));

                    if (t > 1)
                    {
                        ++curveNum;
                        t = 0;
                    }
                }
                else
                {
                    ++WPIndex;
                    currentWP = route.wayPoints[WPIndex].transform.position;

                    bb.agent.areaMask = WormBlackboard.NAVMESH_FLOOR_LAYER;
                    bb.agent.enabled = true;
                    bb.agent.speed = bb.floorSpeed;
                    bb.agent.SetDestination(currentWP);

                    subState = SubState.FOLLOWING_PATH;
                }

                break;

            case SubState.FOLLOWING_PATH:
                if (!bb.agent.hasPath || (bb.agent.hasPath && bb.agent.remainingDistance <= 0.25))
                {
                    if (WPIndex == route.wayPoints.Length - 2)
                    {
                        bb.agent.enabled = false;

                        currentWP = route.wayPoints[WPIndex].transform.position;
                        nextWP = route.wayPoints[WPIndex + 1].transform.position;

                        currentWPUG = currentWP - bb.navMeshLayersDistance;
                        nextWPUG = nextWP - bb.navMeshLayersDistance;

                        head.LookAt(nextWP, Vector3.up);

                        bb.CalculateWorldExitBezierPoints(bb.wormGO.transform);

                        curveNum = 0;
                        t = 0;

                        subState = SubState.EXITING;
                    }
                    else
                    {
                        ++WPIndex;
                        currentWP = route.wayPoints[WPIndex].transform.position;
                        bb.agent.SetDestination(currentWP);
                    }
                }
                break;

            case SubState.EXITING:
                if (curveNum <= 1)
                {
                    shouldMove = Time.deltaTime * bb.floorSpeed;
                    actuallyMoved = 0;
                    lastPosition = head.position;
                    Vector3 newPos = Vector3.zero;

                    while (actuallyMoved < shouldMove && t <= 1)
                    {
                        if (curveNum == 0)
                        {
                            newPos = bb.BezierCubic(currentWP, bb.worldExitBezierCtrl11, bb.worldExitBezierCtrl12, bb.worldExitBezierEnd1Start2, t);
                        }
                        else
                        {
                            newPos = bb.BezierCubic(bb.worldExitBezierEnd1Start2, bb.worldExitBezierCtrl21, bb.worldExitBezierCtrl22, nextWPUG, t);
                        }
                        actuallyMoved = (newPos - lastPosition).magnitude;

                        t += Time.deltaTime / duration;
                    }
                    head.position = newPos;
                    head.LookAt(head.position + (head.position - lastPosition));

                    if (t > 1)
                    {
                        ++curveNum;
                        t = 0;
                    }
                }
                else
                {
                    bb.worm.SetVisible(false);

                    //Set direction to scene center
                    undergroundDirection = bb.sceneCenter.transform.position - head.position;
                    undergroundDirection.y = 0;
                    undergroundDirection.Normalize();

                    subState = SubState.WAITING_FOR_TAIL;
                }

                break;

            case SubState.WAITING_FOR_TAIL:
                //move head until tail is undeground
                if(!bb.tailIsUnderground)
                {
                    head.position = head.position + (undergroundDirection * bb.undergroundSpeed * Time.deltaTime);
                }
                else
                {
                    //TODO: If some random condition, attack else, new wandering state
                    if (Random.Range(0f, 1f) > 0.5f)
                        SetInitialState();
                    else
                        //return blackboard.belowAttackState;
                        SetInitialState();
                }

                break;

            default:
                break;
        }

        return null;
    }
}
