using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormBlackboard : MonoBehaviour
{
    public const int NAVMESH_FLOOR_LAYER = 32;
    public const int NAVMESH_UNDERGROUND_LAYER = 64;
    public const float NAVMESH_LAYER_HEIGHT = 6f;

    //Non resetable values
    [HideInInspector]
    public GameObject wormGO;
    [HideInInspector]
    public WormAIBehaviour worm;
    [HideInInspector]
    public NavMeshAgent agent;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Vector3 navMeshLayersDistance;

    [HideInInspector]
    public WormAIWanderingState wanderingState;

    public WormAIDyingState dyingState;

    //Reseteable values
    [Header("Health Settings")]
    public int wormMaxPhases = 4;
    [HideInInspector]
    public int wormPhase = 1;
    public float headMaxHealth = 100f;
    [HideInInspector]
    public float headCurrentHealth;
    public int headChargeMaxLevel = 3;
    [HideInInspector]
    public int headChargeLevel;
    public float bodyMaxHealth = 50;
    public float bodyWrongColorDamageModifier = 0.5f;

    [Header("Movement Settings")]
    public float floorSpeed = 10;
    public float undergroundSpeed = 10;
    public float rotationSpeed = 180;

    [Header("Body Settings")]
    public Transform headTrf;
    [HideInInspector]
    public WormAIBehaviour head; //same as worm variable
    public Transform[] bodySegmentsTrf;
    private List<WormBodySegmentController> bodySegmentControllers;
    public Transform tailTrf;
    [HideInInspector]
    public WormTailController tail;

    public float headToSegmentDistance;
    public float segmentToSegmentDistance;
    public float segmentToTailDistance;

    public float bodySettingMinTime = 1f;
    public float bodySettingMaxTime = 3f;
    public float bodySettingChangeTime = 0.1f;

    public void Awake()
    {
        wormGO = gameObject;
        worm = GetComponent<WormAIBehaviour>();
        head = worm;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        navMeshLayersDistance = new Vector3(0, WormBlackboard.NAVMESH_LAYER_HEIGHT, 0);
        tail = tailTrf.gameObject.GetComponent<WormTailController>();

        bodySegmentControllers = new List<WormBodySegmentController>();
        for (int i = 0; i < bodySegmentsTrf.Length; ++i)
        {
            WormBodySegmentController ctrl = bodySegmentsTrf[i].GetComponent<WormBodySegmentController>();
            ctrl.SetBlackboard(this);
            bodySegmentControllers.Add(ctrl);
        }

        wanderingState = new WormAIWanderingState(this);
        dyingState = new WormAIDyingState(this);

        ResetValues();
    }

    public void ResetValues()
    {
        wormPhase = 1;
        headCurrentHealth = headMaxHealth;
        headChargeLevel = 0;
    }

    void Start()
    {
        InitBodyParts();
    }

    public void InitBodyParts()
    {
        List<WormBodySegmentController> randomized = new List<WormBodySegmentController>(bodySegmentControllers);

        randomized.Shuffle();
        randomized.Shuffle();

        //Init each segment color
        for (int i = 0; i < randomized.Count; ++i)
        {
            randomized[i].SetInitialState((ChromaColor)(i % ChromaColorInfo.Count));
        }
    }

    public void ShuffleBodyParts()
    {
        List<WormBodySegmentController> randomized = new List<WormBodySegmentController>(bodySegmentControllers);

        randomized.Shuffle();
        randomized.Shuffle();

        //Init each segment color
        for (int i = 0; i < randomized.Count; ++i)
        {
            randomized[i].ResetColor((ChromaColor)(i % ChromaColorInfo.Count));
        }
    }

    public void DisableBodyParts()
    {
        for (int i = 0; i < bodySegmentControllers.Count; ++i)
        {
            bodySegmentControllers[i].Disable();
        }
    }

    public void ConsolidateBodyParts()
    {
        List<WormBodySegmentController> randomized = new List<WormBodySegmentController>(bodySegmentControllers);

        randomized.Shuffle();
        randomized.Shuffle();

        //Init each segment color
        for (int i = 0; i < randomized.Count; ++i)
        {
            randomized[i].Consolidate((ChromaColor)(i % ChromaColorInfo.Count));
        }
    }

    public void Explode()
    {
        StartCoroutine(SequentialExplode());
    }

    private IEnumerator SequentialExplode()
    {
        //tail explode
        tail.Explode();

        //bodyparts explode
        for(int i = bodySegmentControllers.Count -1; i >= 0; --i)
        {
            yield return new WaitForSeconds(0.2f);
            bodySegmentControllers[i].Explode();
        }

        //head explode
        yield return new WaitForSeconds(0.2f);
        worm.Explode();
    }
}
