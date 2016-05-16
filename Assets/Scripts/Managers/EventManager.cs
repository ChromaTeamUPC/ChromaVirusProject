using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventInfo
{
    //Events that need no extra info, can use this object to improve performance
    public static EventInfo emptyInfo = new EventInfo();
}
public class ColorEventInfo : EventInfo
{
    public static ColorEventInfo eventInfo = new ColorEventInfo();

    public ChromaColor newColor;
}

public class ZoneReachedInfo : EventInfo
{
    public static ZoneReachedInfo eventInfo = new ZoneReachedInfo();

    public int zoneId;
    public string playerTag;
}

public class ZonePlanEndedInfo : EventInfo
{
    public static ZonePlanEndedInfo eventInfo = new ZonePlanEndedInfo();

    public int planId;
}

public class PlayerSpawnedEventInfo : EventInfo
{
    public static PlayerSpawnedEventInfo eventInfo = new PlayerSpawnedEventInfo();

    public PlayerController player;
}

public class PlayerDamagedEventInfo : EventInfo
{
    public static PlayerDamagedEventInfo eventInfo = new PlayerDamagedEventInfo();

    public int damage;
    public int currentHealth;
}

public class PlayerDiedEventInfo : EventInfo
{
    public static PlayerDiedEventInfo eventInfo = new PlayerDiedEventInfo();

    public PlayerController player;
}

public class LevelEventInfo : EventInfo
{
    public static LevelEventInfo eventInfo = new LevelEventInfo();

    public int levelId;
}


public class CameraEventInfo : EventInfo
{
    public Camera newCamera;
}

[System.Serializable]
public class CustomEvent : UnityEvent<EventInfo> { }


public class EventManager : MonoBehaviour {

    public enum EventType
    {
        GAME_RESET,
        GAME_PAUSED,
        GAME_RESUMED,
        GAME_FINISHED,
        GAME_OVER,

        LEVEL_STARTED,
        LEVEL_CLEARED,

        PLAYER_SPAWNED,
        PLAYER_DAMAGED,
        PLAYER_DIED,
        PLAYER_WIN,

        ENEMY_SPAWNED,
        ENEMY_DIED,

        TURRET_SPAWNED,
        TURRET_DESTROYED,

        VORTEX_ACTIVATED,
        VORTEX_DESTROYED,

        ZONE_REACHED,
        ZONE_PLAN_FINISHED,

        COLOR_CHANGED,
        CAMERA_CHANGED
    }

    private Dictionary<EventType, CustomEvent> eventDictionary;

    private bool active;

    void Awake()
    {
        Debug.Log("Event Manager created");
        eventDictionary = new Dictionary<EventType, CustomEvent>();
        active = true;
    }

    void OnDestroy()
    {
        Debug.Log("Event Manager destroyed");
    }

    public void Activate()
    {
        active = true;
    }

    public void Deactivate()
    {
        active = false;
    }

    public void StartListening(EventType eventType, UnityAction<EventInfo> listener)
    {
        CustomEvent thisEvent = null;
        if(eventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new CustomEvent();
            thisEvent.AddListener(listener);
            eventDictionary.Add(eventType, thisEvent);
        }
    }

    public void StopListening(EventType eventType, UnityAction<EventInfo> listener)
    {
        CustomEvent thisEvent = null;
        if(eventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public void TriggerEvent(EventType eventType, EventInfo eventInfo)
    {
        if (active)
        {
            CustomEvent thisEvent = null;
            if (eventDictionary.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.Invoke(eventInfo);
            }
        }
    }

}
