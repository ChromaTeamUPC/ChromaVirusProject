using UnityEngine;
using System.Collections;

public class CoroutineHelper : MonoBehaviour {

    void Start()
    {
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
        if(rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
        }
    }

    private void GameReset(EventInfo eventInfo)
    {
        StopAllCoroutines();
    }

	public void StartCoroutineHelp(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void StopCoroutineHelp(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }
}
