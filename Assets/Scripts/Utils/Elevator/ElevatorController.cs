using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            other.transform.SetParent(transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            other.transform.SetParent(null);
        }
    }
}
