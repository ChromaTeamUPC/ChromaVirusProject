using UnityEngine;
using System.Collections;

public class RotateParticles : MonoBehaviour {

    public bool active = true;
    public bool rotateX = false;
    public float dpsSpeedX = 90f;
    public bool rotateY = false;
    public float dpsSpeedY = 90f;
    public bool rotateZ = false;
    public float dpsSpeedZ = 90f;
    public bool globalSpace = true;

    // Update is called once per frame
    void Update ()
    {
        if (active)
            transform.Rotate( (rotateX ? dpsSpeedX : 0) * Time.deltaTime,
                              (rotateY ? dpsSpeedY : 0) * Time.deltaTime,
                              (rotateZ ? dpsSpeedZ : 0) * Time.deltaTime, 
                              (globalSpace ? Space.World : Space.Self));
    }
}
