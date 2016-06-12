using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MosquitoAIBehaviour : EnemyBaseAIBehaviour
{
    public enum SpawnAnimation
    {
        ABOVE,
        BELOW
    }

    private MosquitoBlackboard mosquitoBlackboard;

    [Header("Mosquito Settings")]
    public float checkTargetEverySeconds = 1f;
    public float angularSpeed = 360f;

    public float checkAttackEverySeconds = 2f;
    public int activeAttacksThreshold = 5;
    public int chancesReductionForEachActiveAttack = 20;
    public float patrolShotDamage = 10f;

    public int mosquitoesAttackingThreshold = 3;
    public int chancesReductionForEachAttackingMosquito = 33;
    public float mainAttackDamage = 20f;

    private Transform rotationObject;

    public GameObject[] explosionPrefabs = new GameObject[4];

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();

        mosquitoBlackboard = new MosquitoBlackboard();
        mosquitoBlackboard.InitialSetup(gameObject);

        blackboard = mosquitoBlackboard;

        rotationObject = transform.Find("RotationObject");
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

    public void AIInit(SpawnAnimation spawnAnimation, List<AIAction> patrolList, List<AIAction> attackList)
    {
        mosquitoBlackboard.ResetValues();
        mosquitoBlackboard.spawnAnimation = spawnAnimation;

        //Init states with lists
        mosquitoBlackboard.patrolingState.Init(patrolList);
        mosquitoBlackboard.attackingPlayerState.Init(attackList);
    }

    public void Spawn(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        mosquitoBlackboard.player = rsc.enemyMng.SelectPlayer(gameObject);
        ChangeState(mosquitoBlackboard.spawningState);
    }

    protected override void Update()
    {
        mosquitoBlackboard.checkPlayerElapsedTime += Time.deltaTime;
        //Check valid player
        if(mosquitoBlackboard.checkPlayerElapsedTime >= checkTargetEverySeconds)
        {
            mosquitoBlackboard.checkPlayerElapsedTime = 0f;
            mosquitoBlackboard.player = rsc.enemyMng.SelectPlayer(gameObject);
        }

        //If player is valid, look at it
        if(mosquitoBlackboard.player != null)
        {
            Vector3 direction = mosquitoBlackboard.player.transform.position - transform.position;
            direction.y = 0f;

            Quaternion newRotation = Quaternion.LookRotation(direction);
            newRotation = Quaternion.RotateTowards(rotationObject.rotation, newRotation, angularSpeed * Time.deltaTime);
            rotationObject.rotation = newRotation;
        }
        //else look forward
        else
        {
            Vector3 direction = transform.forward;

            Quaternion newRotation = Quaternion.LookRotation(direction);
            newRotation = Quaternion.RotateTowards(rotationObject.rotation, newRotation, angularSpeed * Time.deltaTime);
            rotationObject.rotation = newRotation;
        }

        mosquitoBlackboard.timeSinceLastAttack += Time.deltaTime;

        base.Update();
    }

    //Not to be used outside FSM
    public override AIBaseState ProcessShotImpact(ChromaColor shotColor, float damage, Vector3 direction, PlayerController player)
    {
        if (mosquitoBlackboard.canReceiveDamage && mosquitoBlackboard.currentHealth > 0)
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
                mosquitoBlackboard.currentHealth -= damage * wrongColorDamageModifier;
            }
            else
            {
                mosquitoBlackboard.currentHealth -= damage;
            }

            if (mosquitoBlackboard.currentHealth <= 0)
            {
                if (shotColor != color)
                {
                    player.ColorMismatch();
                }

                mosquitoBlackboard.lastShotDirection = direction;
                mosquitoBlackboard.lastShotSameColor = (shotColor == color);
                return mosquitoBlackboard.dyingState;
            }
        }

        return null;
    }

    //Not to be used outside FSM
    public override AIBaseState ProcessSpecialImpact(float damage, Vector3 direction)
    {
        if (mosquitoBlackboard.canReceiveDamage && mosquitoBlackboard.currentHealth > 0)
        {
            mosquitoBlackboard.currentHealth -= damage;
            if (mosquitoBlackboard.currentHealth <= 0)
            {
                mosquitoBlackboard.lastShotDirection = direction;
                mosquitoBlackboard.lastShotSameColor = true;
                return mosquitoBlackboard.dyingState;
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

        if (mosquitoBlackboard.canReceiveDamage && mosquitoBlackboard.currentHealth > 0)
        {
            mosquitoBlackboard.currentHealth -= damage;
            if (mosquitoBlackboard.currentHealth <= 0)
            {
                mosquitoBlackboard.lastShotDirection = direction;
                mosquitoBlackboard.lastShotSameColor = (barrelColor == color);
                return mosquitoBlackboard.dyingState;
            }
        }

        return null;
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

        voxelization.SpawnColliderThisTime = false;
        SpawnVoxels();
        rsc.poolMng.mosquitoPool.AddObject(this);
    }

    public override void CollitionOnDie()
    {
        SpawnVoxelsAndReturnToPool();
    }
}
