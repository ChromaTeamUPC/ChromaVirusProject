using UnityEngine;
using System.Collections;

public class LevelBossController : MonoBehaviour 
{
    public Transform mainCameraStartPoint;

    private float respawnDelay = 2f;
    public Transform singlePlayerStartPoint;
    public Transform player1StartPoint;
    public Transform player2StartPoint;

    public Transform[] playerSpawnPoint;

    public BossFloorController floor;

    public GameObject sceneCenter;
    //private HexagonController sceneCenterHexagon;

    public WormBlackboard worm;

    // Use this for initialization
    void Start () 
	{
        rsc.gameMng.CurrentLevel = GameManager.Level.LEVEL_BOSS;
        //Ensure all resources are in place (ie, enemies back to pool)
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_LOADED, EventInfo.emptyInfo);

        rsc.camerasMng.PositionCamera(0, mainCameraStartPoint);
        rsc.camerasMng.ChangeCamera(1);    

        if (rsc.gameInfo.player1Controller.Active)
        {
            rsc.gameInfo.player1.transform.position = singlePlayerStartPoint.position;
            rsc.gameInfo.player1.transform.rotation = singlePlayerStartPoint.rotation;
            rsc.gameInfo.player1.transform.SetParent(null);
            if (!rsc.gameInfo.player1.activeSelf)
                rsc.gameInfo.player1.SetActive(true);
            rsc.gameInfo.player1Controller.Spawn();
        }
        if (rsc.gameInfo.numberOfPlayers == 2 && rsc.gameInfo.player2Controller.Active)
        {
            rsc.gameInfo.player1.transform.position = player1StartPoint.position;

            rsc.gameInfo.player2.transform.position = player2StartPoint.position;
            rsc.gameInfo.player2.transform.rotation = player2StartPoint.rotation;
            rsc.gameInfo.player2.transform.SetParent(null);
            if (!rsc.gameInfo.player2.activeSelf)
                rsc.gameInfo.player2.SetActive(true);
            rsc.gameInfo.player2Controller.Spawn();
        }

        rsc.eventMng.StartListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerOutOfZone);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_PHASE_ENDED, WormPhaseEnded);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_DYING, WormDying);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_DIED, WormDied);     

        rsc.camerasMng.SetEntryCameraLevelAnimation(-1);

        FadeCurtainEventInfo.eventInfo.fadeIn = true;
        FadeCurtainEventInfo.eventInfo.useDefaultColor = true;
        FadeCurtainEventInfo.eventInfo.useDefaultTime = true;
        rsc.eventMng.TriggerEvent(EventManager.EventType.FADE_CURTAIN, FadeCurtainEventInfo.eventInfo);
        rsc.audioMng.FadeInMusic(AudioManager.MusicType.LEVEL_BOSS_01);

        StartCoroutine(InitWorm());
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerOutOfZone);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_PHASE_ENDED, WormPhaseEnded);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_DYING, WormDying);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_DIED, WormDied);
        }
    }

    private IEnumerator InitWorm()
    {
        yield return null; //Wait 1 frame to allow every module Start method to be called

        rsc.colorMng.Activate();
        floor.Activate();

        worm.Init(sceneCenter);
    }

    private void CameraEnded(EventInfo eventInfo)
    {
        rsc.camerasMng.SetMainCameraPositionToEntryCameraPosition();
        rsc.camerasMng.ChangeCamera(0);

        if (rsc.gameInfo.player1Controller.Active)
        {
            rsc.gameInfo.player1Controller.GoToIdle();
        }
        if (rsc.gameInfo.numberOfPlayers == 2 && rsc.gameInfo.player2Controller.Active)
        {
            rsc.gameInfo.player2Controller.GoToIdle();
        }

        //rsc.colorMng.Activate();
        //floor.Activate();
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_STARTED, EventInfo.emptyInfo);
    }

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
        if(rsc.gameInfo.numberOfPlayers == 1)
        {
            player.transform.position = singlePlayerStartPoint.position;
            player.transform.rotation = singlePlayerStartPoint.rotation;
        }
        else
        {
            PlayerController otherPlayer = rsc.gameInfo.player1Controller;
            if(player.Id == 1)
                otherPlayer = rsc.gameInfo.player2Controller;

            if(otherPlayer.ActiveAndAlive)
            {
                Transform spawnPoint = playerSpawnPoint[0];
                float distance = Vector3.Distance(otherPlayer.transform.position, spawnPoint.position);

                for(int i = 1; i < playerSpawnPoint.Length -1; ++i)
                {
                    float newDistance = Vector3.Distance(otherPlayer.transform.position, playerSpawnPoint[i].position);
                    if (newDistance < distance)
                    {
                        spawnPoint = playerSpawnPoint[i];
                        distance = newDistance;
                    }
                }

                player.transform.position = spawnPoint.position;
                player.transform.rotation = otherPlayer.transform.rotation;
            }
            else
            {
                if (player.Id == 1)
                {
                    player.transform.position = player1StartPoint.position;
                    player.transform.rotation = player1StartPoint.rotation;
                }
                else
                {
                    player.transform.position = player2StartPoint.position;
                    player.transform.rotation = player2StartPoint.rotation;
                }
            }
        }

        player.transform.SetParent(null);
    }

    private void WormPhaseEnded(EventInfo eventInfo)
    {
        WormEventInfo info = (WormEventInfo)eventInfo;

        if (info.wormBb.wormCurrentPhase < info.wormBb.wormMaxPhases - 1)
        {
            switch (info.wormBb.wormCurrentPhase)
            {
                case 0:
                    rsc.audioMng.ChangeMusic(AudioManager.MusicType.LEVEL_BOSS_02, 5f);
                    break;

                case 1:
                    rsc.audioMng.ChangeMusic(AudioManager.MusicType.LEVEL_BOSS_03, 5f);
                    break;

                case 2:
                    rsc.audioMng.ChangeMusic(AudioManager.MusicType.LEVEL_BOSS_04, 5f);
                    break;

                default:
                    break;
            }
        }
        else //Last phase
        //if(info.wormBb.wormCurrentPhase == info.wormBb.wormMaxPhases - 1)
        {
            if (rsc.gameInfo.player1Controller.ActiveAndAlive)
            {
                rsc.gameInfo.player1Controller.SetInvulnerable();
            }
            if (rsc.gameInfo.numberOfPlayers == 2 && rsc.gameInfo.player2Controller.ActiveAndAlive)
            {
                rsc.gameInfo.player2Controller.SetInvulnerable();
            }

            rsc.eventMng.TriggerEvent(EventManager.EventType.KILL_ENEMIES, EventInfo.emptyInfo);
        }
    }


    private void WormDying(EventInfo eventInfo)
    {
        /*if (rsc.gameInfo.player1Controller.ActiveAndAlive)
        {
            rsc.gameInfo.player1Controller.SetInvulnerable();
        }
        if (rsc.gameInfo.numberOfPlayers == 2 && rsc.gameInfo.player2Controller.ActiveAndAlive)
        {
            rsc.gameInfo.player2Controller.SetInvulnerable();
        }

        rsc.eventMng.TriggerEvent(EventManager.EventType.KILL_ENEMIES, EventInfo.emptyInfo);*/
    }

    private void WormDied(EventInfo eventInfo)
    { 
        if (rsc.gameInfo.player1Controller.ActiveAndAlive)
        {
            rsc.gameInfo.player1Controller.LevelCleared();
        }
        if (rsc.gameInfo.numberOfPlayers == 2 && rsc.gameInfo.player2Controller.ActiveAndAlive)
        {
            rsc.gameInfo.player2Controller.LevelCleared();
        }
    }
}
