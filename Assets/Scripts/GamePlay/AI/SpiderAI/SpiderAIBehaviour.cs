using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderAIBehaviour : EnemyBaseAIBehaviour
{
    public const int infectionValue = 1;

    public enum SpawnAnimation
    {
        FLOOR,
        SKY
    }

    private SpiderBlackboard spiderBlackboard;

    [Header("Spider Settings")]
    public float biteDamage = 10f;
    public float playerDetectionDistance = 5f;
    public float checkAttackEverySeconds = 1f;
    public int spidersAttackingThreshold = 3;
    public float checkDeviceEverySeconds = 3f;
    public int spidersInfectingDeviceThreshold = 3;

    public GameObject[] explosionPrefabs = new GameObject[4];     

	// Use this for initialization
	protected override void Awake ()
    {
        base.Awake();

        spiderBlackboard = new SpiderBlackboard();    
        spiderBlackboard.InitialSetup(gameObject);

        blackboard = spiderBlackboard;
    }

    public override void SetMaterials(Material[] materials)
    {
        Material[] mats = rend.sharedMaterials;

        if (mats[1] != materials[0])
        {
            mats[1] = materials[0];
            rend.sharedMaterials = mats;

            blinkController.InvalidateMaterials();
        }
    }

    public void AIInit(SpawnAnimation spawnAnimation, List<AIAction> entryList, List<AIAction> attackList, List<AIAction> infectList)
    {
        spiderBlackboard.ResetValues();
        spiderBlackboard.spawnAnimation = spawnAnimation;

        //Init states with lists
        spiderBlackboard.entryState.Init(entryList);
        spiderBlackboard.attackingPlayerState.Init(attackList);
        spiderBlackboard.infectingDeviceState.Init(infectList);   
    }

    public void AIInitGroup(SpawnAnimation spawnAnimation, EnemyGroupInfo groupInfo, List<AIAction> leaderList, List<AIAction> followerList, List<AIAction> attackList, List<AIAction> infectList, bool isLeader = false)
    {
        spiderBlackboard.ResetValues();
        spiderBlackboard.spawnAnimation = spawnAnimation;

        spiderBlackboard.groupInfo = groupInfo;
        if (isLeader)
            groupInfo.leader = this;

        //Init states with lists
        spiderBlackboard.leadingGroupState.Init(leaderList);
        spiderBlackboard.followingGroupState.Init(followerList);
        spiderBlackboard.attackingPlayerState.Init(attackList);
        spiderBlackboard.infectingDeviceState.Init(infectList);
    }

    public void Spawn(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        ChangeState(spiderBlackboard.spawningState);      
    }


    // Update is called once per frame
    protected override void Update ()
    {
        spiderBlackboard.timeSinceLastAttack += Time.deltaTime;

        base.Update();
	}

    //Not to be used outside FSM
    public override AIBaseState ProcessShotImpact(ChromaColor shotColor, float damage, Vector3 direction, PlayerController player)
    {
        if (spiderBlackboard.canReceiveDamage && spiderBlackboard.HaveHealthRemaining())
        {
            blinkController.BlinkWhiteOnce();

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

            if (shotColor != color)
            {
                spiderBlackboard.currentHealthWrongColor -= damage * wrongColorDamageModifier;
            }
            else
            {
                spiderBlackboard.currentHealth -= damage;
            }

            if (!spiderBlackboard.HaveHealthRemaining())
            {
                if (shotColor != color)
                {
                    player.ColorMismatch();
                }

                spiderBlackboard.lastShotDirection = direction;
                spiderBlackboard.lastShotSameColor = (shotColor == color);
                return spiderBlackboard.dyingState;
            }
        }

        return null;
    }

    //Not to be used outside FSM
    public override AIBaseState ProcessSpecialImpact(float damage, Vector3 direction)
    {
        if (spiderBlackboard.canReceiveDamage && spiderBlackboard.HaveHealthRemaining())
        {
            spiderBlackboard.currentHealth -= damage;
            if (!spiderBlackboard.HaveHealthRemaining())
            {
                spiderBlackboard.lastShotDirection = direction;
                spiderBlackboard.lastShotSameColor = true;
                return spiderBlackboard.dyingState;
            }
        }

        return null;
    }


    //Not to be used outside FSM
    public override AIBaseState ProcessBarrelImpact(ChromaColor barrelColor, float damage, Vector3 direction)
    {
        //Harmless to other colors
        if (barrelColor != color)
            return null;

        if (spiderBlackboard.canReceiveDamage && spiderBlackboard.HaveHealthRemaining())
        {
            spiderBlackboard.currentHealth -= damage;
            if (!spiderBlackboard.HaveHealthRemaining())
            {
                spiderBlackboard.lastShotDirection = direction;
                spiderBlackboard.lastShotSameColor = (barrelColor == color);
                return spiderBlackboard.dyingState;
            }
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

    public void SpawnVoxelsAndReturnToPool(bool spawnEnergyVoxels = true)
    {
        if (spawnEnergyVoxels)
        {
            Vector3 pos = transform.position;
            EnergyVoxelPool pool = rsc.poolMng.energyVoxelPool;
            for (int i = 0; i < energyVoxelsSpawnedOnDie; ++i)
            {
                EnergyVoxelController voxel = pool.GetObject();
                if (voxel != null)
                {
                    voxel.transform.position = pos;
                    voxel.transform.rotation = Random.rotation;
                }
            }
        }

        SpawnVoxels();
        rsc.poolMng.spiderPool.AddObject(this);
    }

    public override void CollitionOnDie()
    {
        SpawnVoxelsAndReturnToPool();
    }
}
