using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ZoneActivableObjects
{
    [Header("Zone info")]
    public int zoneId;

    [Header("Misc objects")]
    public Transform playerSpawnPoint;

    [Header("To handle on zone reach")]
    public BridgeController[] deactivableBridges;
    public VortexController[] activableSpawners;
    public CapacitorController[] activableCapacitors;
    public DeviceController[] activableDevices;

    [Header("To handle on zone finished")]
    public BridgeController[] activableBridges;
    public GameObject[] uSBTeleports;
    public GameObject endZone;

    public void ZoneReached()
    {
        for (int i = 0; i < deactivableBridges.Length; i++)
        {
            deactivableBridges[i].Deactivate();
        }

        for (int i = 0; i < activableSpawners.Length; i++)
        {
            activableSpawners[i].Activate();
        }

        for (int i = 0; i < activableCapacitors.Length; i++)
        {
            activableCapacitors[i].Activate();
        }

        for (int i = 0; i < activableDevices.Length; i++)
        {
            activableDevices[i].Activate();
        }
    }

    public void ZoneFinished()
    {
        for (int i = 0; i < activableBridges.Length; i++)
        {
            activableBridges[i].Activate();
        }

        for (int i = 0; i < uSBTeleports.Length; i++)
        {
            uSBTeleports[i].SetActive(true);
        }

        if (endZone != null)
            endZone.SetActive(true);
    }
}

public class Level01Controller : MonoBehaviour 
{
    public ZoneActivableObjects[] zoneActivableObjects;

    private float respawnDelay = 2f;
    public Transform player1StartPoint;
    public Transform player2StartPoint;

    public FloorController floor;

    [SerializeField]
    private FadeSceneScript fadeScript;

    [SerializeField]
    private Animator elevatorAnimator;

    private int currentZoneId;
    private ZoneActivableObjects currentZoneObjects;

    private Dictionary<int, ZoneActivableObjects> zoneDictionary;

	// Use this for initialization
	void Start () 
    {
        rsc.camerasMng.ChangeCamera(1);

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);

        //rsc.colorMng.Activate();

        if (rsc.gameInfo.player1Controller.Active)
        {
            rsc.gameInfo.player1.transform.position = player1StartPoint.position;
            rsc.gameInfo.player1.transform.SetParent(null);
            if (!rsc.gameInfo.player1.activeSelf)
                rsc.gameInfo.player1.SetActive(true);
            rsc.gameInfo.player1Controller.Spawn();
        }
        if (rsc.gameInfo.numberOfPlayers == 2 && rsc.gameInfo.player2Controller.Active)
        {
            rsc.gameInfo.player2.transform.position = player2StartPoint.position;
            rsc.gameInfo.player2.transform.SetParent(null);
            if (!rsc.gameInfo.player2.activeSelf)
                rsc.gameInfo.player2.SetActive(true);
            rsc.gameInfo.player2Controller.Spawn();
        }

        rsc.eventMng.StartListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_REACHED, ZoneReached);
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZonePlanFinished);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerOutOfZone);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameFinished);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameFinished);


        fadeScript.StartFadingToClear();
        rsc.audioMng.FadeInMainMusic();

        zoneDictionary = new Dictionary<int, ZoneActivableObjects>();
        for (int i = 0; i < zoneActivableObjects.Length; ++i)
        {
            zoneDictionary.Add(zoneActivableObjects[i].zoneId, zoneActivableObjects[i]);
        }
	}

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_REACHED, ZoneReached);
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZonePlanFinished);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerOutOfZone);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameFinished);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameFinished);
        }
    }

    private void CameraEnded(EventInfo eventInfo)
    {
        rsc.camerasMng.ChangeCamera(0);
        elevatorAnimator.SetFloat("AnimationSpeed", 1f);
    }

    private void ZoneReached(EventInfo eventInfo)
    {
        ZoneReachedInfo info = (ZoneReachedInfo)eventInfo;
        currentZoneId = info.zoneId;
        currentZoneObjects = zoneDictionary[currentZoneId];

        rsc.enemyMng.StartPlan(currentZoneId); //Call to enemy manager start plan BEFORE activating any vortex or turret, because start plan resets counters

        //Special actions on first zone
        if(currentZoneId == 101)
        {
            if (rsc.gameInfo.player1Controller.Active)
            {
                rsc.gameInfo.player1Controller.GoToIdle();
            }
            if (rsc.gameInfo.numberOfPlayers == 2 && rsc.gameInfo.player2Controller.Active)
            {
                rsc.gameInfo.player2Controller.GoToIdle();
            }
            rsc.colorMng.Activate();
            floor.Activate();
        }

        currentZoneObjects.ZoneReached();
    }

    private void ZonePlanFinished(EventInfo eventInfo)
    {
        ZonePlanEndedInfo info = (ZonePlanEndedInfo)eventInfo;

        ZoneActivableObjects zone;
        if (zoneDictionary.TryGetValue(info.planId, out zone))
            zone.ZoneFinished();
    }

    //This function will reposition player in the battleground in case he fell and god mode was active
    private void PlayerOutOfZone(EventInfo eventInfo)
    {
        //Get player
        PlayerController player = ((PlayerEventInfo)eventInfo).player;

        PositionPlayer(player);
    }

    private void PlayerDied(EventInfo eventInfo)
    {
        //Get player
        PlayerController player = ((PlayerEventInfo)eventInfo).player;

        player.gameObject.transform.parent = null;
        player.gameObject.SetActive(false);

        //Check player remaining lives
        if (player.Lives > 0)
        {
            //Reset player health, energy and state
            //spawn player in last activated zone
            StartCoroutine(RespawnPlayer(player));
        }
    }

    private IEnumerator RespawnPlayer(PlayerController player)
    {
        yield return new WaitForSeconds(respawnDelay);

        PositionPlayer(player);
        
        if (!player.gameObject.activeSelf)
            player.gameObject.SetActive(true);

        player.Spawn(true);
        player.GoToIdle();
    }

    private void PositionPlayer(PlayerController player)
    {
        if (currentZoneObjects != null)
            player.transform.position = currentZoneObjects.playerSpawnPoint.position;
        else
            player.transform.position = zoneActivableObjects[0].playerSpawnPoint.position;

        player.transform.SetParent(null);
    }

    private void GameFinished(EventInfo eventInfo)
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(1.5f);

        fadeScript.StartFadingToColor(1.5f);
    }
}
