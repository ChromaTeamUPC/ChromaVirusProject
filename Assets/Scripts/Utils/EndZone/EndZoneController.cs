using UnityEngine;
using System.Collections;

public class EndZoneController : MonoBehaviour {

    public int levelId;
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
                LevelEventInfo.eventInfo.levelId = levelId;
                rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_CLEARED, LevelEventInfo.eventInfo);
                triggered = true;
            }
        }
    }
}
