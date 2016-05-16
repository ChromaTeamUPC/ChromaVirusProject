using UnityEngine;
using System.Collections;

public class EnemyBaseAIBehaviour : MonoBehaviour {

    protected EnemyBaseBlackboard blackboard; //To be instantiated in inherited classes

    public int maxHealth = 30;

    [HideInInspector]
    public ChromaColor color;

    private VoxelizationClient voxelization;
    [HideInInspector]
    public Renderer rend;

    protected AIBaseState currentState;


    protected virtual void Awake()
    {
        voxelization = GetComponent<VoxelizationClient>();
        rend = GetComponentInChildren<Renderer>();
    }

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
            Debug.Log(this.GetType().Name + " Exiting: " + currentState.GetType().Name);
            currentState.OnStateExit();
        }
        currentState = newState;
        if (currentState != null)
        {
            Debug.Log(this.GetType().Name + " Entering: " + currentState.GetType().Name);
            currentState.OnStateEnter();
        }
    }

    public void ImpactedByShot(ChromaColor shotColor, int damage)
    {
        AIBaseState newState = currentState.ImpactedByShot(shotColor, damage);

        if (newState != null)
        {
            ChangeState(newState);
        }
    }

    public virtual AIBaseState ProcessShotImpact(ChromaColor shotColor, int damage)
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
