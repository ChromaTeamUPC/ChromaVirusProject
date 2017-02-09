using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class TutorialManager : MonoBehaviour 
{
    public enum Type
    {
        CONTROLLER,
        BASICS,
        COLOR_MECHANICS,
        COLLECT_ENERGY,
        SPECIAL_ATTACK,
        CAPACITOR,
        DEVICE,
        HIT_BOSS_HEAD
    }

    [SerializeField]
    private Sprite[] images;
    private bool[] shown;

    [HideInInspector]
    private bool active;

    void Awake()
    {
        shown = new bool[images.Length];
        for (int i = 0; i < shown.Length; ++i)
            shown[i] = false;

        active = true;

        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
        }
    }

    void GameReset(EventInfo eventInfo)
    {
        for (int i = 0; i < shown.Length; ++i)
            shown[i] = false;
    }

    public bool Active
    {
        get { return active; }

        set
        {
            active = value;
            if (active)
                for (int i = 0; i < shown.Length; ++i)
                    shown[i] = false;
        }
    }

    public int GetTotalImages()
    {
        return images.Length;
    }

    public Sprite GetImage(int imgIndex)
    {
        Sprite result = null;

        if (imgIndex >= 0 && imgIndex < images.Length)
            result = images[imgIndex];

        return result;
    }

    public Sprite GetImageIfNotShown(Type imgType)
    {
        Sprite result = null;

        if (active && !shown[(int)imgType])
        {
            shown[(int)imgType] = true;

            result = images[(int)imgType];
        }

        return result;
    }
}
