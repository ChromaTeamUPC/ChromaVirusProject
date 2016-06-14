﻿using UnityEngine;
using System;
using System.Collections;
using SynchronizerData;


//Helper class to know the first and last colors defined in the enumerator, so we can check for .First or .Last and always will return the proper color even if we reorder the enum
public class ChromaColorInfo
{
    public const int CURRENT_COLOR_OFFSET = -4;
    public const int CURRENT_COLOR_PLUS1_OFFSET = -3;
    public const int CURRENT_COLOR_PLUS2_OFFSET = -2;
    public const int CURRENT_COLOR_PLUS3_OFFSET = -1;

    public static ChromaColor First = (ChromaColor)Enum.GetValues(typeof(ChromaColor)).GetValue(0);
    public static ChromaColor Last = (ChromaColor)Enum.GetValues(typeof(ChromaColor)).GetValue(Enum.GetValues(typeof(ChromaColor)).Length - 1);
    public static int Count = Enum.GetValues(typeof(ChromaColor)).Length;

    public static ChromaColor Random
    {
        get { return (ChromaColor)UnityEngine.Random.Range((int)First, (int)Last + 1); }
    }

    public static string GetColorName(ChromaColor color)
    {
        switch (color)
        {
            case ChromaColor.RED:
                return "Red";
            case ChromaColor.GREEN:
                return "Green";                
            case ChromaColor.BLUE:
                return "Blue";
            case ChromaColor.YELLOW:
                return "Yellow";
            default:
                return "Oooops wrong color";
        }
    }
}

public enum ChromaColor
{
    RED,
    GREEN,
    BLUE,
    YELLOW
}

public class ColorManager : MonoBehaviour
{
    public bool musicSynced = true;
    public float changeInterval;

    private int[] colorCount = new int[] { 0, 0, 0, 0 };
    private ChromaColor currentColor;
    private float elapsedTime = 0f;

    private BeatObserver beatObserver;
    private bool colorChange;

    private bool active;

    public ChromaColor CurrentColor { get { return currentColor; } }

    void Awake()
    {
        Debug.Log("Color Manager created");
        active = false;
        colorChange = true;
        currentColor = ChromaColorInfo.Random;
        beatObserver = GetComponent<BeatObserver>();
    }

    void Start()
    {    
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_SPAWNED, EnemySpawned);
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_DIED, EnemyDied); 
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_SPAWNED, EnemySpawned);
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
        }
        Debug.Log("Color Manager destroyed");
    }

    public void Activate()
    {
        active = true;
        elapsedTime = 0f;
        if (musicSynced)
            SendColorEvent();       
        else
            SetNewColor();
    }

    public void Deactivate()
    {
        active = false;
    }

    private void GameReset(EventInfo eventInfo)
    {
        Deactivate();
        for(int i = 0; i < colorCount.Length; ++i)
        {
            colorCount[i] = 0;
        }
    }

    public void EnemySpawned(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        ++colorCount[(int)info.newColor];
    }

    public void EnemyDied(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        --colorCount[(int)info.newColor];
        if (colorCount[(int)info.newColor] < 0) colorCount[(int)info.newColor] = 0;
    }

    void FixedUpdate ()
    {
        if (active && !musicSynced)
        {
            elapsedTime += Time.fixedDeltaTime;

            if (elapsedTime >= changeInterval)
            {
                elapsedTime -= changeInterval;
                SetNewColor();        
            }
        }
	}

    void Update()
    {
        if (active && musicSynced)
        {
            if ((beatObserver.beatMask & BeatType.DownBeat) == BeatType.DownBeat)
            {
                if(colorChange)
                    SetNewColor();

                colorChange = !colorChange;
            }
        }
    }

    private void SetNewColor()
    {
        ChromaColor newColor;

        //If there is no color enemies in the scene, standard sequence
        if (TotalColorItems() == 0)
        {
            newColor = currentColor;

            if (newColor == ChromaColorInfo.Last)
                newColor = ChromaColorInfo.First;
            else
                newColor++;
        }
        else
        {
            //If there is only one color of enemies, change between that color and a random one
            if (TotalColorsWithItems() == 1)
            {
                ChromaColor itemsColor = GetFirstColorWithItems();

                if (currentColor != itemsColor)
                    newColor = itemsColor;
                else
                {
                    do
                    {
                        newColor = ChromaColorInfo.Random;
                    }
                    while (newColor == itemsColor);
                }
            }
            //If there is more than one color of enemies, change between their colors
            else
            {
                newColor = currentColor;

                ////Search first color in loop that has items
                do
                {
                    if (newColor == ChromaColorInfo.Last)
                        newColor = ChromaColorInfo.First;
                    else
                        newColor++;
                }
                while (colorCount[(int)newColor] <= 0);
            }
        }

        if (currentColor != newColor)
        {
            currentColor = newColor;
            SendColorEvent();
        }
    }

    private ChromaColor GetFirstColorWithItems()
    {
        for (int i = 0; i < colorCount.Length; ++i)
            if (colorCount[i] > 0)
                return (ChromaColor)i;

        return ChromaColorInfo.Random;
    }

    private int TotalColorsWithItems()
    {
        int result = 0;

        for (int i = 0; i < colorCount.Length; ++i)
            if (colorCount[i] > 0)
                ++result;

        return result;
    }

    private int TotalColorItems()
    {
        int sum = colorCount[0];

        for (int i = 1; i < colorCount.Length; ++i)
            sum += colorCount[i];

        return sum;
    }

    private void SendColorEvent()
    {
        ColorEventInfo.eventInfo.newColor = currentColor;
        rsc.eventMng.TriggerEvent(EventManager.EventType.COLOR_CHANGED, ColorEventInfo.eventInfo);
    }
}
