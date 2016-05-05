using UnityEngine;
using System.Collections;

public class GodCameraScript : MonoBehaviour {

    public int speed = 10;
   
    [HideInInspector]
    public Camera godCamera;

    public float minY = -60.0f;
    public float maxY = 60.0f;

    public float sens = 100.0f;

    float rotationY = 0.0f;
    float rotationX = 0.0f;

    private DebugKeys keys;

    void Start()
    {
        keys = rsc.debugMng.keys;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(keys.godCameraRight))
        {
            Vector3 displacement = transform.right * Time.deltaTime * speed;
            transform.position = transform.position + displacement;
        }
        else if (Input.GetKey(keys.godCameraLeft))
        {
            Vector3 displacement = transform.right * Time.deltaTime * speed * -1;
            transform.position = transform.position + displacement;
        }
        if (Input.GetKey(keys.godCameraForward))
        {
            Vector3 displacement = transform.forward * Time.deltaTime * speed;
            transform.position = transform.position + displacement;
        }
        else if (Input.GetKey(keys.godCameraBackward))
        {
            Vector3 displacement = transform.forward * Time.deltaTime * speed * -1;
            transform.position = transform.position + displacement;
        }

        // camera rotation with mouse coordinates
        rotationX += Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        rotationY += Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);
        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }
}
