using UnityEngine;
using System.Collections;

public class USBTeleportController : MonoBehaviour {

    public Transform destiny;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            other.transform.position = destiny.position;
        }
    }
}
