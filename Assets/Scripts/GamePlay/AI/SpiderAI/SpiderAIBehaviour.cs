using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderAIBehaviour : EnemyBaseAIBehaviour
{
    public enum SpawnAnimation
    {
        FLOOR,
        SKY
    }

    private SpiderBlackboard spiderBlackboard;

    [Header("Spider Settings")]
    public int biteDamage = 10;
    public float playerDetectionDistance = 5f;
    public int spidersAttackingThreshold = 3;

    public GameObject[] explosionPrefabs = new GameObject[4];  
    
    public SpiderAIBaseState spawningState;
    public SpiderAIActionsBaseState entryState;
    public SpiderAIActionsBaseState attackingPlayerState;
    public SpiderAIActionsBaseState leadingGroupState;
    public SpiderAIActionsBaseState followingGroupState;
    public SpiderAIBaseState attackingChipState;
    public SpiderAIBaseState attractedToBarrelState;
    public SpiderAIBaseState dyingState;

	// Use this for initialization
	protected override void Awake ()
    {
        base.Awake();

        spiderBlackboard = new SpiderBlackboard();    
        spiderBlackboard.InitialSetup(gameObject);

        blackboard = spiderBlackboard;

        spawningState = new SpiderSpawningAIState(spiderBlackboard);
        entryState = new SpiderEntryAIState(spiderBlackboard);
        attackingPlayerState = new SpiderAttackingPlayerAIState(spiderBlackboard);
        leadingGroupState = new SpiderLeadingGroupAIState(spiderBlackboard);
        followingGroupState = new SpiderFollowingGroupAIState(spiderBlackboard);
        attackingChipState = new SpiderAttackingChipAIState(spiderBlackboard);
        attractedToBarrelState = new SpiderAttractedToBarrelAIState(spiderBlackboard);  
        dyingState = new SpiderDyingAIState(spiderBlackboard);
    }

    public override void SetMaterials(Material[] materials)
    {
        Material[] mats = rend.materials;

        if (mats[1] != materials[0])
        {
            mats[1] = materials[0];
            rend.materials = mats;

            blinkController.InvalidateMaterials();
        }
    }

    public void AIInit(SpawnAnimation spawnAnimation, List<AIAction> entryList, List<AIAction> attackList)
    {
        spiderBlackboard.ResetValues();
        spiderBlackboard.spawnAnimation = spawnAnimation;

        //Init states with lists
        entryState.Init(entryList);
        attackingPlayerState.Init(attackList);   
    }

    public void AIInitGroup(SpawnAnimation spawnAnimation, EnemyGroupInfo groupInfo, List<AIAction> leaderList, List<AIAction> followerList, List<AIAction> attackList, bool isLeader = false)
    {
        spiderBlackboard.ResetValues();
        spiderBlackboard.spawnAnimation = spawnAnimation;

        spiderBlackboard.groupInfo = groupInfo;
        if (isLeader)
            groupInfo.leader = this;

        //Init states with lists
        leadingGroupState.Init(leaderList);
        followingGroupState.Init(followerList);
        attackingPlayerState.Init(attackList);
    }

    public void Spawn(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        ChangeState(spawningState);      
    }


    // Update is called once per frame
    void Update ()
    {
        spiderBlackboard.timeSinceLastAttack += Time.deltaTime;

        AIBaseState newState = currentState.Update();

        if(newState != null)
        {
            ChangeState(newState);
        }
	}

    //Not to be used outside FSM
    public override AIBaseState ProcessShotImpact(ChromaColor shotColor, int damage, Vector3 direction, PlayerController player)
    {
        if (spiderBlackboard.canReceiveDamage && spiderBlackboard.currentHealth > 0)
        {
            blinkController.Blink();

            /*if (shotColor == color)
            {
                spiderBlackboard.currentHealth -= damage;
                if (spiderBlackboard.currentHealth <= 0)
                {
                    spiderBlackboard.lastShotDirection = direction;
                    return dyingState;
                }
            }
            //Else future behaviour like duplicate or increase health*/
            if(shotColor != color)
            {
                player.ColorMismatch();
            }

            spiderBlackboard.currentHealth -= damage;
            if (spiderBlackboard.currentHealth <= 0)
            {
                spiderBlackboard.lastShotDirection = direction;
                spiderBlackboard.lastShotSameColor = (shotColor == color);
                return dyingState;
            }
        }

        return null;
    }

    //Not to be used outside FSM
    public override AIBaseState ProcessBarrelImpact(ChromaColor barrelColor, int damage)
    {
        //Harmless to other colors
        if (barrelColor != color)
            return null;

        if (spiderBlackboard.canReceiveDamage && spiderBlackboard.currentHealth > 0)
        {
            spiderBlackboard.currentHealth -= damage;
            if (spiderBlackboard.currentHealth <= 0)
                return dyingState;
        }

        return null;
    }

    public bool CheckPlayersDistance()
    {
        bool result = false;
        if (rsc.gameInfo.player1Controller.Active)
            result = Vector3.Distance(blackboard.entityGO.transform.position, rsc.gameInfo.player1.transform.position) < playerDetectionDistance;

        if (!result && rsc.gameInfo.player2Controller.Active)
            result = Vector3.Distance(blackboard.entityGO.transform.position, rsc.gameInfo.player2.transform.position) < playerDetectionDistance;

        return result;
    }

    public void SpawnVoxelsAndReturnToPool()
    {
        SpawnVoxels();
        rsc.poolMng.spiderPool.AddObject(gameObject);
    }

    public override void CollitionOnDie()
    {
        SpawnVoxelsAndReturnToPool();
    }
}
