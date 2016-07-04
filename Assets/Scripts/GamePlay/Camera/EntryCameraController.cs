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
        int ctrlNumber = 0;

        while (!skipped && ctrlNumber < rsc.gameInfo.numberOfPlayers)
        {
            if (InputManager.Devices[ctrlNumber].Action2.WasPressed)
            {
                skipped = true;
                rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
            }

            ++ctrlNumber;
        }
    }
}
