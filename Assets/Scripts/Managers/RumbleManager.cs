using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

public class RumbleManager : MonoBehaviour 
{
    private class RumbleInfo
    {
        public PlayerIndex player = PlayerIndex.One;
        public float weakMotorPower = 0f;
        public float strongMotorPower = 0f;
        public float duration = 0f;
        public float startFadingTime = 0f;

        private float powerFactor;

        public RumbleInfo(int pl, float weak, float strong, float dur, float fade)
        {
            player = (PlayerIndex)(pl - 1);
            weakMotorPower = weak;
            strongMotorPower = strong;
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

        public float GetMaxWeakValue(float reference)
        {
            return Mathf.Max(weakMotorPower * powerFactor, reference);
        }

        public float GetMaxStrongValue(float reference)
        {
            return Mathf.Max(strongMotorPower * powerFactor, reference);
        }
    }

    private List<RumbleInfo> rumbleList = new List<RumbleInfo>();

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
        rumbleList.Clear();
        GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
        GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
    }

    // Update is called once per frame
    void Update () 
	{
        if (rumbleList.Count > 0)
        {
            float p1Weak = 0f;
            float p1Strong = 0f;
            float p2Weak = 0f;
            float p2Strong = 0f;


            for (int i = rumbleList.Count - 1; i >= 0; --i)
            {
                RumbleInfo rumble = rumbleList[i];
                rumble.Update();

                if(rumble.duration > 0)
                {
                    if(rumble.player == PlayerIndex.One)
                    {
                        p1Weak = rumble.GetMaxWeakValue(p1Weak);
                        p1Strong = rumble.GetMaxStrongValue(p1Strong);
                    }
                    else
                    {
                        p2Weak = rumble.GetMaxWeakValue(p2Weak);
                        p2Strong = rumble.GetMaxStrongValue(p2Strong);
                    }
                }
                else
                {
                    rumbleList.RemoveAt(i);
                }
            }

            GamePad.SetVibration(PlayerIndex.One, p1Strong, p1Weak);
            GamePad.SetVibration(PlayerIndex.Two, p2Strong, p2Weak);
        }
	}

    public void Rumble(int player = 0, float duration = 0.5f, float weakMotor = 1f, float strongMotor = 1f, float startFading = -1f)
    {
        //Player 0 means both players
        if (player == 0)
        {
            RumbleInfo rumble = new RumbleInfo(1, weakMotor, strongMotor, duration, (startFading == -1f ? duration * 0.75f : startFading));
            rumbleList.Add(rumble);
            rumble = new RumbleInfo(2, weakMotor, strongMotor, duration, (startFading == -1f ? duration * 0.75f : startFading));
            rumbleList.Add(rumble);
        }
        else
        {
            RumbleInfo rumble = new RumbleInfo(player, weakMotor, strongMotor, duration, (startFading == -1f ? duration * 0.75f : startFading));
            rumbleList.Add(rumble);
        }
    }
}
