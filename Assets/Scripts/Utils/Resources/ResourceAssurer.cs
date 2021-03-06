﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ResourceAssurer : MonoBehaviour {

    [Range(1,2)]
    public int numberOfPlayers = 1;
    [Range(-80,0)]
    public int musicVol = 0;
    [Range(-80, 0)]
    public int fxVol = 0;

    public bool enableDebug = true;
    public bool godMode = false;
    public bool alwaysKillOk = false;
    public bool showPlayerLimits = false;
    public bool vibration = true;
    public bool tutorial = true;

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
            rsc.debugMng.debugModeEnabled = enableDebug;
            rsc.debugMng.godMode = godMode;
            rsc.debugMng.alwaysKillOk = alwaysKillOk;
            rsc.debugMng.showPlayerLimits = showPlayerLimits;
            rsc.rumbleMng.active = vibration;
            rsc.tutorialMng.Active = tutorial;
            rsc.audioMng.audioMixer.SetFloat("MusicVolume", musicVol);
            rsc.audioMng.audioMixer.SetFloat("FxVolume", fxVol);
            rsc.gameMng.InitPlayers(numberOfPlayers);
            rsc.gameMng.SetGameStartedDEBUG();
        }
	}
}
