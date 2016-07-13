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
	}

    void OnDestroy()
    {
        if(rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
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
