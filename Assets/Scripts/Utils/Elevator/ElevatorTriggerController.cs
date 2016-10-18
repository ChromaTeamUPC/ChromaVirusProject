using UnityEngine;
using System.Collections;

public class ElevatorTriggerController : MonoBehaviour {

    private PlayerController player1;
    private bool firstTimePlayer1 = true;
    private PlayerController player2;
    private bool firstTimePlayer2 = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (other.tag == "Player1")
            {
                if (firstTimePlayer1)
                {
                    other.transform.SetParent(transform);
                    player1 = other.GetComponent<PlayerController>();
                    firstTimePlayer1 = false;
                }
            }
            else
            {
                if (firstTimePlayer2)
                {
                    other.transform.SetParent(transform);
                    player2 = other.GetComponent<PlayerController>();
                    firstTimePlayer2 = false;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {           
            if (other.tag == "Player1")
            {
                if (player1 != null)
                {
                    other.transform.SetParent(null);
                    player1.forceMovementEnabled = false;
                    player1 = null;
                }
            }
            else
            {
                if (player2 != null)
                {
                    other.transform.SetParent(null);
                    player2.forceMovementEnabled = false;
                    player2 = null;
                }
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
