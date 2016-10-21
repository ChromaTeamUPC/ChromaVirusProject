using UnityEngine;
using System.Collections;

public class ShotsMover : MonoBehaviour
{
    public float speed = 70;

	// Update is called once per frame
	void Update ()
    {
	    if(gameObject.transform.localPosition.z < 10)
        {
            gameObject.transform.Translate(0, 0, speed * Time.deltaTime, Space.Self);
        }
        else
        {
            gameObject.transform.localPosition = new Vector3(0, 0, -10);
        }

	}
}
