using UnityEngine;
using System.Collections;

public class WormBlackboard
{
    public const int NAVMESH_FLOOR_LAYER = 32;
    public const int NAVMESH_UNDERGROUND_LAYER = 64;
    public const float NAVMESH_LAYER_HEIGHT = 6f;

    //Non resetable values
    public GameObject wormGO;
    public WormAIBehaviour worm;
    public NavMeshAgent agent;
    public Animator animator;
    public Vector3 navMeshLayersDistance;

    public WormAIWanderingState wanderingState;

    //Reseteable values

    public void InitialSetup(GameObject e)
    {
        wormGO = e;
        worm = wormGO.GetComponent<WormAIBehaviour>();
        agent = wormGO.GetComponent<NavMeshAgent>();
        animator = wormGO.GetComponent<Animator>();
        navMeshLayersDistance = new Vector3(0, WormBlackboard.NAVMESH_LAYER_HEIGHT, 0);

        wanderingState = new WormAIWanderingState(this);

        ResetValues();
    }

    public void ResetValues()
    {

    }
}
