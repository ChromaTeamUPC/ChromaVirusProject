using UnityEngine;
using System.Collections;

public class EnemyBaseAIBehaviour : MonoBehaviour {

    protected EnemyBaseBlackboard blackboard; //To be instantiated in inherited classes

    [Header("Enemy Common Settings")]
    public int maxHealth = 30;

    public float shotForceModifier = 1f;

    [HideInInspector]
    public ChromaColor color;

    private VoxelizationClient voxelization;
    [HideInInspector]
    public Renderer rend;
    [HideInInspector]
    public Collider mainCollider;

    protected AIBaseState currentState;

    protected BlinkController blinkController;


    protected virtual void Awake()
    {
        voxelization = GetComponentInChildren<VoxelizationClient>();
        rend = GetComponentInChildren<Renderer>();
        mainCollider = GetComponent<Collider>();
        blinkController = GetComponent<BlinkController>();
    }

    public virtual void SetMaterials(Material[] materials) { }

    public void AnimationEnded()
    {
        blackboard.animationEnded = true;
    }

    public void SpawnVoxels()
    {
        voxelization.SetMaterial(color);
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

    public void ImpactedByShot(ChromaColor shotColor, int damage, Vector3 direction)
    {
        AIBaseState newState = currentState.ImpactedByShot(shotColor, damage, direction);

        if (newState != null)
        {
            ChangeState(newState);
        }
    }

    public virtual AIBaseState ProcessShotImpact(ChromaColor shotColor, int damage, Vector3 direction)
    {
        return null;
    }

    public void ImpactedByBarrel(ChromaColor barrelColor, int damage)
    {
        AIBaseState newState = currentState.ImpactedByBarrel(barrelColor, damage);

        if (newState != null)
        {
            ChangeState(newState);
        }
    }

    public virtual AIBaseState ProcessBarrelImpact(ChromaColor barrelColor, int damage)
    {
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BarrelAttractor")
        {
            blackboard.barrelController = other.GetComponent<BarrelAttractor>().controller;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "BarrelAttractor")
        {
            blackboard.barrelController = null;
        }
    }
}
