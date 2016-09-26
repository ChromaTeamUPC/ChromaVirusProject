using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WormBlackboard : MonoBehaviour 
{
    public const int NAVMESH_FLOOR_LAYER = 32;
    public const int NAVMESH_UNDERGROUND_LAYER = 64;
    public const float NAVMESH_LAYER_HEIGHT = 8f;

    [Serializable]
    public class HealthSettings
    {
        public float headMaxHealth = 100f;
        public float bodyMaxHealth = 50;
        public float bodyWrongColorDamageModifier = 0.5f;
        public float knockOutTime = 7f;
    }

    [Serializable]
    public class SpawningMinionsSettings
    {
        public int chancesOfSpawningMinion = 33;
        public int maxMinionsOnScreen = 3;
        public float cooldownTime = 3f;
    }

    [Serializable]
    public class WanderingSettings
    {
        public float initialWaitTime = 0.25f;
        public int routeMinId = 0;
        public int routeMaxId = 1;
        public float wanderingSpeed = 10;
        public float sinLongitude = 3f;
        public float sinAmplitude = 0.25f;
        public float sinCycleDuration = 30f;
    }

    [Serializable]
    public class BelowAttackSettings
    {
        [Header("Trigger Settings")]
        public bool active = true;
        public float chancesOfBelowAttackAfterWandering = 50f;

        [Header("Attack Settings")]
        public float initialWaitTime = 2f;
        public float warningTime = 1f;
        public int adjacentDamagingCells = 3;
        public float jumpSpeed = 10f;
        public float rotationSpeed = 180f;
    }

    [Serializable]
    public class AboveAttackSettings
    {
        [Header("Trigger Settings")]
        public bool active = true;
        public float exposureTimeNeeded = 3f;
        public float exposureMaxAngle = 135f;
        public float exposureMinHexagons = 0;
        public float exposureMaxHexagons = 5;
        public float attackMinHexagons = 2;
        public float attackMaxHexagons = 5;
        public float cooldownTime = 10f;

        [Header("Attack Settings")]
        public float warningTime = 0.5f;
        public float jumpDuration = 1f;
        public float selfRotation = 180f;
    }

    [Serializable]
    public class MeteorAttackSettings
    {
        [Header("Trigger Settings")]
        public bool active = true;
        public bool triggerAfterHeadDestroyed = false;
        public int triggerAfterNumberOfBodySegmentsDestroyed = 0;

        [Header("Worm Settings")]
        public float warningTime = 0.75f;
        public float enterHeadSpeed = 20f;
        public int numberOfThrownMeteors = 3;
        public float speedOfThrownMeteors = 30f;
        public float timeShooting = 1f;
        public float jumpDuration = 1f;
        public float rotationSpeed = 360;

        [Header("Meteor Rain Settings")]
        public int meteorInitialBurst = 20;
        public float meteorRainDuration = 5f;
        public float meteorInterval = 0.2f;
        public int meteorsPerInterval = 5;

        [Header("Meteor Item Settings")]
        public int meteorWaitTime = 0;
        public float meteorWarningTime = 2f;
    }

    #region Settings
    [Header("Debug Settings")]
    public bool attacksEnabled = true;

    [Header("Scene Related Settings")]
    [HideInInspector]
    public Vector3 navMeshLayersDistance;
    [HideInInspector]
    public GameObject sceneCenter;
    [HideInInspector]
    public HexagonController sceneCenterHexagon;

    [Header("Body Construction Settings")]
    public Vector3 initialPosition;
    public GameObject bodyPrefab;
    public GameObject junctionPrefab;
    public int bodyParts = 12;
    public float headToJunctionDistance = 3;
    public float segmentToJunctionDistance = 2;
    public float tailToJunctionDistance = 1.6f;
    public int wormMaxPhases = 4;
    [HideInInspector]
    public int wormCurrentPhase = -1;
    public int headMaxChargeLevel = 3;
    [HideInInspector]
    public int headCurrentChargeLevel;

    //Head
    [HideInInspector]
    public Transform headTrf;
    [HideInInspector]
    public WormAIBehaviour head;

    //Body parts
    [HideInInspector]
    public Transform bodySegmentsGroupTrf;

    [HideInInspector]
    public Transform[] bodySegmentsTrf;
    [HideInInspector]
    public WormBodySegmentController[] bodySegmentControllers;

    //Junctions
    [HideInInspector]
    public Transform junctionsGroupTrf;

    [HideInInspector]
    public Transform[] junctionsTrf;
    [HideInInspector]
    public WormJunctionController[] junctionControllers;

    //Tail
    [HideInInspector]
    public Transform tailTrf;
    [HideInInspector]
    public WormTailController tailController;

    private WormWayPoint headWayPoint;

    [Header("Spawn Settings")]
    public float spawnJumpToHeightRatio = 1f;
    public float spawnSpeed = 8f;

    [Header("Health Settings")]
    [SerializeField]
    public HealthSettings[] healthSettings = new HealthSettings[4];
    public HealthSettings HealthSettingsPhase { get { return healthSettings[wormCurrentPhase]; } }
    public float headVulnerableBlinkInterval = 0.4f;
    public float knockOutBlinkInterval = 0.2f;
    public int knockOutEnergyVoxelsSpawned = 20;
    public float knockOutHexagonsRotationSpeed = 360f;
    [HideInInspector]
    public float headCurrentDamage;

    [Header("Body Changing Color Settings")]
    public float bodyColorsCarrouselMinTime = 1f;
    public float bodyColorsCarrouselMaxTime = 3f;
    public float bodyColorsCarrouselChangeInterval = 0.1f;

    [Header("Head Destroyed Settings")]
    public float headDestroyedWaitTime = 3f;
    public float headDestoryedBodyWaitTime = 1f;
    public float headDestroyedLookRotationSpeed = 50;
    public float headDestroyedJumpDuration = 1f;
    public float headDestroyedRotationSpeed = 360f;

    [Header("Jump Settings")]
    public float jumpOffset = 0f;
    public float jumpHeightToDistanceRatio = 0.5f;
    [HideInInspector]
    public float realJumpHeightToDistanceRatio = 0.5f;
    private Vector3 jumpOrigin;
    private Vector3 jumpDestiny;
    private Vector3 jumpCenter;
    private float jumpDistance;
    private float jumpHalfDistance;
    private float jumpMaxHeight;
    private float jumpParabolaAperture;
    private Vector3 jumpDirectionVector;

    [Header("Contact Settings")]
    public float contactDamage = 12f;
    public Vector2 infectionForces = new Vector2(5, 9);
    public float attackRumbleDuration = 1f;

    [Header("Spawning Minions Settings")]
    [SerializeField]
    public SpawningMinionsSettings[] spawningMinionsSettings = new SpawningMinionsSettings[4];
    public SpawningMinionsSettings SpawningMinionsSettingsPhase { get { return spawningMinionsSettings[wormCurrentPhase]; } }
    [HideInInspector]
    public float spawningMinionsCurrentCooldownTime;

    [Header("Wandering Settings")]
    [SerializeField]
    public WanderingSettings[] wanderingSettings = new WanderingSettings[4];
    public WanderingSettings WanderingSettingsPhase { get { return wanderingSettings[wormCurrentPhase]; } }
    [HideInInspector]
    public float sinDistanceFactor;
    [HideInInspector]
    public float sinTimeFactor;
    [HideInInspector]
    public float sinTimeOffset;
    [HideInInspector]
    public float sinElapsedTime;
    [HideInInspector]
    public bool applySinMovement;
    [HideInInspector]
    public bool isHeadOverground;
    [HideInInspector]
    public bool tailReachedMilestone;

    //Local bezier points
    private Vector3 localEnterBezier11;
    private Vector3 localEnterBezier12;
    private Vector3 localEnterBezierMiddle;
    private Vector3 localEnterBezier21;
    private Vector3 localEnterBezier22;

    private Vector3 localExitBezier11;
    private Vector3 localExitBezier12;
    private Vector3 localExitBezierMiddle;
    private Vector3 localExitBezier21;
    private Vector3 localExitBezier22;

    //World bezier points
    private Vector3 worldEnterBezier11;
    private Vector3 worldEnterBezier12;
    private Vector3 worldEnterBezierMiddle;
    private Vector3 worldEnterBezier21;
    private Vector3 worldEnterBezier22;

    private Vector3 worldExitBezier11;
    private Vector3 worldExitBezier12;
    private Vector3 worldExitBezierMiddle;
    private Vector3 worldExitBezier21;
    private Vector3 worldExitBezier22;

    [Header("Below Attack Settings")]
    [SerializeField]
    public BelowAttackSettings[] belowAttackSettings = new BelowAttackSettings[4];
    public BelowAttackSettings BelowAttackSettingsPhase { get { return belowAttackSettings[wormCurrentPhase]; } }

    [Header("Above Attack Settings")]
    [SerializeField]
    public AboveAttackSettings[] aboveAttackSettings = new AboveAttackSettings[4];
    public AboveAttackSettings AboveAttackSettingsPhase { get { return aboveAttackSettings[wormCurrentPhase]; } } 
    [HideInInspector]
    public float aboveAttackCurrentExposureTime;
    [HideInInspector]
    public float aboveAttackCurrentCooldownTime;

    [Header("Meteor Attack Settings")]
    [SerializeField]
    public MeteorAttackSettings[] meteorAttackSettings = new MeteorAttackSettings[4];
    public MeteorAttackSettings MeteorAttackSettingsPhase { get { return meteorAttackSettings[wormCurrentPhase]; } }
    [HideInInspector]
    public bool shouldMeteorBeTriggedAfterWandering;
    [HideInInspector]
    public bool meteorInmediate;


    [Header("Misc variables")]
    public GameObject spawnEntry;
    public GameObject spawnExit;
    public GameObject bezierCurvesPrefab;

    [HideInInspector]
    public PlayerController killerPlayer;
    [HideInInspector]
    public PlayerController playerInSight;
    #endregion

    void OnDrawGizmos()
    {
        //Uncomment to view in scene the initial parabola of the boss

        /*realJumpHeightToDistanceRatio = spawnJumpToHeightRatio;
        CalculateParabola(spawnEntry.transform.position, spawnExit.transform.position);
        Vector3 highestPoint = GetJumpPositionGivenX(0);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawCube(highestPoint, Vector3.one);

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(spawnEntry.transform.position, Vector3.one);

        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(spawnExit.transform.position, Vector3.one);

        Gizmos.color = Color.grey;
        float half = GetJumpHalfDistance();

        for (int i = 1; i < 10; ++i)
        {
            Vector3 point = GetJumpPositionGivenX(half / 10 * i);
            Gizmos.DrawCube(point, Vector3.one / 2);

            point = GetJumpPositionGivenX(-half / 10 * i);
            Gizmos.DrawCube(point, Vector3.one / 2);
        }*/
    }

    void Awake () 
	{
        navMeshLayersDistance = new Vector3(0, WormBlackboard.NAVMESH_LAYER_HEIGHT, 0);

        headTrf = transform.FindDeepChild("Head");
        head = headTrf.GetComponent<WormAIBehaviour>();
        head.SetBlackboard(this);

        bodySegmentsGroupTrf = transform.FindDeepChild("BodyParts");
        bodySegmentsTrf = new Transform[bodyParts];
        bodySegmentControllers = new WormBodySegmentController[bodyParts];

        for (int i = 0; i < bodySegmentsTrf.Length; ++i)
        {
            GameObject bodySegment = Instantiate(bodyPrefab, initialPosition, Quaternion.identity) as GameObject;
            bodySegmentsTrf[i] = bodySegment.transform;
            bodySegmentsTrf[i].SetParent(bodySegmentsGroupTrf);
            bodySegmentControllers[i] = bodySegment.GetComponent<WormBodySegmentController>();
            bodySegmentControllers[i].SetBlackboard(this);
        }

        junctionsGroupTrf = transform.FindDeepChild("Junctions");

        junctionsTrf = new Transform[bodyParts + 1];
        junctionControllers = new WormJunctionController[bodyParts + 1];
        for (int i = 0; i < junctionsTrf.Length; ++i)
        {
            GameObject junction = Instantiate(junctionPrefab, initialPosition, Quaternion.identity) as GameObject;
            junctionsTrf[i] = junction.transform;
            junctionsTrf[i].SetParent(junctionsGroupTrf);
            junctionControllers[i] = junction.GetComponent<WormJunctionController>();
        }

        tailTrf = transform.FindDeepChild("Tail");
        tailController = tailTrf.GetComponent<WormTailController>();
        tailController.SetBlackboard(this);

        GameObject bezierContainer = Instantiate(bezierCurvesPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        Transform bezierTrf = bezierContainer.transform;
        localEnterBezier11 = bezierTrf.FindDeepChild("Enter11").position;
        localEnterBezier12 = bezierTrf.FindDeepChild("Enter12").position;
        localEnterBezierMiddle = bezierTrf.FindDeepChild("EnterMiddle").position;
        localEnterBezier21 = bezierTrf.FindDeepChild("Enter21").position;
        localEnterBezier22 = bezierTrf.FindDeepChild("Enter22").position;

        localExitBezier11 = bezierTrf.FindDeepChild("Exit11").position;
        localExitBezier12 = bezierTrf.FindDeepChild("Exit12").position;
        localExitBezierMiddle = bezierTrf.FindDeepChild("ExitMiddle").position;
        localExitBezier21 = bezierTrf.FindDeepChild("Exit21").position;
        localExitBezier22 = bezierTrf.FindDeepChild("Exit22").position;

        sinDistanceFactor = 360 / WanderingSettingsPhase.sinLongitude;
        sinTimeFactor = 360 / WanderingSettingsPhase.sinCycleDuration;

        ResetValues();
        SetInitialBodyWayPoints();
    }

    public void ResetValues()
    {
        isHeadOverground = false;
        wormCurrentPhase = -1;
        //headCurrentHealth = headMaxHealth;
        headCurrentChargeLevel = 0;
        spawningMinionsCurrentCooldownTime = 0f;
        aboveAttackCurrentCooldownTime = 0f;
        aboveAttackCurrentExposureTime = 0f;
        sinElapsedTime = 0;
        tailReachedMilestone = false;
        applySinMovement = false;
    }

    public void Init(GameObject screenCnt)
    {
        sceneCenter = screenCnt;
        sceneCenterHexagon = sceneCenter.GetComponent<HexagonController>();

        InitBodyParts();
        head.Init();
    }

    //Body segments management
    #region Body Segments
    public void InitBodyParts()
    {
        //Init each segment color
        for (int i = 0; i < bodySegmentControllers.Length; ++i)
        {
            bodySegmentControllers[i].SetInitialState((ChromaColor)(i % ChromaColorInfo.Count));
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

        //rsc.colorMng.PrintColors();
    }

    public void DisableBodyParts()
    {
        //Debug.Log("Disable body parts");
        for (int i = 0; i < bodySegmentControllers.Length; ++i)
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

        //rsc.colorMng.PrintColors();
        for (int i = 0; i < bodySegmentControllers.Length; ++i)
        {
            if (bodySegmentControllers[i].IsDestroyed())
            {
                junctionControllers[i].SetWireframe();
                junctionControllers[i + 1].SetWireframe();
            }
        }
    }

    public void Explode()
    {
        StartCoroutine(SequentialExplode());
    }

    private IEnumerator SequentialExplode()
    {
        //tail explode
        tailController.Explode();
        //Destroy(junctionsTrf[junctionsTrf.Length-1].gameObject);
        junctionsTrf[junctionsTrf.Length - 1].gameObject.SetActive(false);

        //bodyparts explode
        for (int i = bodySegmentControllers.Length - 1; i >= 0; --i)
        {
            yield return new WaitForSeconds(0.2f);
            //Destroy(junctionsTrf[i].gameObject);
            junctionsTrf[i].gameObject.SetActive(false);
            bodySegmentControllers[i].Explode();
        }

        //head explode
        yield return new WaitForSeconds(0.2f);
        head.Explode();
    }
    #endregion

    //Parabola calculation functions
    #region Parabola
    public void CalculateParabola(Vector3 origin, Vector3 destiny)
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
        jumpOrigin = origin;
        jumpDestiny = destiny;

        jumpDirectionVector = (jumpDestiny - jumpOrigin).normalized;

        Vector3 offsetVector = jumpDirectionVector * jumpOffset;
        jumpOrigin += offsetVector;
        jumpDestiny -= offsetVector;

        jumpCenter = jumpOrigin + ((jumpDestiny - jumpOrigin) / 2);
        jumpDistance = (jumpOrigin - jumpDestiny).magnitude;
        jumpHalfDistance = jumpDistance / 2;

        jumpMaxHeight = jumpDistance * realJumpHeightToDistanceRatio;

        jumpParabolaAperture = -jumpMaxHeight / (jumpHalfDistance * jumpHalfDistance);

    }

    public float GetJumpDistance()
    {
        return jumpDistance;
    }

    public float GetJumpHalfDistance()
    {
        return jumpHalfDistance;
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
    #endregion

    //Bezier curves functions
    #region Bezier curves
    public Vector3 GetEnterCurvePosition(Vector3 start, Vector3 end, float t)
    {
        //0 < t < 1 => first enter curve
        if (t < 1)
        {
            return BezierCubic(start, worldEnterBezier11, worldEnterBezier12, worldEnterBezierMiddle, t);
        }
        // t = 1 => middle point
        else if (t == 1)
        {
            return worldEnterBezierMiddle;
        }
        //1 < t < 2 => second enter curve
        else if (t <= 2)
        {
            t -= 1;
            return BezierCubic(worldEnterBezierMiddle, worldEnterBezier21, worldEnterBezier22, end, t);
        }

        return Vector3.zero;
    }

    public Vector3 GetExitCurvePosition(Vector3 start, Vector3 end, float t)
    {
        //0 < t < 1 => first enter curve
        if (t < 1)
        {
            return BezierCubic(start, worldExitBezier11, worldExitBezier12, worldExitBezierMiddle, t);
        }
        // t = 1 => middle point
        else if (t == 1)
        {
            return worldEnterBezierMiddle;
        }
        //1 < t < 2 => second enter curve
        else if (t <= 2)
        {
            t -= 1;
            return BezierCubic(worldExitBezierMiddle, worldExitBezier21, worldExitBezier22, end, t);
        }

        return Vector3.zero;
    }

    private Vector3 BezierQuadratic(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        //B(t) = (1-t)2P0 + 2(1-t)tP1 + t2P2 , 0 < t < 1

        return (Mathf.Pow(1 - t, 2) * start) +
               (2 * (1 - t) * t * control) +
               (Mathf.Pow(t, 2) * end);
    }

    private Vector3 BezierCubic(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, float t)
    {
        //B(t) = (1-t)3P0 + 3(1-t)2tP1 + 3(1-t)t2P2 + t3P3 , 0 < t < 1

        return (Mathf.Pow(1 - t, 3) * start) +
               (3 * Mathf.Pow(1 - t, 2) * t * control1) +
               (3 * (1 - t) * Mathf.Pow(t, 2) * control2) +
               (Mathf.Pow(t, 3) * end);

    }

    public void CalculateWorldEnterBezierPoints(Transform origin)
    {
        worldEnterBezier11 = origin.TransformPoint(localEnterBezier11);
        worldEnterBezier12 = origin.TransformPoint(localEnterBezier12);
        worldEnterBezierMiddle = origin.TransformPoint(localEnterBezierMiddle);
        worldEnterBezier21 = origin.TransformPoint(localEnterBezier21);
        worldEnterBezier22 = origin.TransformPoint(localEnterBezier22);
    }

    public void CalculateWorldExitBezierPoints(Transform origin)
    {
        worldExitBezier11 = origin.TransformPoint(localExitBezier11);
        worldExitBezier12 = origin.TransformPoint(localExitBezier12);
        worldExitBezierMiddle = origin.TransformPoint(localExitBezierMiddle);
        worldExitBezier21 = origin.TransformPoint(localExitBezier21);
        worldExitBezier22 = origin.TransformPoint(localExitBezier22);
    }
    #endregion

    //Body movement functions
    #region BodyMovement
    private void SetInitialBodyWayPoints()
    {
        headWayPoint = null;

        if (tailTrf != null)
        {
            headWayPoint = new WormWayPoint(tailTrf.position, tailTrf.rotation, false);
        }

        for (int i = bodySegmentsTrf.Length - 1; i >= 0; --i)
        {
            WormWayPoint segmentWayPoint = new WormWayPoint(bodySegmentsTrf[i].position, bodySegmentsTrf[i].rotation, false, (headWayPoint != null ? headWayPoint : null));
            headWayPoint = segmentWayPoint;
        }

        if (headTrf != null)
        {
            headWayPoint = new WormWayPoint(headTrf.position, headTrf.rotation, false, (headWayPoint != null ? headWayPoint : null));
        }
    }

    public void FlagCurrentWaypointAsMilestone()
    {
        headWayPoint.milestone = true;
    }

    public void UpdateBodyMovement()
    {
        sinDistanceFactor = 360 / WanderingSettingsPhase.sinLongitude;
        sinTimeFactor = 360 / WanderingSettingsPhase.sinCycleDuration;

        //If head has moved, create a new waypoint and recalculate all segments' position
        if ((headTrf.position != headWayPoint.position))
        {
            //Update sin time
            sinElapsedTime += Time.deltaTime;
            if (sinElapsedTime >= WanderingSettingsPhase.sinCycleDuration)
                sinElapsedTime -= WanderingSettingsPhase.sinCycleDuration;
            sinTimeOffset = sinElapsedTime * sinTimeFactor;

            headWayPoint = new WormWayPoint(headTrf.position, headTrf.rotation, head.IsVisible(), headWayPoint);

            WormWayPoint current = headWayPoint;
            WormWayPoint next = current.next;
            //if we are in the last waypoint, there is nothing more we can do, so we quit
            if (next == null) return;

            float totalDistance = headToJunctionDistance;                            //Total distance we have to position the element from the head
            float consolidatedDistance = 0f;                                        //Sum of the distances of evaluated waypoints
            float distanceBetween = (current.position - next.position).magnitude;   //Distance between current current and next waypoints

            float effectiveDistance;
            float totalOffset = 0f;
            if (applySinMovement)
            {
                totalOffset = (Mathf.Sin((totalDistance * sinDistanceFactor) + sinTimeOffset) * WanderingSettingsPhase.sinAmplitude);
                effectiveDistance = totalDistance + totalOffset;
            }
            else
            {
                effectiveDistance = totalDistance;
            }

            //move each body segment through the virtual line
            for (int i = 0; i < bodySegmentsTrf.Length; ++i)
            {
                //---- Junction ----
                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < effectiveDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                float remainingDistance = effectiveDistance - consolidatedDistance;
                Vector3 direction = (next.position - current.position).normalized * remainingDistance;

                junctionsTrf[i].position = current.position + direction;
                junctionsTrf[i].rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                //junctionsTrf[i].Translate(0f, -totalOffset, 0f, Space.Self);
                junctionsTrf[i].gameObject.SetActive(current.visible || next.visible);

                //---- Body ----
                totalDistance += segmentToJunctionDistance;
                if (applySinMovement)
                {
                    totalOffset = (Mathf.Sin((totalDistance * sinDistanceFactor) + sinTimeOffset) * WanderingSettingsPhase.sinAmplitude);
                    effectiveDistance = totalDistance + totalOffset;
                }
                else
                {
                    effectiveDistance = totalDistance;
                }

                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < effectiveDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                remainingDistance = effectiveDistance - consolidatedDistance;
                direction = (next.position - current.position).normalized * remainingDistance;

                bodySegmentsTrf[i].position = current.position + direction;
                bodySegmentsTrf[i].rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                //bodySegmentsTrf[i].Translate(0f, -totalOffset, 0f, Space.Self);
                bodySegmentControllers[i].SetVisible(current.visible || next.visible);

                //if it was the final body part and there is no tail, release the oldest waypoints
                if (i == bodySegmentsTrf.Length - 1)
                {
                    if (tailTrf == null)
                        next.next = null; //Remove reference, let garbage collector do its job
                }
                //else add total distance for the next iteration
                else
                {
                    totalDistance += segmentToJunctionDistance;
                    if (applySinMovement)
                    {
                        totalOffset = (Mathf.Sin((totalDistance * sinDistanceFactor) + sinTimeOffset) * WanderingSettingsPhase.sinAmplitude);
                        effectiveDistance = totalDistance + totalOffset;
                    }
                    else
                    {
                        effectiveDistance = totalDistance;
                    }
                }
            }

            //finally do the same for the tail
            if (tailTrf != null)
            {
                //---- Junction ----
                totalDistance += segmentToJunctionDistance;
                if (applySinMovement)
                {
                    totalOffset = (Mathf.Sin((totalDistance * sinDistanceFactor) + sinTimeOffset) * WanderingSettingsPhase.sinAmplitude);
                    effectiveDistance = totalDistance + totalOffset;
                }
                else
                {
                    effectiveDistance = totalDistance;
                }

                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < effectiveDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                float remainingDistance = effectiveDistance - consolidatedDistance;
                Vector3 direction = (next.position - current.position).normalized * remainingDistance;

                junctionsTrf[junctionsTrf.Length - 1].position = current.position + direction;
                junctionsTrf[junctionsTrf.Length - 1].rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                junctionsTrf[junctionsTrf.Length - 1].gameObject.SetActive(current.visible || next.visible);

                //---- Tail ----
                totalDistance += tailToJunctionDistance;
                if (applySinMovement)
                {
                    totalOffset = (Mathf.Sin((totalDistance * sinDistanceFactor) + sinTimeOffset) * WanderingSettingsPhase.sinAmplitude);
                    effectiveDistance = totalDistance + totalOffset;
                }
                else
                {
                    effectiveDistance = totalDistance;
                }

                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < effectiveDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                remainingDistance = effectiveDistance - consolidatedDistance;
                direction = (next.position - current.position).normalized * remainingDistance;

                tailTrf.position = current.position + direction;
                tailTrf.rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                tailController.SetVisible(current.visible || next.visible);

                //search for milestones
                WormWayPoint aux = next;
                while(aux != null)
                {
                    if(aux.milestone)
                    {
                        aux.milestone = false;
                        tailReachedMilestone = true;
                        break;
                    }
                    aux = aux.next;
                }
                aux = null;

                //release the oldest waypoints
                next.next = null; //Remove reference, let garbage collector do its job
            }
        }
    }
    #endregion
}
