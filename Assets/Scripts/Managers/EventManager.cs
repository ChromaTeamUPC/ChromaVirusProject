using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventInfo
{
    //Events that need no extra info, can use this object to improve performance
    public static EventInfo emptyInfo = new EventInfo();
}

[System.Serializable]
public class CustomEvent : UnityEvent<EventInfo> { }

public class ColorEventInfo : EventInfo
{
    public static ColorEventInfo eventInfo = new ColorEventInfo();

    public ChromaColor oldColor;
    public ChromaColor newColor;
}

public class ColorPrewarnEventInfo : EventInfo
{
    public static ColorPrewarnEventInfo eventInfo = new ColorPrewarnEventInfo();

    public ChromaColor newColor;
    public float prewarnSeconds;
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

public class PlayerEventInfo : EventInfo
{
    public static PlayerEventInfo eventInfo = new PlayerEventInfo();

    public PlayerController player;
}

public class PlayerDamagedEventInfo : EventInfo
{
    public static PlayerDamagedEventInfo eventInfo = new PlayerDamagedEventInfo();

    public float damage;
    public float currentHealth;
}

public class EnemyDiedEventInfo : EventInfo
{
    public static EnemyDiedEventInfo eventInfo = new EnemyDiedEventInfo();

    public ChromaColor color;
    public int infectionValue;
    public PlayerController killerPlayer;
    public bool killedSameColor;
}

public class WormSpawnedEventInfo : EventInfo
{
    public static WormSpawnedEventInfo eventInfo = new WormSpawnedEventInfo();

    public int wormPhases;
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

public class DeviceEventInfo : EventInfo
{
    public static DeviceEventInfo eventInfo = new DeviceEventInfo();

    public DeviceController device;
}

public class CutSceneEventInfo : EventInfo
{
    public static CutSceneEventInfo eventInfo = new CutSceneEventInfo();

    public bool skippeable;
}

public class ButtonHintEventInfo : EventInfo
{
    public enum ButtonType
    {
        A,
        B,
        X,
        Y,
        COLOR_BUTTONS,
        LB,
        LT,
        RB,
        RT
    }

    public static ButtonHintEventInfo eventInfo = new ButtonHintEventInfo();

    public bool show;
    public int playerId;
    public ButtonType buttonType;
}


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

        PLAYER_SPAWNING,
        PLAYER_SPAWNED,
        PLAYER_DAMAGED,
        PLAYER_DASHING,
        PLAYER_DASHED,
        PLAYER_DYING,
        PLAYER_DIED,
        PLAYER_WIN,
        PLAYER_COLOR_MISMATCH,
        PLAYER_COLOR_MISMATCH_START,
        PLAYER_COLOR_MISMATCH_END,
        PLAYER_OUT_OF_ZONE,

        ENEMY_SPAWNED,
        ENEMY_DIED,

        TURRET_SPAWNED,
        TURRET_DESTROYED,

        VORTEX_ACTIVATED,
        VORTEX_DESTROYED,

        WORM_SPAWNED,
        WORM_DIED,

        WORM_HEAD_ACTIVATED,
        WORM_HEAD_DESTROYED,

        WORM_SECTION_ACTIVATED,
        WORM_SECTION_COLOR_CHANGED,
        WORM_SECTION_DESTROYED,

        ZONE_REACHED,
        ZONE_PLAN_FINISHED,

        COLOR_WILL_CHANGE,
        COLOR_CHANGED,

        START_CUT_SCENE,
        CAMERA_ANIMATION_ENDED,
        CAMERA_CHANGED,

        DEVICE_ACTIVATED,
        DEVICE_DEACTIVATED,
        DEVICE_INFECTION_LEVEL_CHANGED,

        BUTTON_HINT,
    }

    private Dictionary<EventType, CustomEvent> eventDictionary;

    private bool active;

    void Awake()
    {
        //Debug.Log("Event Manager created");
        eventDictionary = new Dictionary<EventType, CustomEvent>();
        active = true;
    }

    void OnDestroy()
    {
        //Debug.Log("Event Manager destroyed");
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
