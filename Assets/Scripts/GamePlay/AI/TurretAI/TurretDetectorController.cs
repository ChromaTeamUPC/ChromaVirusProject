using UnityEngine;
using System.Collections;

public class TurretDetectorController : MonoBehaviour 
{
    private TurretAIBehaviour turret;

	void Awake () 
	{
        turret = GetComponentInParent<TurretAIBehaviour>();
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1")
        {
            turret.player1 = other.GetComponent<PlayerController>();
            //Debug.Log("Player 1 entered turret range");
        }

        if (other.tag == "Player2")
        {
            turret.player2 = other.GetComponent<PlayerController>();
            //Debug.Log("Player 2 entered turret range");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1")
        {
            turret.player1 = null;
            //Debug.Log("Player 1 exit turret range");
        }

        if (other.tag == "Player2")
        {
            turret.player2 = null;
            //Debug.Log("Player 2 exit turret range");
        }
    }
}
