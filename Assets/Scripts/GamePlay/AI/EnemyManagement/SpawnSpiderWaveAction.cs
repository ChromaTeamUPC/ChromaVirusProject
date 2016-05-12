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

    private int howMany;
    private float delayBetweenSpawns;
    private Transform spawn;

    //public SpawnSpiderWaveAction(ChromaColor c,  Transform spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float del = 0f)
    public SpawnSpiderWaveAction(ChromaColor c, string spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float iniDel = 0f, int howMny = 1, float delBetweenSp = 0f)
    {
        colorMode = ColorMode.FIXED;
        color = c;
        spawnPoint = spPoint;
        defaultActions = def;
        closeActions = close;
        attackChipActions = att;
        initialDelay = iniDel;
        howMany = howMny;
        delayBetweenSpawns = delBetweenSp;
    }

    public SpawnSpiderWaveAction(bool current, int colorOffset, string spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float iniDel = 0f, int howMny = 1, float delBetweenSp = 0f)
    {
        colorMode = ColorMode.CURRENT;
        this.colorOffset = colorOffset;
        spawnPoint = spPoint;
        defaultActions = def;
        closeActions = close;
        attackChipActions = att;
        initialDelay = iniDel;
        howMany = howMny;
        delayBetweenSpawns = delBetweenSp;
    }

    public SpawnSpiderWaveAction(string spPoint, List<AIAction> def, List<AIAction> close, List<AIAction> att, float iniDel = 0f, int howMny = 1, float delBetweenSp = 0f)
    {
        colorMode = ColorMode.RANDOM;
        spawnPoint = spPoint;
        defaultActions = def;
        closeActions = close;
        attackChipActions = att;
        initialDelay = iniDel;
        howMany = howMny;
        delayBetweenSpawns = delBetweenSp;
    }

    public override void Execute()
    {
        executing = true;

        spawn = GameObject.Find(spawnPoint).transform; //TODO: Improve this

        SpawnSpider();

        if (howMany > 1)
        {
            rsc.coroutineHlp.StartCoroutine(SpawnSpiders());
        }
        else
            executing = false;
    }

    private void SpawnSpider()
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
            spider.AIInit(defaultActions, closeActions, attackChipActions);
            spider.Spawn(spawn);
        }
    }

    IEnumerator SpawnSpiders()
    {
        int totalSpawned = 1;

        while(totalSpawned < howMany)
        {
            yield return new WaitForSeconds(delayBetweenSpawns);
            SpawnSpider();
            ++totalSpawned;
        }

        executing = false;
    }
}
