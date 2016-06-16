using UnityEngine;
using System.Collections;

public class RotateParticles : MonoBehaviour {

    private float particlesRotation = 10f;
    public float mprSpeed = 1.75f;
	
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(0, mprSpeed * Time.deltaTime, 0* particlesRotation * Time.deltaTime, Space.World);
	}
}
