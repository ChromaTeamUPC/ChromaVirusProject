using UnityEngine;
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
    public float prewarningSeconds;

    private int[] colorCount = new int[] { 0, 0, 0, 0 };
    private ChromaColor currentColor;
    private ChromaColor newColor;
    private float elapsedTime = 0f;

    private BeatObserver beatObserver;
    private bool colorChange;
    private bool colorPrecalculated;
    private float prewarnInterval;

    private bool active;

    public ChromaColor CurrentColor { get { return currentColor; } }
    public ChromaColor NextColor { get { return newColor; } }
    public float ElapsedTime { get { return elapsedTime; } }

    void Awake()
    {
        //Debug.Log("Color Manager created");
        prewarnInterval = changeInterval - prewarningSeconds;
        active = false;
        colorChange = true;
        currentColor = ChromaColorInfo.Random;
        newColor = currentColor;
        beatObserver = GetComponent<BeatObserver>();
    }

    void Start()
    {    
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_SPAWNED, EnemySpawned);
        rsc.eventMng.StartListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_SECTION_ACTIVATED, WormSectionActivated);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_SECTION_COLOR_CHANGED, WormSectionColorChanged);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_SECTION_DESTROYED, WormSectionDestroyed);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_SPAWNED, EnemySpawned);
            rsc.eventMng.StopListening(EventManager.EventType.ENEMY_DIED, EnemyDied);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_SECTION_ACTIVATED, WormSectionActivated);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_SECTION_COLOR_CHANGED, WormSectionColorChanged);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_SECTION_DESTROYED, WormSectionDestroyed);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
        }
        //Debug.Log("Color Manager destroyed");
    }

    public void Activate()
    {
        active = true;
        elapsedTime = 0f;
        colorPrecalculated = false;

        if (musicSynced)
            SendNewColorEvent();
        else
        {
            SetNewColor();
            //NotifyNewColor();
            PrecalculateColor();
        }
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

    private void EnemySpawned(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        ++colorCount[(int)info.newColor];
    }

    private void EnemyDied(EventInfo eventInfo)
    {
        EnemyDiedEventInfo info = (EnemyDiedEventInfo)eventInfo;
        --colorCount[(int)info.color];
        if (colorCount[(int)info.color] < 0) colorCount[(int)info.color] = 0;
    }

    private void WormSectionActivated(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        ++colorCount[(int)info.newColor];;
    }

    private void WormSectionColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        --colorCount[(int)info.oldColor];
        ++colorCount[(int)info.newColor];
    }

    private void WormSectionDestroyed(EventInfo eventInfo)
    {
        EnemyDiedEventInfo info = (EnemyDiedEventInfo)eventInfo;
        --colorCount[(int)info.color];
        if (colorCount[(int)info.color] < 0) colorCount[(int)info.color] = 0;
    }

    public void PrintColors()
    {
        Debug.Log("Red: " + colorCount[0] + " // Green: " + +colorCount[1] + " // Blue: " + +colorCount[2] + " // Yellow: " + +colorCount[3]);
    }

    void FixedUpdate ()
    {
        if (active && !musicSynced)
        {
            elapsedTime += Time.fixedDeltaTime;

            if (elapsedTime >= changeInterval)
            {
                elapsedTime -= changeInterval;
                colorPrecalculated = false;
                NotifyNewColor();        
            }
            else if (elapsedTime >= prewarnInterval && !colorPrecalculated)
            {
                PrecalculateColor();
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

    private ChromaColor GetColor()
    {
        ChromaColor result;

        //If there is no color enemies in the scene, standard sequence
        if (TotalColorItems() == 0)
        {
            result = currentColor;

            if (result == ChromaColorInfo.Last)
                result = ChromaColorInfo.First;
            else
                result++;
        }
        else
        {
            //If there is only one color of enemies, change between that color and a random one
            if (TotalColorsWithItems() == 1)
            {
                ChromaColor itemsColor = GetFirstColorWithItems();

                if (currentColor != itemsColor)
                {
                    result = itemsColor;
                }
                else
                {
                    do
                    {
                        result = ChromaColorInfo.Random;
                    }
                    while (result == itemsColor);
                }
            }
            //If there is more than one color of enemies, change between their colors
            else
            {
                result = currentColor;

                ////Search first color in loop that has items
                do
                {
                    if (result == ChromaColorInfo.Last)
                        result = ChromaColorInfo.First;
                    else
                        result++;
                }
                while (colorCount[(int)result] <= 0);
            }
        }

        return result;
    }

    private void PrecalculateColor()
    {
        colorPrecalculated = true;

        newColor = GetColor();

        if (currentColor != newColor)
        {
            SendNextColorEvent();
        }
    }

    private void NotifyNewColor()
    {
        if (currentColor != newColor)
        {
            currentColor = newColor;
            SendNewColorEvent();
        }
    }

    private void SetNewColor()
    {
        ChromaColor color = GetColor();     

        if (currentColor != color)
        {
            currentColor = color;
            SendNewColorEvent();
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

    private void SendNextColorEvent()
    {
        ColorPrewarnEventInfo.eventInfo.newColor = newColor;
        ColorPrewarnEventInfo.eventInfo.prewarnSeconds = prewarningSeconds;
        rsc.eventMng.TriggerEvent(EventManager.EventType.COLOR_WILL_CHANGE, ColorPrewarnEventInfo.eventInfo);
    }

    private void SendNewColorEvent()
    {
        ColorEventInfo.eventInfo.newColor = currentColor;
        rsc.eventMng.TriggerEvent(EventManager.EventType.COLOR_CHANGED, ColorEventInfo.eventInfo);
    }
}
