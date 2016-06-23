using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnSpiderGroupWaveAction : WaveAction  {

    public enum FormationType
    {
        ROLLING,
        THREE_FRONT,
        THREE_BACK,
        TRIANGLE,
        QUAD
    }

    private const float delayAfterLeader = 1.5f;
    private const float delayBetweenFollowers = 0.5f;

    private string spawnPointName;
    private Transform spawnPoint;
    private SpawnPointController spawnController;

    private List<AIAction> leaderActions;
    private List<AIAction> attackActions;
    private List<AIAction> infectActions;

    private FormationType formation;
    private int[] colors;
    private List<AIAction>[] followersActions;
    private int spiderIndex;

    private EnemyManager enemyMng;

    private EnemyGroupInfo groupInfo;

    private SpiderAIBehaviour.SpawnAnimation spawnAnimation;

    public SpawnSpiderGroupWaveAction(string spPoint, List<AIAction> leader, List<AIAction> attack, List<AIAction> infect, FormationType form, int[] col, float iniDel = 0f, SpiderAIBehaviour.SpawnAnimation spawnAnim = SpiderAIBehaviour.SpawnAnimation.FLOOR)
    {
        spawnPointName = spPoint;
        leaderActions = leader;
        attackActions = attack;
        infectActions = infect;
        initialDelay = iniDel;
        formation = form;
        colors = col;
        spawnAnimation = spawnAnim;
        spiderIndex = 0;

        enemyMng = rsc.enemyMng;


        switch (formation)
        {
            case FormationType.ROLLING:
                followersActions = new List<AIAction>[4] { null, enemyMng.rolling, enemyMng.rolling, enemyMng.rolling };
                break;

            case FormationType.THREE_FRONT:
                followersActions = new List<AIAction>[4] { null, enemyMng.three_front_1, enemyMng.three_front_2, enemyMng.three_front_3 };
                break;

            case FormationType.THREE_BACK:
                followersActions = new List<AIAction>[4] { null, enemyMng.three_back_1, enemyMng.three_back_2, enemyMng.three_back_3 };
                break;

            case FormationType.TRIANGLE:
                followersActions = new List<AIAction>[5] { null, enemyMng.triangle_1, enemyMng.triangle_2, enemyMng.triangle_3, enemyMng.triangle_4 };
                break;

            case FormationType.QUAD:
                followersActions = new List<AIAction>[5] { null, enemyMng.quad_1, enemyMng.quad_2, enemyMng.quad_3, enemyMng.quad_4 };
                break;
            default:
                break;
        }

        groupInfo = new EnemyGroupInfo();
        groupInfo.followersCount = followersActions.Length - 1;
        groupInfo.leaderActionIndex = 0;

    }

    public override void Execute()
    {
        executing = true;

        spawnPoint = GameObject.Find(spawnPointName).transform; //TODO: Improve this
        spawnController = spawnPoint.GetComponent<SpawnPointController>();

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
            spider.AIInitGroup(spawnAnimation, groupInfo, leaderActions, followersActions[spiderIndex], attackActions, infectActions, (spiderIndex == 0) ? true : false);
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

    public override int GetWaveTotalInfection()
    {
        return SpiderAIBehaviour.infectionValue * followersActions.Length;
    }
}
