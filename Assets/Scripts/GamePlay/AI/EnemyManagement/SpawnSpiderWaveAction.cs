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

    private string spawnPointName;
    private Transform spawnPoint;
    private SpawnPointController spawnController;

    private List<AIAction> entryActions;
    private List<AIAction> attackActions;
    private List<AIAction> infectActions;

    private int totalSpawnsNumber;
    private float delayBetweenSpawns;

    private SpiderAIBehaviour.SpawnAnimation spawnAnimation;


    private SpawnSpiderWaveAction(ColorMode mode, string spPoint, List<AIAction> entry, List<AIAction> attack, List<AIAction> infect, float iniDel, int howMany, float delBetweenSp, SpiderAIBehaviour.SpawnAnimation spawnAnim)
    {
        colorMode = mode;
        spawnPointName = spPoint;
        entryActions = entry;
        attackActions = attack;
        infectActions = infect;
        initialDelay = iniDel;
        totalSpawnsNumber = howMany;
        delayBetweenSpawns = delBetweenSp;
        spawnAnimation = spawnAnim;
    }

    public SpawnSpiderWaveAction(ChromaColor c, string spPoint, List<AIAction> entry, List<AIAction> attack, List<AIAction> infect,
        float iniDel = 0f, int howMany = 1, float delBetweenSp = 0f, SpiderAIBehaviour.SpawnAnimation spawnAnim = SpiderAIBehaviour.SpawnAnimation.FLOOR)
        : this(ColorMode.FIXED, spPoint, entry, attack, infect, iniDel, howMany, delBetweenSp, spawnAnim)
    {
        color = c;
    }

    public SpawnSpiderWaveAction(bool current, int colorOffset, string spPoint, List<AIAction> entry, List<AIAction> attack, List<AIAction> infect,
        float iniDel = 0f, int howMany = 1, float delBetweenSp = 0f, SpiderAIBehaviour.SpawnAnimation spawnAnim = SpiderAIBehaviour.SpawnAnimation.FLOOR)
        : this(ColorMode.CURRENT, spPoint, entry, attack, infect, iniDel, howMany, delBetweenSp, spawnAnim)
    {
        this.colorOffset = colorOffset;
    }

    public SpawnSpiderWaveAction(string spPoint, List<AIAction> entry, List<AIAction> attack, List<AIAction> infect,
        float iniDel = 0f, int howMany = 1, float delBetweenSp = 0f, SpiderAIBehaviour.SpawnAnimation spawnAnim = SpiderAIBehaviour.SpawnAnimation.FLOOR)
        : this(ColorMode.RANDOM, spPoint, entry, attack, infect, iniDel, howMany, delBetweenSp, spawnAnim)
    {
    }

    public override void Execute()
    {
        executing = true;

        spawnPoint = GameObject.Find(spawnPointName).transform; //TODO: Improve this
        spawnController = spawnPoint.GetComponent<SpawnPointController>();

        SpawnSpider();

        if (totalSpawnsNumber > 1)
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
                spider = rsc.coloredObjectsMng.GetSpider(true);
                break;

            default:
                spider = rsc.coloredObjectsMng.GetSpider(colorOffset);
                break;
        }

        if (spider != null)
        {
            spider.AIInit(spawnAnimation, entryActions, attackActions, infectActions);
            spider.Spawn(spawnPoint);

            if(spawnAnimation == SpiderAIBehaviour.SpawnAnimation.FLOOR)
            {
                spawnController.CreatePortal(spider.color);
            }
        }
    }

    IEnumerator SpawnSpiders()
    {
        int totalSpawned = 1;

        while(totalSpawned < totalSpawnsNumber)
        {
            yield return new WaitForSeconds(delayBetweenSpawns);
            SpawnSpider();
            ++totalSpawned;
        }

        executing = false;
    }

    public override int GetWaveTotalInfection()
    {
        return SpiderAIBehaviour.infectionValue * totalSpawnsNumber;
    }
}
