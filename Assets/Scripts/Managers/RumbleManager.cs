using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;
using InControl;

public enum RumbleType
{
    TEST,
    PLAYER_SHOOT,
    PLAYER_DASH,
    PLAYER_USB,
    PLAYER_DISINFECT,
    PLAYER_DYING
}

public class RumbleManager : MonoBehaviour 
{
    [HideInInspector]
    public bool active;

    private bool pauseRumble;

    private class RumbleInfo
    {
        public int player = 0;
        public float weakMotorPower = 0f;
        public float strongMotorPower = 0f;

        public RumbleInfo(int pl, float weak, float strong)
        {
            player = pl;
            weakMotorPower = weak;
            strongMotorPower = strong;
        }

        public virtual float GetMaxWeakValue(float reference)
        {
            return Mathf.Max(weakMotorPower, reference);
        }

        public virtual float GetMaxStrongValue(float reference)
        {
            return Mathf.Max(strongMotorPower, reference);
        }
    }

    private class TemporalRumbleInfo : RumbleInfo
    {
        public float duration = 0f;
        public float startFadingTime = 0f;

        private float powerFactor;

        public TemporalRumbleInfo(int pl, float weak, float strong, float dur, float fade): base (pl, weak, strong)
        {
            duration = dur;
            startFadingTime = fade;
            if (startFadingTime <= 0)
                startFadingTime = 0.1f;
        }

        public void Update()
        {
            duration -= Time.deltaTime;
            powerFactor = Mathf.Clamp(duration / startFadingTime, 0f, 1f);
        }

        public override float GetMaxWeakValue(float reference)
        {
            return Mathf.Max(weakMotorPower * powerFactor, reference);
        }

        public override float GetMaxStrongValue(float reference)
        {
            return Mathf.Max(strongMotorPower * powerFactor, reference);
        }
    }

    private class ContinousRumbleInfo : RumbleInfo
    {
        public RumbleType id;

        public ContinousRumbleInfo(int pl, float weak, float strong, RumbleType rumbleId) : base(pl, weak, strong)
        {
            id = rumbleId;
        }
    }

    private List<TemporalRumbleInfo> temporalRumbleList = new List<TemporalRumbleInfo>();
    private Dictionary<RumbleType, ContinousRumbleInfo> continousRumbleList = new Dictionary<RumbleType, ContinousRumbleInfo>();

    void Awake()
    {
        active = true;
        //Debug.Log("Rumble Manager created");       
    }

    void Start()
    {
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, ClearAllRumbles);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_LOADED, ClearAllRumbles);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_UNLOADED, ClearAllRumbles);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_CLOSED, TutorialClosed);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, ClearAllRumbles);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_LOADED, ClearAllRumbles);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_UNLOADED, ClearAllRumbles);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_CLOSED, TutorialClosed);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        }
        //Debug.Log("Rumble Manager destroyed");
    }

    private void ClearAllRumbles(EventInfo eventInfo)
    {
        pauseRumble = false;
        temporalRumbleList.Clear();
        continousRumbleList.Clear();

        //GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
        //GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
        if (InputManager.Devices.Count >= 1) InputManager.Devices[0].Vibrate(0f, 0f);
        if (InputManager.Devices.Count >= 2) InputManager.Devices[1].Vibrate(0f, 0f);
    }

    private void TutorialOpened(EventInfo eventInfo)
    {
        pauseRumble = true;
    }

    private void TutorialClosed(EventInfo eventInfo)
    {
        pauseRumble = false;
    }

    private void GamePaused(EventInfo eventInfo)
    {
        pauseRumble = true;
    }

    private void GameResumed(EventInfo eventInfo)
    {
        pauseRumble = false;
    }

    // Update is called once per frame
    void Update () 
	{
        if (temporalRumbleList.Count > 0 || continousRumbleList.Count > 0)
        {
            float p1Weak = 0f;
            float p1Strong = 0f;
            float p2Weak = 0f;
            float p2Strong = 0f;

            if (!pauseRumble)
            {
                for (int i = temporalRumbleList.Count - 1; i >= 0; --i)
                {
                    TemporalRumbleInfo rumble = temporalRumbleList[i];
                    rumble.Update();

                    if (rumble.duration > 0)
                    {
                        switch (rumble.player)
                        {
                            case 0:
                                p1Weak = rumble.GetMaxWeakValue(p1Weak);
                                p1Strong = rumble.GetMaxStrongValue(p1Strong);

                                p2Weak = rumble.GetMaxWeakValue(p2Weak);
                                p2Strong = rumble.GetMaxStrongValue(p2Strong);
                                break;

                            case 1:
                                p1Weak = rumble.GetMaxWeakValue(p1Weak);
                                p1Strong = rumble.GetMaxStrongValue(p1Strong);
                                break;

                            case 2:
                                p2Weak = rumble.GetMaxWeakValue(p2Weak);
                                p2Strong = rumble.GetMaxStrongValue(p2Strong);
                                break;
                        }
                    }
                    else
                    {
                        temporalRumbleList.RemoveAt(i);
                    }
                }


                foreach (ContinousRumbleInfo rumble in continousRumbleList.Values)
                {
                    switch (rumble.player)
                    {
                        case 0:
                            p1Weak = rumble.GetMaxWeakValue(p1Weak);
                            p1Strong = rumble.GetMaxStrongValue(p1Strong);

                            p2Weak = rumble.GetMaxWeakValue(p2Weak);
                            p2Strong = rumble.GetMaxStrongValue(p2Strong);
                            break;

                        case 1:
                            p1Weak = rumble.GetMaxWeakValue(p1Weak);
                            p1Strong = rumble.GetMaxStrongValue(p1Strong);
                            break;

                        case 2:
                            p2Weak = rumble.GetMaxWeakValue(p2Weak);
                            p2Strong = rumble.GetMaxStrongValue(p2Strong);
                            break;
                    }
                }
            }

            //GamePad.SetVibration(PlayerIndex.One, p1Strong, p1Weak);
            //GamePad.SetVibration(PlayerIndex.Two, p2Strong, p2Weak);

            if (ShouldVibratePlayer(1)) InputManager.Devices[0].Vibrate(p1Strong, p1Weak);
            if (ShouldVibratePlayer(2)) InputManager.Devices[1].Vibrate(p2Strong, p2Weak);
        }
	}

    private bool ShouldVibratePlayer(int playerId)
    {
        if (playerId == 1)
        {
            return (InputManager.Devices.Count >= 1 && rsc.gameInfo.player1Controller.IsPlaying);
        }
        else if (playerId == 2)
        {
            return (rsc.gameInfo.numberOfPlayers == 2 && InputManager.Devices.Count >= 2 && rsc.gameInfo.player2Controller.IsPlaying);
        }

        return false;
    }

    public void Rumble(int player = 0, float duration = 0.5f, float weakMotor = 1f, float strongMotor = 1f, float startFading = -1f)
    {
        if (!active) return;

        //Player 0 means both players
        TemporalRumbleInfo rumble = new TemporalRumbleInfo(player, weakMotor, strongMotor, duration, (startFading == -1f ? duration * 0.75f : startFading));
        temporalRumbleList.Add(rumble);
    }

    public void AddContinousRumble(RumbleType rumbleId, int player = 0, float weakMotor = 1f, float strongMotor = 1f)
    {
        if (!active) return;

        if (!continousRumbleList.ContainsKey(rumbleId))
        {
            ContinousRumbleInfo rumble = new ContinousRumbleInfo(player, weakMotor, strongMotor, rumbleId);
            continousRumbleList.Add(rumbleId, rumble);
        }
    }

    public void RemoveContinousRumble(RumbleType rumbleId)
    {
        continousRumbleList.Remove(rumbleId);

        //If that was the last rumble, stop all controllers
        if(temporalRumbleList.Count == 0 && continousRumbleList.Count == 0)
        {
            //GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
            //GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
            if (InputManager.Devices.Count >= 1) InputManager.Devices[0].Vibrate(0f, 0f);
            if (InputManager.Devices.Count >= 2) InputManager.Devices[1].Vibrate(0f, 0f);
        }
    }
}
