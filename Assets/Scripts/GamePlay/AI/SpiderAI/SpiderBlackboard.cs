using UnityEngine;
using System.Collections;

public class SpiderBlackboard: EnemyBaseBlackboard
{
    //Non reseteable values
    public SpiderAIBehaviour spider;

    public Transform boltSpawnPoint;

    public GameObject[] explosions = new GameObject[4];

    public SpiderAIBaseState spawningState;
    public SpiderAIActionsBaseState entryState;
    public SpiderAIActionsBaseState attackingPlayerState;
    public SpiderAIActionsBaseState leadingGroupState;
    public SpiderAIActionsBaseState followingGroupState;
    public SpiderAIBaseState attackingChipState;
    public SpiderAIBaseState attractedToBarrelState;
    public SpiderAIBaseState dyingState;

    //Reseteable values
    public float timeSinceLastAttack;   

    public float initialCheckDelay;
    public float checkAttackingSpidersDelay;
    public float checkInfectingChipDelay;

    public SpiderAIBehaviour.SpawnAnimation spawnAnimation;


    public override void InitialSetup(GameObject e)
    {
        base.InitialSetup(e);

        spider = entityGO.GetComponent<SpiderAIBehaviour>();
        boltSpawnPoint = entityGO.transform.Find("BoltSpawnPoint");

        for(int i = 0; i < 4; ++i)
        {
            explosions[i] = GameObject.Instantiate(spider.explosionPrefabs[i], spider.transform.position, spider.transform.rotation) as GameObject;
            explosions[i].transform.parent = spider.transform;
            explosions[i].SetActive(false);
        }

        spawningState = new SpiderSpawningAIState(this);
        entryState = new SpiderEntryAIState(this);
        attackingPlayerState = new SpiderAttackingPlayerAIState(this);
        leadingGroupState = new SpiderLeadingGroupAIState(this);
        followingGroupState = new SpiderFollowingGroupAIState(this);
        attackingChipState = new SpiderAttackingChipAIState(this);
        attractedToBarrelState = new SpiderAttractedToBarrelAIState(this);
        dyingState = new SpiderDyingAIState(this);

        ResetValues();
    }

    public override void ResetValues()
    {
        base.ResetValues();

        timeSinceLastAttack = 100f;

        initialCheckDelay = 0f;
        checkAttackingSpidersDelay = 0f;
        checkInfectingChipDelay = 0f;

        spawnAnimation = SpiderAIBehaviour.SpawnAnimation.FLOOR;   
    }
}
