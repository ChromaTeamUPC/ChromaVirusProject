using UnityEngine;
using System.Collections;

public class EnemyBaseBlackboard
{
    public GameObject entityGO;
    public EnemyBaseAIBehaviour entity;

    public Rigidbody rigidBody;
    public NavMeshAgent agent;

    public Animator animator;
    public bool animationEnded;

    public bool canReceiveDamage;

    public GameObject target;
    public bool targetIsPlayer;
    public BarrelController barrelController;

    public Vector3 lastShotDirection;

    public virtual void InitialSetup(GameObject e)
    {     
        entityGO = e;
        entity = entityGO.GetComponent<EnemyBaseAIBehaviour>();
        rigidBody = entityGO.GetComponent<Rigidbody>();
        agent = entityGO.GetComponent<NavMeshAgent>();
        animator = entityGO.GetComponent<Animator>();

        animationEnded = true;
        canReceiveDamage = false;      
        target = null;
        targetIsPlayer = false;
        barrelController = null;
    }

    public virtual void ResetValues()
    {
        animationEnded = true;
        canReceiveDamage = false;
        target = null;
        targetIsPlayer = false;
        barrelController = null;
    }
}
