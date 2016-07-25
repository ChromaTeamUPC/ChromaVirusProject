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

        head = blackboard.worm.head.transform;

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
            int routeNum = Random.Range(0, blackboard.worm.routes.Length);
            newRoute = blackboard.worm.routes[routeNum];
        }
        while (route == newRoute);

        route = newRoute;

        blackboard.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
        blackboard.agent.enabled = true;
        blackboard.agent.speed = blackboard.worm.undergroundSpeed;
        blackboard.agent.SetDestination(route.wayPoints[WPIndex].transform.position - blackboard.navMeshLayersDistance);
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.GOING_TO_ENTRY:
                if(blackboard.agent.hasPath && blackboard.agent.remainingDistance <= 0.25f)
                {
                    blackboard.agent.enabled = false;

                    currentWP = route.wayPoints[WPIndex].transform.position;
                    nextWP = route.wayPoints[WPIndex +1].transform.position;

                    Vector3 currentWPUG = currentWP - blackboard.navMeshLayersDistance;
                    Vector3 nextWPUG = nextWP - blackboard.navMeshLayersDistance;

                    Vector3 headUp = currentWPUG - nextWPUG;
                    //Rotate head
                    head.LookAt(currentWP, headUp);

                    subState = SubState.ENTERING;
                }
                break;

            case SubState.ENTERING:
                //Go up
                if(head.position.y < currentWP.y)
                {
                    head.position = Vector3.MoveTowards(head.position, currentWP, Time.deltaTime * blackboard.worm.floorSpeed);
                }
                //Then rotate
                else if (Vector3.Angle(head.forward, nextWP - head.position) > 1)
                {
                    lookRotation = Quaternion.LookRotation(nextWP - head.position, Vector3.up);
                    head.rotation = Quaternion.RotateTowards(head.rotation, lookRotation, Time.deltaTime * blackboard.worm.rotationSpeed);
                }
                //Then start following path
                else
                {
                    ++WPIndex;
                    currentWP = route.wayPoints[WPIndex].transform.position;

                    blackboard.agent.areaMask = WormBlackboard.NAVMESH_FLOOR_LAYER;
                    blackboard.agent.enabled = true;
                    blackboard.agent.speed = blackboard.worm.floorSpeed;
                    blackboard.agent.SetDestination(currentWP);

                    subState = SubState.FOLLOWING_PATH;
                }
                break;

            case SubState.FOLLOWING_PATH:
                if (blackboard.agent.hasPath && blackboard.agent.remainingDistance <= 0.25f)
                {
                    //If it was the last WP, exit
                    if (WPIndex == route.wayPoints.Length -1)
                    {
                        blackboard.agent.enabled = false;

                        subState = SubState.EXITING;
                    }
                    else
                    {
                        ++WPIndex;
                        currentWP = route.wayPoints[WPIndex].transform.position;
                        blackboard.agent.SetDestination(currentWP);
                    }
                }
                break;

            case SubState.EXITING:
                //rotate down
                if(Vector3.Angle(head.forward, Vector3.down) > 1)
                {
                    lookRotation = Quaternion.LookRotation(Vector3.down, head.forward);
                    head.rotation = Quaternion.RotateTowards(head.rotation, lookRotation, Time.deltaTime * blackboard.worm.rotationSpeed);
                }
                //go down
                else if (head.position.y > currentWP.y - WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    head.position = Vector3.MoveTowards(head.position, currentWP - blackboard.navMeshLayersDistance, Time.deltaTime * blackboard.worm.floorSpeed);
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
