using UnityEngine;
using System.Collections;

public class WormMovementController : MonoBehaviour 
{
    public Transform head;
    public Transform[] bodySegments;
    public Transform tail;

    public float headToSegmentDistance;
    public float segmentToSegmentDistance;
    public float segmentToTailDistance;

    private WormWayPoint headWayPoint;

	// Use this for initialization
	void Awake () 
	{
        headWayPoint = null;

        if(tail != null)
        {
            headWayPoint = new WormWayPoint(tail.position, tail.rotation);
        }

        for(int i = bodySegments.Length - 1; i >= 0; --i)
        {
            WormWayPoint segmentWayPoint = new WormWayPoint(bodySegments[i].position, bodySegments[i].rotation, (headWayPoint != null ? headWayPoint : null));
            headWayPoint = segmentWayPoint;
        }

        if(head != null)
        {
            headWayPoint = new WormWayPoint(head.position, head.rotation, (headWayPoint != null ? headWayPoint : null));
        }
	}
	
	// Update is called once per frame
	void Update () 
	{
        //If head has moved, create a new waypoint and recalculate all segments' position
        if(head.position != headWayPoint.position)
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
                while(consolidatedDistance + distanceBetween < totalDistance)
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
                if(i == bodySegments.Length - 1)
                {
                    if(tail == null)
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
