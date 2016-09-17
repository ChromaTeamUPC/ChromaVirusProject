using UnityEngine;
using System.Collections;

public class EndZoneController : MonoBehaviour {

    public Transform player1EndPoint;
    public Transform player2EndPoint;

    private bool triggered;

    void OnEnable()
    {
        triggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            Vector3 destination = transform.position;

            if (rsc.gameInfo.numberOfPlayers == 2)
            {
                switch (player.Id)
                {
                    case 1:
                        destination = player1EndPoint.position;
                        break;

                    case 2:
                        destination = player2EndPoint.position;
                        break;
                }
            }
            player.LevelCleared(destination);

            if (!triggered)
            {
                rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_CLEARED, EventInfo.emptyInfo);
                triggered = true;
            }
        }
    }
}
