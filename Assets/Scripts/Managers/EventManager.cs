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
    public bool specialKill;
    public int phase; //Only relevant to boss
}

public class WormEventInfo : EventInfo
{
    public static WormEventInfo eventInfo = new WormEventInfo();

    public WormBlackboard wormBb;
}

public class LevelEventInfo : EventInfo
{
    public static LevelEventInfo eventInfo = new LevelEventInfo();
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

public class TutorialEventInfo : EventInfo
{
    public static TutorialEventInfo eventInfo = new TutorialEventInfo();

    public TutorialManager.Type type;
}

public class CutSceneEventInfo : EventInfo
{
    public static CutSceneEventInfo eventInfo = new CutSceneEventInfo();

    public bool skippeable;
}

public class ComboEventInfo: EventInfo
{
    public static ComboEventInfo eventInfo = new ComboEventInfo();

    public int playerId;
    public ChromaColor comboColor;
    public int comboAdd;
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

public class MeteorAttackEventInfo: EventInfo
{
    public static MeteorAttackEventInfo eventInfo = new MeteorAttackEventInfo();

    public int meteorInitialBurst = 20;
    public float meteorRainDuration = 5f;
    public float meteorInterval = 0.2f;
    public int meteorsPerInterval = 5;

    public int meteorWaitTime = 0;
    public float meteorWarningTime = 2f;
}

public class FadeCurtainEventInfo: EventInfo
{
    public static FadeCurtainEventInfo eventInfo = new FadeCurtainEventInfo();

    public bool fadeIn;
    public bool useDefaultTime;
    public float fadeTime;
    public bool useDefaultColor;
    public Color fadeColor;
}


public class EventManager : MonoBehaviour {

    public enum EventType
    {
        GAME_RESET,
        GAME_PAUSED,
        GAME_RESUMED,
        GAME_FINISHED,
        GAME_OVER,

        LEVEL_LOADED,
        LEVEL_STARTED,
        LEVEL_CLEARED,
        LEVEL_UNLOADED,

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

        KILL_ENEMIES,

        ENEMY_SPAWNED,
        ENEMY_DIED,

        TURRET_SPAWNED,
        TURRET_DESTROYED,

        VORTEX_ACTIVATED,
        VORTEX_DESTROYED,

        WORM_SPAWNED,
        WORM_DYING,
        WORM_DIED,

        WORM_VULNERABLE,
        WORM_INVULNERABLE,

        WORM_PHASE_ACTIVATED,
        WORM_PHASE_ENDED,

        WORM_HEAD_CHARGED,
        WORM_HEAD_DISCHARGED,

        WORM_HEAD_ACTIVATED,
        WORM_HEAD_STUNNED,
        WORM_HEAD_DEACTIVATED,

        WORM_SECTION_ACTIVATED,
        WORM_SECTION_COLOR_CHANGED,
        WORM_SECTION_DESTROYED,

        WORM_ATTACK,

        METEOR_RAIN_START,
        METEOR_RAIN_STARTED,
        METEOR_RAIN_ENDED,

        ZONE_REACHED,
        ZONE_WAVES_FINISHED,
        ZONE_PLAN_FINISHED,

        COLOR_WILL_CHANGE,
        COLOR_CHANGED,

        COMBO_ADD,
        COMBO_BREAK,

        SHOW_SCORE,
        HIDE_SCORE,
        SCORE_OPENING,
        SCORE_OPENED,
        SCORE_CLOSED,

        SHOW_TUTORIAL,
        HIDE_TUTORIAL,
        TUTORIAL_OPENED,
        TUTORIAL_CLOSED,

        START_CUT_SCENE,
        CAMERA_ANIMATION_ENDED,
        CAMERA_CHANGED,

        DEVICE_ACTIVATED,
        DEVICE_DEACTIVATED,
        DEVICE_INFECTION_LEVEL_CHANGED,

        BUTTON_HINT,
        FADE_CURTAIN,
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
