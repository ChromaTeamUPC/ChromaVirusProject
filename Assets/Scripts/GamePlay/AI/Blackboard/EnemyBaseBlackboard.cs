using UnityEngine;
using System.Collections;

public class EnemyBaseBlackboard
{
    public GameObject entityGO;
    public EnemyBaseAIBehaviour entity;

    public NavMeshAgent agent;

    public Animator animator;

    public bool spawnAnimationEnded;
    public bool attackAnimationTrigger;
    public bool attackAnimationEnded;
    public bool dieAnimationEnded;

    public bool canReceiveDamage;

    public GameObject target;
    public bool targetIsPlayer;

    public CapacitorController barrelController;

    public Vector3 lastShotDirection;
    public bool lastShotSameColor;

    public EnemyGroupInfo groupInfo;

    public float currentHealth;

    public virtual void InitialSetup(GameObject e)
    {     
        entityGO = e;
        entity = entityGO.GetComponent<EnemyBaseAIBehaviour>();
        agent = entityGO.GetComponent<NavMeshAgent>();
        animator = entityGO.GetComponent<Animator>();

        spawnAnimationEnded = true;
        attackAnimationEnded = true;
        attackAnimationTrigger = false;
        dieAnimationEnded = true;

        canReceiveDamage = false;      
        target = null;
        targetIsPlayer = false;
        barrelController = null;

        groupInfo = null;
    }

    public virtual void ResetValues()
    {
        spawnAnimationEnded = true;
        attackAnimationTrigger = false;
        attackAnimationEnded = true;
        dieAnimationEnded = true;

        canReceiveDamage = false;
        target = null;
        targetIsPlayer = false;
        barrelController = null;

        groupInfo = null;
        currentHealth = entity.maxHealth;
    }
}
