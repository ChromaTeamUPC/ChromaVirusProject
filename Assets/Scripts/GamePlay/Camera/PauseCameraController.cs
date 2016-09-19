using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(BlurOptimized))]
public class PauseCameraController : MonoBehaviour 
{
    private BlurOptimized blur;

	void Start () 
	{
        blur = GetComponent<BlurOptimized>();
        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_CLOSED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.SCORE_OPENING, GamePaused);
        //rsc.eventMng.StartListening(EventManager.EventType.SCORE_CLOSED, GameResumed);
    }

    void OnDestroy()
    {
        if(rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_CLOSED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.SCORE_OPENING, GamePaused);
            //rsc.eventMng.StopListening(EventManager.EventType.SCORE_CLOSED, GameResumed);
        }
    }
	
	private void GamePaused(EventInfo eventInfo)
    {
        blur.enabled = true;
    }

    private void GameResumed(EventInfo evetInfo)
    {
        blur.enabled = false;
    }
}
