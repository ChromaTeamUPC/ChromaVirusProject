using UnityEngine;
using System.Collections;

public class EntryCameraController : MonoBehaviour 
{
    private bool skipped = false;

	public void AnimationEnded()
    {
        if(!skipped)
            rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
    }

    void Update()
    {
        if((Input.GetButtonDown("P1_Red") || Input.GetButtonDown("P2_Red")) && !skipped)
        {
            skipped = true;
            rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
        }
    }
}
