using UnityEngine;
using System.Collections;

public class WormAIWanderingState : WormAIBaseState 
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
    private Transform head;
    private Quaternion lookRotation;

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
                if(bb.agent.hasPath && bb.agent.remainingDistance <= 0.25f)
                {
                    bb.agent.enabled = false;

                    currentWP = route.wayPoints[WPIndex].transform.position;
                    nextWP = route.wayPoints[WPIndex +1].transform.position;

                    Vector3 currentWPUG = currentWP - bb.navMeshLayersDistance;
                    Vector3 nextWPUG = nextWP - bb.navMeshLayersDistance;

                    Vector3 headUp = currentWPUG - nextWPUG;
                    //Rotate head
                    head.LookAt(currentWP, headUp);

                    subState = SubState.ENTERING;
                }
                break;

            case SubState.ENTERING:
                head.position = Vector3.MoveTowards(head.position, head.position + head.forward, Time.deltaTime * bb.floorSpeed);

                //Go up
                if(head.position.y < currentWP.y)
                {
                    head.position = Vector3.MoveTowards(head.position, currentWP, Time.deltaTime * bb.floorSpeed);
                }
                //Then rotate
                else if (Vector3.Angle(head.forward, nextWP - head.position) > 1)
                {
                    lookRotation = Quaternion.LookRotation(nextWP - head.position, Vector3.up);
                    head.rotation = Quaternion.RotateTowards(head.rotation, lookRotation, Time.deltaTime * bb.rotationSpeed);
                }
                //Then start following path
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
                if (bb.agent.hasPath && bb.agent.remainingDistance <= 0.25f)
                {
                    //If it was the last WP, exit
                    if (WPIndex == route.wayPoints.Length -1)
                    {
                        bb.agent.enabled = false;

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
                //rotate down
                if(Vector3.Angle(head.forward, Vector3.down) > 1)
                {
                    lookRotation = Quaternion.LookRotation(Vector3.down, head.forward);
                    head.rotation = Quaternion.RotateTowards(head.rotation, lookRotation, Time.deltaTime * bb.rotationSpeed);
                }
                //go down
                else if (head.position.y > currentWP.y - WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    head.position = Vector3.MoveTowards(head.position, currentWP - bb.navMeshLayersDistance, Time.deltaTime * bb.floorSpeed);
                }
                //else decide what's next
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
