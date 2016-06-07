using UnityEngine;
using System.Collections;

public class EnemyBaseAIBehaviour : MonoBehaviour {

    protected EnemyBaseBlackboard blackboard; //To be instantiated in inherited classes 

    [Header("Enemy Common Settings")]
    public int maxHealth = 30;

    public float shotForceModifier = 1f;

    public int energyVoxelsSpawnedOnDie = 0;

    [HideInInspector]
    public ChromaColor color;

    private VoxelizationClient voxelization;
    [HideInInspector]
    public Renderer rend;
    [HideInInspector]
    public Collider mainCollider;
    [HideInInspector]
    public GameObject dyingCollider;

    protected AIBaseState currentState;

    protected BlinkController blinkController;


    protected virtual void Awake()
    {
        voxelization = GetComponentInChildren<VoxelizationClient>();
        rend = GetComponentInChildren<Renderer>();
        mainCollider = GetComponent<Collider>();
        dyingCollider = transform.Find("DyingCollider").gameObject;
        blinkController = GetComponent<BlinkController>();
    }

    public virtual void SetMaterials(Material[] materials) { }

    public void AnimationEnded()
    {
        blackboard.animationEnded = true;
    }

    public void AnimationTrigger()
    {
        blackboard.animationTrigger = true;
    }

    public void SpawnVoxels()
    {
        if (blackboard.lastShotSameColor)
            voxelization.SetMaterial(color);
        else
        {
            voxelization.SetGreyMaterial();
            voxelization.SpawnColliderThisTime = false;
        }

        voxelization.CalculateVoxelsGrid();
        voxelization.SpawnVoxels();
    }

    protected void ChangeState(AIBaseState newState)
    {
        if (currentState != null)
        {
            //Debug.Log(this.GetType().Name + " Exiting: " + currentState.GetType().Name);
            currentState.OnStateExit();
        }
        currentState = newState;
        if (currentState != null)
        {
            //Debug.Log(this.GetType().Name + " Entering: " + currentState.GetType().Name);
            currentState.OnStateEnter();
        }
    }

    public void ImpactedByShot(ChromaColor shotColor, int damage, Vector3 direction, PlayerController player)
    {
        AIBaseState newState = currentState.ImpactedByShot(shotColor, damage, direction, player);

        if (newState != null)
        {
            ChangeState(newState);
        }
    }

    public virtual AIBaseState ProcessShotImpact(ChromaColor shotColor, int damage, Vector3 direction, PlayerController player)
    {
        return null;
    }

    public void ImpactedByBarrel(ChromaColor barrelColor, int damage, Vector3 direction)
    {
        AIBaseState newState = currentState.ImpactedByBarrel(barrelColor, damage, direction);

        if (newState != null)
        {
            ChangeState(newState);
        }
    }

    public virtual AIBaseState ProcessBarrelImpact(ChromaColor barrelColor, int damage, Vector3 direction)
    {
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BarrelAttractor")
        {
            blackboard.barrelController = other.GetComponent<CapacitorAttractor>().controller;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "BarrelAttractor")
        {
            blackboard.barrelController = null;
        }
    }

    public virtual void CollitionOnDie() { }
}
