using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class SpawnSpiderWaveAction : WaveAction
{
    public enum ColorMode
    {
        FIXED,
        CURRENT,
        RANDOM
    }

    private ColorMode colorMode;
    private ChromaColor color;
    private int colorOffset;
    //private Transform spawnPoint;
    private string spawnPoint;
    private List<AIAction> defaultActions;
    private List<AIAction> closeActions;
    private List<AIAction> attackChipActions;

    //public SpawnSpiderWaveAction(ChromaColor c,  Transform spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float del = 0f)
    public SpawnSpiderWaveAction(ChromaColor c, string spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float del = 0f)
    {
        colorMode = ColorMode.FIXED;
        color = c;
        spawnPoint = spPoint;
        defaultActions = def;
        closeActions = close;
        attackChipActions = att;
        delay = del;
    }

    public SpawnSpiderWaveAction(bool current, int colorOffset, string spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float del = 0f)
    {
        colorMode = ColorMode.CURRENT;
        this.colorOffset = colorOffset;
        spawnPoint = spPoint;
        defaultActions = def;
        closeActions = close;
        attackChipActions = att;
        delay = del;
    }

    public SpawnSpiderWaveAction(string spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float del = 0f)
    {
        colorMode = ColorMode.RANDOM;
        spawnPoint = spPoint;
        defaultActions = def;
        closeActions = close;
        attackChipActions = att;
        delay = del;
    }

    public override void Execute()
    {
        //Spawn enemy
        SpiderAIBehaviour spider;
        
        switch (colorMode)
	    {
		case ColorMode.FIXED:
            spider = rsc.coloredObjectsMng.GetSpider(color);
            break;

        case ColorMode.CURRENT:
            spider = rsc.coloredObjectsMng.GetSpider(colorOffset);
            break;

        case ColorMode.RANDOM:
            spider = rsc.coloredObjectsMng.GetSpiderRandomColor();
            break;

        default:
            spider = rsc.coloredObjectsMng.GetSpider(colorOffset);
            break;
	    }
        
        if (spider != null)
        {
            Transform spawn = GameObject.Find(spawnPoint).transform; //TODO: Improve this

            spider.AIInit(defaultActions, closeActions, attackChipActions);
            spider.Spawn(spawn);
        }
    }
}
