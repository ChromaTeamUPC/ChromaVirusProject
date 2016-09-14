using UnityEngine;
using System.Collections;

public class EnemyBaseBlackboard
{
    //Non reseteable values
    public GameObject entityGO;
    public EnemyBaseAIBehaviour entity;
    public NavMeshAgent agent;
    public Animator animator;

    //Reseteable values
    public float currentHealth;
    public float currentHealthWrongColor;
    public bool canReceiveDamage;

    public bool spawnAnimationEnded;
    public bool attackAnimationTrigger;
    public bool attackAnimationEnded;
    public bool dieAnimationEnded;

    public GameObject target;

    public GameObject player;
    public PlayerController playerController;
    public bool lookAtPlayer;

    public CapacitorController capacitorController;
    public DeviceController deviceController;

    public Vector3 lastShotDirection;
    public bool lastShotSameColor;
    public PlayerController lastShotPlayer;
    public bool specialKill;

    public EnemyGroupInfo groupInfo;

    public virtual void InitialSetup(GameObject e)
    {     
        entityGO = e;
        entity = entityGO.GetComponent<EnemyBaseAIBehaviour>();
        agent = entityGO.GetComponent<NavMeshAgent>();
        animator = entityGO.GetComponent<Animator>();

        ResetValues();
    }

    public virtual void ResetValues()
    {
        currentHealth = entity.maxHealth;
        currentHealthWrongColor = entity.maxHealth;
        canReceiveDamage = false;

        spawnAnimationEnded = true;
        attackAnimationTrigger = false;
        attackAnimationEnded = true;
        dieAnimationEnded = true;

        target = null;

        SetPlayer(null);
        lookAtPlayer = true;

        capacitorController = null;
        deviceController = null;

        lastShotDirection = Vector3.zero;
        lastShotSameColor = false;
        lastShotPlayer = null;

        groupInfo = null;
    }

    public void SetPlayer(GameObject p)
    {
        player = p;
        if (player != null)
            playerController = player.GetComponent<PlayerController>();
        else
            playerController = null;
    }

    public bool HaveHealthRemaining()
    {
        return currentHealth > 0 && currentHealthWrongColor > 0;
    }
}
