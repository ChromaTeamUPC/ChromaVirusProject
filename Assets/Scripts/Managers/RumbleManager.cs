using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

public class RumbleManager : MonoBehaviour 
{
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
        public int id;

        public ContinousRumbleInfo(int pl, float weak, float strong, int rumbleId) : base(pl, weak, strong)
        {
            id = rumbleId;
        }
    }

    private List<TemporalRumbleInfo> temporalRumbleList = new List<TemporalRumbleInfo>();
    private Dictionary<int, ContinousRumbleInfo> continousRumbleList = new Dictionary<int, ContinousRumbleInfo>();

    void Awake()
    {
        //Debug.Log("Rumble Manager created");       
    }

    void Start()
    {
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
        }
        //Debug.Log("Rumble Manager destroyed");
    }

    private void GameReset(EventInfo eventInfo)
    {
        temporalRumbleList.Clear();
        continousRumbleList.Clear();
        GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
        GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
    }

    // Update is called once per frame
    void Update () 
	{
        /*if (temporalRumbleList.Count == 0 && continousRumbleList.Count == 0)
        {
            GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
            GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
        }
        else*/
        if (temporalRumbleList.Count > 0 || continousRumbleList.Count > 0)
        {
            float p1Weak = 0f;
            float p1Strong = 0f;
            float p2Weak = 0f;
            float p2Strong = 0f;


            for (int i = temporalRumbleList.Count - 1; i >= 0; --i)
            {
                TemporalRumbleInfo rumble = temporalRumbleList[i];
                rumble.Update();

                if(rumble.duration > 0)
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

            foreach(ContinousRumbleInfo rumble in continousRumbleList.Values)
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

            GamePad.SetVibration(PlayerIndex.One, p1Strong, p1Weak);
            GamePad.SetVibration(PlayerIndex.Two, p2Strong, p2Weak);
        }
	}

    public void Rumble(int player = 0, float duration = 0.5f, float weakMotor = 1f, float strongMotor = 1f, float startFading = -1f)
    {
        //Player 0 means both players
        TemporalRumbleInfo rumble = new TemporalRumbleInfo(player, weakMotor, strongMotor, duration, (startFading == -1f ? duration * 0.75f : startFading));
        temporalRumbleList.Add(rumble);
    }

    public void AddContinousRumble(int rumbleId, int player = 0, float weakMotor = 1f, float strongMotor = 1f)
    {
        if(!continousRumbleList.ContainsKey(rumbleId))
        {
            ContinousRumbleInfo rumble = new ContinousRumbleInfo(player, weakMotor, strongMotor, rumbleId);
            continousRumbleList.Add(rumbleId, rumble);
        }
    }

    public void RemoveContinousRumble(int rumbleId)
    {
        continousRumbleList.Remove(rumbleId);

        //If that was the last rumble, stop all controllers
        if(temporalRumbleList.Count == 0 && continousRumbleList.Count == 0)
        {
            GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
            GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
        }
    }
}
