using UnityEngine;
using System.Collections;

public class ZoneTrigger : MonoBehaviour {

    public int zoneId;
    public bool triggerOnce = true;
    public bool triggerOnEnter = true;

    public bool teleportOtherPlayer = false;
    public Transform teleportDestiny;

    private bool triggered;

    void OnEnable()
    {
        triggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                ZoneReachedInfo.eventInfo.zoneId = zoneId;
                ZoneReachedInfo.eventInfo.playerTag = other.tag;
                rsc.eventMng.TriggerEvent(EventManager.EventType.ZONE_REACHED, ZoneReachedInfo.eventInfo);

                if(rsc.gameInfo.numberOfPlayers == 2 && teleportOtherPlayer)
                {
                    PlayerController player = other.GetComponent<PlayerController>();
                    PlayerController otherPlayer;
                    if(player.Id == 1)
                    {
                        otherPlayer = rsc.gameInfo.player2Controller;
                    }
                    else
                    {
                        otherPlayer = rsc.gameInfo.player1Controller;
                    }

                    if(otherPlayer.ActiveAndAlive &&
                        !otherPlayer.IsFalling &&
                        !otherPlayer.IsDying)
                    {
                        otherPlayer.transform.position = teleportDestiny.position;
                    }
                }
            }

            triggered = true;
        }       
    }

    void OnTriggerExit(Collider other)
    {
        if (triggerOnEnter) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!triggerOnce || !triggered)
            {
                ZoneReachedInfo.eventInfo.zoneId = zoneId;
                ZoneReachedInfo.eventInfo.playerTag = other.tag;
                rsc.eventMng.TriggerEvent(EventManager.EventType.ZONE_REACHED, ZoneReachedInfo.eventInfo);
            }

            triggered = true;
        }
    }
}
