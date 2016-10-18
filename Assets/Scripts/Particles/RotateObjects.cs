using UnityEngine;
using System.Collections;

public class RotateObjects : MonoBehaviour {

    public bool active = true;
    public bool rotateX = false;
    public float dpsSpeedX = 90f;
    public bool rotateY = false;
    public float dpsSpeedY = 90f;
    public bool rotateZ = false;
    public float dpsSpeedZ = 90f;
    public bool globalSpace = true;

    // Update is called once per frame
    public void Update()
    {
        if (active)
            transform.Rotate((rotateX ? dpsSpeedX : 0),
                              (rotateY ? dpsSpeedY : 0),
                              (rotateZ ? dpsSpeedZ : 0),
                              (globalSpace ? Space.World : Space.Self));
    }
}
