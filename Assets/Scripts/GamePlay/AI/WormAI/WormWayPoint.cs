using UnityEngine;
using System.Collections;

public class WormWayPoint
{
    public WormWayPoint() { }
    public WormWayPoint(Vector3 pos, Quaternion rot, WormWayPoint nex = null)
    {
        position = pos;
        rotation = rot;
        next = nex;
    }

    public Vector3 position;
    public Quaternion rotation;
    public WormWayPoint next;
}
