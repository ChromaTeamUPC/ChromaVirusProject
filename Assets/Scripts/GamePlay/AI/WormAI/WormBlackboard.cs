using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormBlackboard : MonoBehaviour
{
    public const int NAVMESH_FLOOR_LAYER = 32;
    public const int NAVMESH_UNDERGROUND_LAYER = 64;
    public const float NAVMESH_LAYER_HEIGHT = 8f;

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
    public GameObject sceneCenter;
    [HideInInspector]
    public HexagonController sceneCenterHexagon;

    [HideInInspector]
    public WormAISpawningState spawningState;
    public WormAIWanderingState wanderingState;
    public WormAIBelowAttackState belowAttackState;
    public WormAIDyingState dyingState;

    public WormAITestState testState;

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

    public GameObject enterBezierCtrl11;
    public GameObject enterBezierCtrl12;
    public GameObject enterBezierEnd1Start2;
    public GameObject enterBezierCtrl21;
    public GameObject enterBezierCtrl22;

    public GameObject exitBezierCtrl11;
    public GameObject exitBezierCtrl12;
    public GameObject exitBezierEnd1Start2;
    public GameObject exitBezierCtrl21;
    public GameObject exitBezierCtrl22;

    [HideInInspector]
    public Vector3 worldEnterBezierCtrl11;
    [HideInInspector]
    public Vector3 worldEnterBezierCtrl12;
    [HideInInspector]
    public Vector3 worldEnterBezierEnd1Start2;
    [HideInInspector]
    public Vector3 worldEnterBezierCtrl21;
    [HideInInspector]
    public Vector3 worldEnterBezierCtrl22;

    [HideInInspector]
    public Vector3 worldExitBezierCtrl11;
    [HideInInspector]
    public Vector3 worldExitBezierCtrl12;
    [HideInInspector]
    public Vector3 worldExitBezierEnd1Start2;
    [HideInInspector]
    public Vector3 worldExitBezierCtrl21;
    [HideInInspector]
    public Vector3 worldExitBezierCtrl22;


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
    [HideInInspector]
    public List<WormBodySegmentController> bodySegmentControllers;
    public Transform tailTrf;
    [HideInInspector]
    public WormTailController tail;
    [HideInInspector]
    public bool tailIsUnderground;
    public GameObject junctionPrefab;
    [HideInInspector]
    public Transform[] junctionsTrf;

    //public float headToSegmentDistance;
    public float headToJunctionDistance;
    //public float segmentToSegmentDistance;
    public float segmentToJunctionDistance;
    //public float segmentToTailDistance;
    public float tailToJunctionDistance;

    public float bodySettingMinTime = 1f;
    public float bodySettingMaxTime = 3f;
    public float bodySettingChangeTime = 0.1f;

    [Header("Attack Settings")]
    public float contactDamage = 12f;
    public Vector2 infectionForces;
    public float belowAttackWaitTime = 2f;
    public float belowAttackWarningTime = 0.5f;
    public float belowAttackRumbleDuration = 1f;

    [Header("Misc variables")]
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

        junctionsTrf = new Transform[bodySegmentsTrf.Length + 1];
        for(int i = 0; i < junctionsTrf.Length; ++i)
        {
            GameObject junction = Instantiate(junctionPrefab) as GameObject;
            junctionsTrf[i] = junction.transform;
        }

        spawningState = new WormAISpawningState(this);
        wanderingState = new WormAIWanderingState(this);
        belowAttackState = new WormAIBelowAttackState(this);
        dyingState = new WormAIDyingState(this);

        testState = new WormAITestState(this);

        ResetValues();
    }

    public void ResetValues()
    {
        wormPhase = 0;
        headCurrentHealth = headMaxHealth;
        headChargeLevel = 0;
    }

    public void StartNewPhase()
    {
        wormPhase++;
        //Debug.Log("Worm phase: " + wormPhase);
        headCurrentHealth = headMaxHealth;
        headChargeLevel = 0;
        ConsolidateBodyParts();
    }

    #region BodySegments
    public void InitBodyParts()
    {
        //Init each segment color
        for (int i = 0; i < bodySegmentControllers.Count; ++i)
        {
            bodySegmentControllers[i].SetInitialState();
        }
    }

    public void ShuffleBodyParts()
    {
        //Debug.Log("Shuffle body parts");
        List<WormBodySegmentController> randomized = new List<WormBodySegmentController>(bodySegmentControllers);

        randomized.Shuffle();
        randomized.Shuffle();

        //Init each segment color
        for (int i = 0; i < randomized.Count; ++i)
        {
            randomized[i].ResetColor((ChromaColor)(i % ChromaColorInfo.Count));
        }

        rsc.colorMng.PrintColors();
    }

    public void DisableBodyParts()
    {
        //Debug.Log("Disable body parts");
        for (int i = 0; i < bodySegmentControllers.Count; ++i)
        {
            bodySegmentControllers[i].Disable();
        }      
    }

    public void ConsolidateBodyParts()
    {
        //Debug.Log("Consolidate body parts");
        List<WormBodySegmentController> randomized = new List<WormBodySegmentController>(bodySegmentControllers);

        randomized.Shuffle();
        randomized.Shuffle();

        //Init each segment color
        for (int i = 0; i < randomized.Count; ++i)
        {
            randomized[i].Consolidate((ChromaColor)(i % ChromaColorInfo.Count));
        }

        rsc.colorMng.PrintColors();
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
    #endregion

    #region Parabola
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

    public Vector3 BezierQuadratic(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        //B(t) = (1-t)2P0 + 2(1-t)tP1 + t2P2 , 0 < t < 1

        return (Mathf.Pow(1 - t, 2) * start) +
               (2 * (1 - t) * t * control) +
               (Mathf.Pow(t, 2) * end);
    }

    public Vector3 BezierCubic(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, float t)
    {
        //B(t) = (1-t)3P0 + 3(1-t)2tP1 + 3(1-t)t2P2 + t3P3 , 0 < t < 1

        return (Mathf.Pow(1 - t, 3) * start) +
               (3 * Mathf.Pow(1 - t, 2) * t * control1) +
               (3 * (1 - t) * Mathf.Pow(t, 2) * control2) +
               (Mathf.Pow(t, 3) * end);

    }

    public void CalculateWorldEnterBezierPoints(Transform origin)
    {
        worldEnterBezierCtrl11 = origin.TransformPoint(enterBezierCtrl11.transform.position);
        worldEnterBezierCtrl12 = origin.TransformPoint(enterBezierCtrl12.transform.position);
        worldEnterBezierEnd1Start2 = origin.TransformPoint(enterBezierEnd1Start2.transform.position);
        worldEnterBezierCtrl21 = origin.TransformPoint(enterBezierCtrl21.transform.position);
        worldEnterBezierCtrl22 = origin.TransformPoint(enterBezierCtrl22.transform.position);
    }

    public void CalculateWorldExitBezierPoints(Transform origin)
    {
        worldExitBezierCtrl11 = origin.TransformPoint(exitBezierCtrl11.transform.position);
        worldExitBezierCtrl12 = origin.TransformPoint(exitBezierCtrl12.transform.position);
        worldExitBezierEnd1Start2 = origin.TransformPoint(exitBezierEnd1Start2.transform.position);
        worldExitBezierCtrl21 = origin.TransformPoint(exitBezierCtrl21.transform.position);
        worldExitBezierCtrl22 = origin.TransformPoint(exitBezierCtrl22.transform.position);
    }
    #endregion
}
