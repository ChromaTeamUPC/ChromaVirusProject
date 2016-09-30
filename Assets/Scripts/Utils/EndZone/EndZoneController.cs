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
            if (!triggered)
            {
                rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_CLEARED, EventInfo.emptyInfo);
                triggered = true;
            }
        }
    }
}
