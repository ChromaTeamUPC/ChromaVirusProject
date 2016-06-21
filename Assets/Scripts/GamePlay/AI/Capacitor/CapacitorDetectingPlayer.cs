using UnityEngine;
using System.Collections;

public class CapacitorDetectingPlayer : MonoBehaviour {

    public CapacitorController controller;

    private PlayerController p1;
    private PlayerController p2;

    void Awake()
    {
        p1 = null;
        p2 = null;
    }

    void OnDisable()
    {
        if (p1 != null)
            p1.SetCapacitor(null);

        if (p2 != null)
            p2.SetCapacitor(null);
    }

	void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetCapacitor(controller);

            if (player.Id == 1)
                p1 = player;
            else
                p2 = player;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetCapacitor(null);

            if (player.Id == 1)
                p1 = null;
            else
                p2 = null;
        }
    }
}
