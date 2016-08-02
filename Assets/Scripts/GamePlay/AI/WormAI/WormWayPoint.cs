using UnityEngine;
using System.Collections;

public class WormWayPoint
{
    public WormWayPoint() { }
    public WormWayPoint(Vector3 pos, Quaternion rot, bool vis, WormWayPoint nex = null)
    {
        position = pos;
        rotation = rot;
        visible = vis;
        next = nex;
    }

    public Vector3 position;
    public Quaternion rotation;
    public bool visible;
    public WormWayPoint next;
}
