using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnSpiderGroupWaveAction : WaveAction  {

    public enum FormationType
    {
        TEST,
        H,
        X,
        V,
        TRIANGLE
    }

    private const float delayAfterLeader = 1.5f;
    private const float delayBetweenFollowers = 0.5f;

    private string spawnPointName;
    private Transform spawnPoint;
    private SpawnPointController spawnController;

    private List<AIAction> leaderActions;
    private List<AIAction> attackActions;

    private FormationType formation;
    private int[] colors;
    private List<AIAction>[] followersActions;
    private int spiderIndex;

    private EnemyManager enemyMng;

    private EnemyGroupInfo groupInfo;

    private SpiderAIBehaviour.SpawnAnimation spawnAnimation;

    public SpawnSpiderGroupWaveAction(string spPoint, List<AIAction> leader, List<AIAction> attack, FormationType form, int[] col, float iniDel = 0f, SpiderAIBehaviour.SpawnAnimation spawnAnim = SpiderAIBehaviour.SpawnAnimation.FLOOR)
    {
        spawnPointName = spPoint;
        leaderActions = leader;
        attackActions = attack;
        initialDelay = iniDel;
        formation = form;
        colors = col;
        spawnAnimation = spawnAnim;
        spiderIndex = 0;

        enemyMng = rsc.enemyMng;
    }

    public override void Execute()
    {
        executing = true;

        spawnPoint = GameObject.Find(spawnPointName).transform; //TODO: Improve this
        spawnController = spawnPoint.GetComponent<SpawnPointController>();
            
        switch (formation)
        {
            case FormationType.TEST:               
                followersActions = new List<AIAction>[3] { null, enemyMng.jordiTestFollower01_135_5, enemyMng.jordiTestFollower02_225_5 };
                break;
            case FormationType.H:
                break;
            case FormationType.X:
                break;
            case FormationType.V:
                break;
            case FormationType.TRIANGLE:
                break;
            default:
                break;
        }

        groupInfo = new EnemyGroupInfo();
        groupInfo.groupCount = followersActions.Length;
        groupInfo.leaderActionIndex = 0;

        spiderIndex = 0;
        SpawnSpider();

        rsc.coroutineHlp.StartCoroutine(SpawnSpiders());
    }

    private void SpawnSpider()
    {
        SpiderAIBehaviour spider;

        int color = colors[spiderIndex];

        //-4 to -1 (current color plus optional offset)
        if(color >= ChromaColorInfo.CURRENT_COLOR_OFFSET && color <= ChromaColorInfo.CURRENT_COLOR_PLUS3_OFFSET)
        {
            spider = rsc.coloredObjectsMng.GetSpider(color);
        }
        //first chroma color to last chroma color (fixed color)
        else if (color >= (int)ChromaColorInfo.First && color <= (int)ChromaColorInfo.Last)
        {
            spider = rsc.coloredObjectsMng.GetSpider((ChromaColor)color);
        }
        //any other number will be handled as offset
        else
        {
            color = color % ChromaColorInfo.Count;
            spider = rsc.coloredObjectsMng.GetSpider(color);
        }

        if (spider != null)
        {
            spider.AIInitGroup(spawnAnimation, groupInfo, leaderActions, followersActions[spiderIndex], attackActions, (spiderIndex == 0) ? true : false);
            spider.Spawn(spawnPoint);

            if (spawnAnimation == SpiderAIBehaviour.SpawnAnimation.FLOOR)
            {
                spawnController.CreatePortal(spider.color);
            }
        }

        ++spiderIndex;
    }

    IEnumerator SpawnSpiders()
    {
        //first wait
        yield return new WaitForSeconds(delayAfterLeader);

        while (spiderIndex < followersActions.Length)
        {
            SpawnSpider();

            if (spiderIndex < followersActions.Length)
                yield return new WaitForSeconds(delayBetweenFollowers);
        }

        executing = false;
    }
}
