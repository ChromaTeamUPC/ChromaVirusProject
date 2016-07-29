using UnityEngine;
using System.Collections;

public class LevelBossController : MonoBehaviour 
{
    private float respawnDelay = 2f;
    public Transform player1StartPoint;
    public Transform player2StartPoint;

    public Transform[] playerSpawnPoint;

    [SerializeField]
    private FadeSceneScript fadeScript;

    // Use this for initialization
    void Start () 
	{
        //Stop color manager
        rsc.colorMng.Deactivate();

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
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerOutOfZone);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameFinished);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameFinished);

        rsc.camerasMng.SetEntryCameraLevelAnimation(-1);

        fadeScript.StartFadingToClear();
        rsc.audioMng.FadeInMainMusic();
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.CAMERA_ANIMATION_ENDED, CameraEnded);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DIED, PlayerDied);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerOutOfZone);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameFinished);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameFinished);
        }
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

        rsc.colorMng.Activate();
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
        player.transform.position = playerSpawnPoint[Random.Range(0, playerSpawnPoint.Length)].position;

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
