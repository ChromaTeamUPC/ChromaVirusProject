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
    public WormAISpawningState spawningState;
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

    [Header("Jump Settings")]
    public float jumpOffset = 1.3f;
    public float jumpHeightToDistanceRatio = 1f;
    [HideInInspector]
    public Vector3 jumpOrigin;
    [HideInInspector]
    public Vector3 jumpDestiny;
    [HideInInspector]
    public Vector3 jumpCenter;
    [HideInInspector]
    public float jumpDistance;
    [HideInInspector]
    public float jumpHalfDistance;
    [HideInInspector]
    public float jumpMaxHeight;
    [HideInInspector]
    public float jumpParabolaAperture;
    [HideInInspector]
    public Vector3 jumpDirectionVector;

    [Header("Body Settings")]
    public Transform headTrf;
    [HideInInspector]
    public WormAIBehaviour head; //same as worm variable
    public Transform[] bodySegmentsTrf;
    private List<WormBodySegmentController> bodySegmentControllers;
    public Transform tailTrf;
    [HideInInspector]
    public WormTailController tail;
    [HideInInspector]
    public bool tailIsUnderground;

    public float headToSegmentDistance;
    public float segmentToSegmentDistance;
    public float segmentToTailDistance;

    public float bodySettingMinTime = 1f;
    public float bodySettingMaxTime = 3f;
    public float bodySettingChangeTime = 0.1f;

    [Header("Test variables")]
    public GameObject spawnEntry;
    public GameObject spawnExit;

    public void Awake()
    {
        wormGO = gameObject;
        worm = GetComponent<WormAIBehaviour>();
        head = worm;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        navMeshLayersDistance = new Vector3(0, WormBlackboard.NAVMESH_LAYER_HEIGHT, 0);
        tail = tailTrf.gameObject.GetComponent<WormTailController>();
        tail.SetBlackboard(this);

        bodySegmentControllers = new List<WormBodySegmentController>();
        for (int i = 0; i < bodySegmentsTrf.Length; ++i)
        {
            WormBodySegmentController ctrl = bodySegmentsTrf[i].GetComponent<WormBodySegmentController>();
            ctrl.SetBlackboard(this);
            bodySegmentControllers.Add(ctrl);
        }

        spawningState = new WormAISpawningState(this);
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

    public void CalculateParabola()
    {
        /*Info needed to apply the formula ax^2 + bx + c = y:
         * We can ignore bx completely
         * Origin on y = 0. (Entry hexagon. Should be assigned before call this function)
         * Destiny on y = 0. (Exit hexagon. Should be assigned before call this function)
         * Distance between Origin and Destiny
         * Half distance. this will be the x1 and x2 for the formula y to be 0
         * Desired max height. Could be proportional to distance. This will be the c factor.
         * Parabola aperture. This will be the a factor
         */
        jumpDirectionVector = (jumpDestiny - jumpOrigin).normalized;

        Vector3 offsetVector = jumpDirectionVector * jumpOffset;
        jumpOrigin += offsetVector;
        jumpDestiny -= offsetVector;

        jumpCenter = jumpOrigin + ((jumpDestiny - jumpOrigin) / 2);
        jumpDistance = (jumpOrigin - jumpDestiny).magnitude;
        jumpHalfDistance = jumpDistance / 2;

        jumpMaxHeight = jumpDistance * jumpHeightToDistanceRatio;

        jumpParabolaAperture = -jumpMaxHeight / (jumpHalfDistance * jumpHalfDistance);

    }

    public float GetJumpYGivenX(float x)
    {
        /* a*x^2 + c = y
           jumpParabolaAperture * x^2 + jumpMaxHeight = y/
         */

        return (jumpParabolaAperture * (x * x)) + jumpMaxHeight;
    }

    public float GetJumpXGivenY(float y, bool positiveSide = true)
    {
        /* a*x^2 + c = y
           jumpParabolaAperture * x^2 + jumpMaxHeight = y
           x = +- sqrt(y - jumpMaxHeight / jumpParabolaAperture)
         */

        float x = Mathf.Sqrt((y - jumpMaxHeight) / jumpParabolaAperture);
        if (!positiveSide)
            x = -x;

        return x;
    }

    private Vector3 GetJumpPosition(float x, float y)
    {
        Vector3 direction = jumpDirectionVector * x;
        Vector3 position = jumpCenter + direction;
        position.y = y;

        return position;
    }
    
    public Vector3 GetJumpPositionGivenX(float x)
    {
        float y = GetJumpYGivenX(x);

        return GetJumpPosition(x, y);
    }

    public Vector3 GetJumpPositionGivenY(float y, bool positiveSide = true)
    {
        float x = GetJumpXGivenY(y, positiveSide);

        return GetJumpPosition(x, y);
    }
}
