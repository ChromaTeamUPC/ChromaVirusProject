using UnityEngine;
using System.Collections;

public class Level01Controller : MonoBehaviour 
{
    private float respawnDelay = 4f;
    public Transform player1StartPoint;
    public Transform player2StartPoint;

    public GameObject bridgeZone01;
    public GameObject bridgeZone02;

    public GameObject uSBTeleport1;   

    public GameObject endZone;

    public Transform zone1PlayerSpawnPoint;
    public Transform zone2PlayerSpawnPoint;
    public Transform zone3PlayerSpawnPoint;

    public FloorController floor;

    public VortexController zone2spawner1;
    public VortexController zone3spawner1;
    public VortexController zone3spawner2;

    [SerializeField]
    private FadeSceneScript fadeScript;

    private int currentZone;

	// Use this for initialization
	void Start () 
    {
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

        rsc.eventMng.StartListening(EventManager.EventType.ZONE_REACHED, ZoneReached);
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZonePlanFinished);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);

        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.GAME_RESET, EventInfo.emptyInfo);

        fadeScript.StartFadingToClear();
        rsc.audioMng.FadeInMainMusic();
	}

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_REACHED, ZoneReached);
            rsc.eventMng.StopListening(EventManager.EventType.ZONE_PLAN_FINISHED, ZonePlanFinished);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        }
    }

    private void ZoneReached(EventInfo eventInfo)
    {
        ZoneReachedInfo info = (ZoneReachedInfo)eventInfo;
        currentZone = info.zoneId;        
        switch (currentZone)
        {
            case 101:
                rsc.colorMng.Activate();
                floor.Activate();
                break;
            case 102:
                zone2spawner1.Activate();
                bridgeZone01.SetActive(false);
                break;
            case 103:
                zone3spawner1.Activate();
                zone3spawner2.Activate();
                bridgeZone02.SetActive(false);
                break;
            default:
                break;
        }
        rsc.enemyMng.StartPlan(currentZone);
    }

    private void ZonePlanFinished(EventInfo eventInfo)
    {
        ZonePlanEndedInfo info = (ZonePlanEndedInfo)eventInfo;
        switch (info.planId)
        {
            //open door 01
            case 101:
                bridgeZone01.SetActive(true);
                break;

            case 102:
                bridgeZone02.SetActive(true);
                uSBTeleport1.SetActive(true);              
                break;

            case 103:
                endZone.SetActive(true);
                break;
        }
    }

    private void PlayerDied(EventInfo eventInfo)
    {
        //Get player
        PlayerController player = ((PlayerDiedEventInfo)eventInfo).player;

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

        switch (currentZone)
        {
            //open door 01
            case 101:
                player.transform.position = zone1PlayerSpawnPoint.position;
                break;

            case 102:
                player.transform.position = zone2PlayerSpawnPoint.position;
                break;

            case 103:
                player.transform.position = zone3PlayerSpawnPoint.position;
                break;
        }
        player.transform.SetParent(null);
        if (!player.gameObject.activeSelf)
            player.gameObject.SetActive(true);

        player.Spawn();
    }
}
