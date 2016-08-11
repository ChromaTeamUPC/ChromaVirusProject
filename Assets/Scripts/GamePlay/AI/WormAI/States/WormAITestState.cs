using UnityEngine;
using System.Collections;

public class WormAITestState : WormAIBaseState 
{
    private enum SubState
    {
        GOING_TO_ENTRY,
        ENTERING,
        FOLLOWING_PATH,
        EXITING
    }

    private SubState subState;

    private WormRoute route;
    private int WPIndex;
    private Vector3 currentWP;
    private Vector3 nextWP;
    private Vector3 currentWPUG;
    private Vector3 nextWPUG;
    private Quaternion lookRotation;
    private Vector3 lastPosition;

    private int curveNum;
    private float t;
    private float duration = 4;
    private float shouldMove;
    private float actuallyMoved;

    public WormAITestState(WormBlackboard bb) : base(bb)
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

        route = bb.worm.routes[4];
        bb.agent.enabled = false;

        /*bb.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
        bb.agent.enabled = true;
        bb.agent.speed = bb.undergroundSpeed;
        bb.agent.SetDestination(route.wayPoints[WPIndex].transform.position - bb.navMeshLayersDistance);*/
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.GOING_TO_ENTRY:

                currentWP = route.wayPoints[WPIndex].transform.position;
                nextWP = route.wayPoints[WPIndex +1].transform.position;

                currentWPUG = currentWP - bb.navMeshLayersDistance;
                nextWPUG = nextWP - bb.navMeshLayersDistance;

                Vector3 headUp = currentWPUG - nextWPUG;
                //Rotate head
                head.position = currentWPUG;
                head.LookAt(nextWPUG, Vector3.up);
                bb.CalculateWorldEnterBezierPoints(bb.wormGO.transform);
                head.LookAt(currentWP, headUp);

                curveNum = 0;
                t = 0;

                subState = SubState.ENTERING;
                break;

            case SubState.ENTERING:
                if(curveNum <= 1)
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
                   
                    if(t > 1)
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
                if (bb.agent.hasPath)
                {
                    if (bb.agent.remainingDistance <= 0.25f)
                    {
                        //If it was the last WP, exit
                        if (WPIndex == route.wayPoints.Length - 2)
                        {
                            bb.agent.enabled = false;

                            currentWP = route.wayPoints[WPIndex].transform.position;
                            nextWP = route.wayPoints[WPIndex + 1].transform.position;

                            currentWPUG = currentWP - bb.navMeshLayersDistance;
                            nextWPUG = nextWP - bb.navMeshLayersDistance;

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
                }
                else
                {                 
                    if (WPIndex == route.wayPoints.Length - 2)
                    {
                        bb.agent.enabled = false;

                        currentWP = head.position;
                        nextWP = route.wayPoints[WPIndex + 1].transform.position;

                        currentWPUG = currentWP - bb.navMeshLayersDistance;
                        nextWPUG = nextWP - bb.navMeshLayersDistance;

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
                    //TODO: If some random condition, attack else, new wandering state
                    if (true)
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
