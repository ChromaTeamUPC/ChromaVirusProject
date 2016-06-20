using UnityEngine;
using System.Collections;

public class CapacitorDetectingPlayer : MonoBehaviour {

    public CapacitorController controller;

	void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetCapacitor(controller);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetCapacitor(null);
        }
    }
}
