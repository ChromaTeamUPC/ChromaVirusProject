using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ResourceAssurer : MonoBehaviour {

    [Range(1,2)]
    public int numberOfPlayers = 1;
    public bool tutorial = true;
    [Range(-80,0)]
    public int musicVol = 0;
    [Range(-80, 0)]
    public int fxVol = 0;

    private bool resourcesForced;

    void Awake()
    {
        //Check resources are initialized
        //They should be when playing normal, but they may not when testing and loading this scene from editor
        if (!rsc.ObjectsInitialized)
        {
            resourcesForced = true;
            SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);
        } 
        else
        {
            resourcesForced = false;
        }     
    }

	// Use this for initialization
	void Start ()
    {
	    if(resourcesForced)
        {
            rsc.tutorialMng.active = tutorial;
            rsc.audioMng.audioMixer.SetFloat("MusicVolume", musicVol);
            rsc.audioMng.audioMixer.SetFloat("FxVolume", fxVol);
            rsc.gameMng.InitPlayers(numberOfPlayers);
            rsc.gameMng.SetGameStartedDEBUG();
        }
	}
}
