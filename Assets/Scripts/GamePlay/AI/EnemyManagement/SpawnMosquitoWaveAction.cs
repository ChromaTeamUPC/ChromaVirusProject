using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnMosquitoWaveAction : WaveAction
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

    private List<AIAction> patrolActions;
    private List<AIAction> attackActions;

    private int totalSpawnsNumber;
    private float delayBetweenSpawns;

    private MosquitoAIBehaviour.SpawnAnimation spawnAnimation;


    private SpawnMosquitoWaveAction(ColorMode mode, string spPoint, List<AIAction> patrol, List<AIAction> attack, float iniDel, int howMany, float delBetweenSp, MosquitoAIBehaviour.SpawnAnimation spawnAnim)
    {
        colorMode = mode;
        spawnPointName = spPoint;
        patrolActions = patrol;
        attackActions = attack;
        initialDelay = iniDel;
        totalSpawnsNumber = howMany;
        delayBetweenSpawns = delBetweenSp;
        spawnAnimation = spawnAnim;
    }

    public SpawnMosquitoWaveAction(ChromaColor c, string spPoint, List<AIAction> patrol, List<AIAction> attack,
        float iniDel = 0f, int howMany = 1, float delBetweenSp = 0f, MosquitoAIBehaviour.SpawnAnimation spawnAnim = MosquitoAIBehaviour.SpawnAnimation.ABOVE)
        : this(ColorMode.FIXED, spPoint, patrol, attack, iniDel, howMany, delBetweenSp, spawnAnim)
    {
        color = c;
    }

    public SpawnMosquitoWaveAction(bool current, int colorOffset, string spPoint, List<AIAction> patrol, List<AIAction> attack,
        float iniDel = 0f, int howMany = 1, float delBetweenSp = 0f, MosquitoAIBehaviour.SpawnAnimation spawnAnim = MosquitoAIBehaviour.SpawnAnimation.ABOVE)
        : this(ColorMode.CURRENT, spPoint, patrol, attack, iniDel, howMany, delBetweenSp, spawnAnim)
    {
        this.colorOffset = colorOffset;
    }

    public SpawnMosquitoWaveAction(string spPoint, List<AIAction> patrol, List<AIAction> attack,
        float iniDel = 0f, int howMany = 1, float delBetweenSp = 0f, MosquitoAIBehaviour.SpawnAnimation spawnAnim = MosquitoAIBehaviour.SpawnAnimation.ABOVE)
        : this(ColorMode.RANDOM, spPoint, patrol, attack, iniDel, howMany, delBetweenSp, spawnAnim)
    {
    }

    public override void Execute()
    {
        executing = true;

        spawnPoint = GameObject.Find(spawnPointName).transform; //TODO: Improve this

        SpawnMosquito();

        if (totalSpawnsNumber > 1)
        {
            rsc.coroutineHlp.StartCoroutine(SpawnMosquitoes());
        }
        else
            executing = false;
    }

    private void SpawnMosquito()
    {
        //Spawn enemy
        MosquitoAIBehaviour mosquito;

        switch (colorMode)
        {
            case ColorMode.FIXED:
                mosquito = rsc.coloredObjectsMng.GetMosquito(color);
                break;

            case ColorMode.CURRENT:
                mosquito = rsc.coloredObjectsMng.GetMosquito(colorOffset);
                break;

            case ColorMode.RANDOM:
                mosquito = rsc.coloredObjectsMng.GetMosquito(true);
                break;

            default:
                mosquito = rsc.coloredObjectsMng.GetMosquito(colorOffset);
                break;
        }

        if (mosquito != null)
        {
            mosquito.AIInit(spawnAnimation, patrolActions, attackActions);
            mosquito.Spawn(spawnPoint);
        }
    }

    IEnumerator SpawnMosquitoes()
    {
        int totalSpawned = 1;

        while (totalSpawned < totalSpawnsNumber)
        {
            yield return new WaitForSeconds(delayBetweenSpawns);
            SpawnMosquito();
            ++totalSpawned;
        }

        executing = false;
    }

    public override int GetWaveTotalInfection()
    {
        return MosquitoAIBehaviour.infectionValue * totalSpawnsNumber;
    }
}
