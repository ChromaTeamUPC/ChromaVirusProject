using UnityEngine;
using System.Collections;

public class EnemyBaseBlackboard
{
    public GameObject entityGO;
    public EnemyBaseAIBehaviour entity;

    public NavMeshAgent agent;

    public Animator animator;
    public bool animationEnded;
    public bool animationTrigger;

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

        animationEnded = true;
        animationTrigger = false;
        canReceiveDamage = false;      
        target = null;
        targetIsPlayer = false;
        barrelController = null;

        groupInfo = null;
    }

    public virtual void ResetValues()
    {
        animationEnded = true;
        animationTrigger = false;
        canReceiveDamage = false;
        target = null;
        targetIsPlayer = false;
        barrelController = null;

        groupInfo = null;
    }
}
