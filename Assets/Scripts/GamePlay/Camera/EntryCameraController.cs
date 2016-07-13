using UnityEngine;
using System.Collections;
using InControl;

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
        if (rsc.gameMng.State == GameManager.GameState.STARTED)
        {        
            if (InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2))
            {
                skipped = true;
                rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
            }
        }
    }
}
