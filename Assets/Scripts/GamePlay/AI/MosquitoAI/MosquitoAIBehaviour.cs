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
        mainCollider = rotationObject.GetComponent<Collider>();
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
        mosquitoBlackboard.SetPlayer( rsc.enemyMng.SelectPlayer(gameObject) );

        ChangeState(mosquitoBlackboard.spawningState);
    }

    protected override void Update()
    {
        mosquitoBlackboard.checkPlayerElapsedTime += Time.deltaTime;
        //Check valid player
        if(mosquitoBlackboard.checkPlayerElapsedTime >= checkTargetEverySeconds)
        {
            mosquitoBlackboard.checkPlayerElapsedTime = 0f;
            mosquitoBlackboard.SetPlayer(rsc.enemyMng.SelectPlayer(gameObject));
        }

        Vector3 movingVector = transform.forward;
        Vector3 lookingVector;

        //If player is valid, look at it
        if(mosquitoBlackboard.player != null)
        {
            lookingVector = mosquitoBlackboard.player.transform.position - transform.position;
            lookingVector.y = 0f;

            Quaternion newRotation = Quaternion.LookRotation(lookingVector);
            newRotation = Quaternion.RotateTowards(rotationObject.rotation, newRotation, angularSpeed * Time.deltaTime);
            rotationObject.rotation = newRotation;
        }
        //else look forward
        else
        {
            lookingVector = transform.forward;

            Quaternion newRotation = Quaternion.LookRotation(lookingVector);
            newRotation = Quaternion.RotateTowards(rotationObject.rotation, newRotation, angularSpeed * Time.deltaTime);
            rotationObject.rotation = newRotation;
        }

        int angleBetweenVectors = AngleBetween360(lookingVector, movingVector);

        float angleRad = angleBetweenVectors * Mathf.Deg2Rad;
        float forward = Mathf.Cos(angleRad);
        float lateral = Mathf.Sin(angleRad);
        blackboard.animator.SetFloat("forward", forward);
        blackboard.animator.SetFloat("lateral", lateral);

        mosquitoBlackboard.timeSinceLastAttack += Time.deltaTime;

        base.Update();
    }

    private int AngleBetween360(Vector3 v1, Vector3 v2)
    {
        Vector3 n = new Vector3(0, 1, 0);

        float signedAngle = Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

        if (signedAngle >= 0)
            return (int)signedAngle;
        else
            return (int)(360 + signedAngle);
    }

    //Not to be used outside FSM
    public override AIBaseState ProcessShotImpact(ChromaColor shotColor, float damage, Vector3 direction, PlayerController player)
    {
        if (mosquitoBlackboard.canReceiveDamage && mosquitoBlackboard.HaveHealthRemaining())
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
                mosquitoBlackboard.currentHealthWrongColor -= damage * wrongColorDamageModifier;
            }
            else
            {
                mosquitoBlackboard.currentHealth -= damage;
            }

            if (!mosquitoBlackboard.HaveHealthRemaining())
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
        if (mosquitoBlackboard.canReceiveDamage && mosquitoBlackboard.HaveHealthRemaining())
        {
            mosquitoBlackboard.currentHealth -= damage;
            if (!mosquitoBlackboard.HaveHealthRemaining())
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

        if (mosquitoBlackboard.canReceiveDamage && mosquitoBlackboard.HaveHealthRemaining())
        {
            mosquitoBlackboard.currentHealth -= damage;
            if (!mosquitoBlackboard.HaveHealthRemaining())
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
