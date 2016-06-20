using UnityEngine;
using System.Collections;

public class InfectionManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //Debug.Log("Infection Manager created");
    }

    void OnDestroy()
    {
        //if (rsc.eventMng != null)
        //    rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        //Debug.Log("Infection Manager destroyed");
    }

    // Update is called once per frame
    void Update () {
	
	}
}
