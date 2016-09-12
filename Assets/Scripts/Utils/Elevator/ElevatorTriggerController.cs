using UnityEngine;
using System.Collections;

public class ElevatorTriggerController : MonoBehaviour {

    private PlayerController player1;
    private PlayerController player2;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            other.transform.SetParent(transform);
            if (other.tag == "Player1")
                player1 = other.GetComponent<PlayerController>();
            else
                player2 = other.GetComponent<PlayerController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            other.transform.SetParent(null);
            if (other.tag == "Player1")
            {
                player1.forceMovementEnabled = false;
                player1 = null;
            }
            else
            {
                player2.forceMovementEnabled = false;
                player2 = null;
            }
        }
    }
    
    void Update()
    {
        if (player1 != null)
            player1.ForcePositionUpdate();

        if (player2 != null)
            player2.ForcePositionUpdate();
    }   
}
