using UnityEngine;
using System.Collections;

public class WormHeadController : MonoBehaviour 
{
    private float fTurnRate = 90.0f;  // 90 degrees of turning per second
    private float fSpeed = 1.0f;  // Units per second of movement;

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            fSpeed += 0.1f;

        if (Input.GetKey(KeyCode.DownArrow))
        {
            fSpeed -= 0.1f;
            if (fSpeed < 0f)
                fSpeed = 0f;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(-Vector3.up * fTurnRate * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(Vector3.up * fTurnRate * Time.deltaTime);

        transform.position = transform.position + transform.forward * fSpeed * Time.deltaTime;
    }
}
