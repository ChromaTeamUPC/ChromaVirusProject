using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormAIBehaviour : MonoBehaviour
{
    private WormBlackboard blackboard;

    public float floorSpeed = 8;
    public float undergroundSpeed = 12;
    public float rotationSpeed = 360;

    public Transform head;
    public Transform[] bodySegments;
    private List<WormBodySegmentController> bodySegmentControllers;
    public Transform tail;

    public float headToSegmentDistance;
    public float segmentToSegmentDistance;
    public float segmentToTailDistance;

    public WormRoute[] routes;
    private WormWayPoint headWayPoint;

    private WormAIBaseState currentState;

    private Color[] gizmosColors = { Color.blue, Color.cyan, Color.green, Color.grey, Color.magenta, Color.red, Color.yellow };
    //Debug
    void OnDrawGizmos()
    {

        for (int i = 0; i < routes.Length; ++i)
        {
            WormRoute route = routes[i];

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(route.wayPoints[0].transform.position, 1f);

            Gizmos.color = gizmosColors[i % gizmosColors.Length];

            for (int j = 1; j < route.wayPoints.Length; ++j)
            {
                Gizmos.DrawLine(route.wayPoints[j - 1].transform.position, route.wayPoints[j].transform.position);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(route.wayPoints[route.wayPoints.Length-1].transform.position, 1f);
        }
    }

    // Use this for initialization
    void Awake()
    {
        blackboard = new WormBlackboard();
        blackboard.InitialSetup(gameObject);

        SetInitialBodyWayPoints();
    }

    void Start()
    {
        InitBodyParts();

        ChangeState(blackboard.wanderingState);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBodyMovement();

        if (currentState != null)
        {
            WormAIBaseState newState = currentState.Update();

            if (newState != null)
            {
                ChangeState(newState);
            }
        }
    }

    protected void ChangeState(WormAIBaseState newState)
    {
        if (currentState != null)
        {
            //Debug.Log(this.GetType().Name + " Exiting: " + currentState.GetType().Name);
            currentState.OnStateExit();
        }
        currentState = newState;
        if (currentState != null)
        {
            //Debug.Log(this.GetType().Name + " Entering: " + currentState.GetType().Name);
            currentState.OnStateEnter();
        }
    }

    public void ChargeHead()
    {

    }

    public void DischargeHead()
    {
        ShuffleBodyParts();
    }

    //
    public void InitBodyParts()
    {
        List<WormBodySegmentController> randomized = new List<WormBodySegmentController>(bodySegmentControllers);

        randomized.Shuffle();
        randomized.Shuffle();

        //Init each segment color
        for (int i = 0; i < randomized.Count; ++i)
        {
            randomized[i].Init((ChromaColor)(i % ChromaColorInfo.Count));
        }
    }

    public void ShuffleBodyParts()
    {
        List<WormBodySegmentController> randomized = new List<WormBodySegmentController>(bodySegmentControllers);

        randomized.Shuffle();
        randomized.Shuffle();

        //Init each segment color
        for (int i = 0; i < randomized.Count; ++i)
        {
            randomized[i].Reset((ChromaColor)(i % ChromaColorInfo.Count));
        }
    }

    //Body movement functions
    private void SetInitialBodyWayPoints()
    {
        headWayPoint = null;

        if (tail != null)
        {
            headWayPoint = new WormWayPoint(tail.position, tail.rotation);
        }

        bodySegmentControllers = new List<WormBodySegmentController>();
        for (int i = bodySegments.Length - 1; i >= 0; --i)
        {
            WormWayPoint segmentWayPoint = new WormWayPoint(bodySegments[i].position, bodySegments[i].rotation, (headWayPoint != null ? headWayPoint : null));
            headWayPoint = segmentWayPoint;
            bodySegmentControllers.Add(bodySegments[i].GetComponent<WormBodySegmentController>());
        }

        if (head != null)
        {
            headWayPoint = new WormWayPoint(head.position, head.rotation, (headWayPoint != null ? headWayPoint : null));
        }
    }

    private void UpdateBodyMovement()
    {
        //If head has moved, create a new waypoint and recalculate all segments' position
        if ((head.position != headWayPoint.position))
        {
            headWayPoint = new WormWayPoint(head.position, head.rotation, headWayPoint);

            WormWayPoint current = headWayPoint;
            WormWayPoint next = current.next;
            //if we are in the last waypoint, there is nothing more we can do, so we quit
            if (next == null) return;

            float totalDistance = headToSegmentDistance;                            //Total distance we have to position the element from the head
            float consolidatedDistance = 0f;                                        //Sum of the distances of evaluated waypoints
            float distanceBetween = (current.position - next.position).magnitude;   //Distance between current current and next waypoints

            //move each body segment through the virtual line
            for (int i = 0; i < bodySegments.Length; ++i)
            {
                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < totalDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                float remainingDistance = totalDistance - consolidatedDistance;
                Vector3 direction = (next.position - current.position).normalized * remainingDistance;

                bodySegments[i].position = current.position + direction;
                bodySegments[i].rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);


                //if it was the final body part and there is no tail, release the oldest waypoints
                if (i == bodySegments.Length - 1)
                {
                    if (tail == null)
                        next.next = null; //Remove reference, let garbage collector do its job
                }
                //else add total distance for the next iteration
                else
                {
                    totalDistance += segmentToSegmentDistance;
                }
            }

            //finally do the same for the tail
            if (tail != null)
            {
                totalDistance += segmentToTailDistance;

                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < totalDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                float remainingDistance = totalDistance - consolidatedDistance;
                Vector3 direction = (next.position - current.position).normalized * remainingDistance;

                tail.position = current.position + direction;
                tail.rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);

                //release the oldest waypoints
                next.next = null; //Remove reference, let garbage collector do its job
            }
        }
    }
}
