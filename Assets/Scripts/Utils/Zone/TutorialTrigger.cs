using UnityEngine;
using System.Collections;

public class TutorialTrigger : MonoBehaviour 
{
    public TutorialManager.Type type;
    public bool triggerOnce = true;
    public bool triggerOnEnter = true;

    private bool triggered;

    void OnEnable()
    {
        triggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                TutorialEventInfo.eventInfo.type = type;
                rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
            }

            triggered = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (triggerOnEnter) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                TutorialEventInfo.eventInfo.type = type;
                rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
            }

            triggered = true;
        }
    }
}
