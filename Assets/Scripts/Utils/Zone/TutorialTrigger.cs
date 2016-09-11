using UnityEngine;
using System.Collections;

public class TutorialTrigger : MonoBehaviour 
{ 
    public TutorialManager.Type[] types;
    private int index;
    public bool triggerOnce = true;
    public bool triggerOnEnter = true;

    private bool triggered;

    void OnEnable()
    {
        triggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter || types.Length == 0) return;

        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_CLOSED, TutorialClosed);

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                index = 0;
                TutorialEventInfo.eventInfo.type = types[index];
                rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
            }

            triggered = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (triggerOnEnter || types.Length == 0) return;

        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_CLOSED, TutorialClosed);

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                index = 0;
                TutorialEventInfo.eventInfo.type = types[index];
                rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
            }

            triggered = true;
        }
    }

    private void TutorialClosed(EventInfo eventInfo)
    {
        index++;

        if(index < types.Length)
        {
            TutorialEventInfo.eventInfo.type = types[index];
            rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
        }
        else
        {
            StartCoroutine(StopListening());
        }
    }

    private IEnumerator StopListening()
    {
        yield return null;
        rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_CLOSED, TutorialClosed);
    }
}
