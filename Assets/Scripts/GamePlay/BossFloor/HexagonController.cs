using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexagonController : MonoBehaviour 
{
    private enum Neighbour
    {
        TOP_LEFT,
        TOP,
        TOP_RIGHT,
        BOTTOM_RIGHT,
        BOTTOM,
        BOTTOM_LEFT
    }

    public const float DISTANCE_BETWEEN_HEXAGONS = 7.225f;
    public static int hexagonLayer = 0;

    private HexagonController[] neighbours = new HexagonController[6];

    private HexagonBaseState currentState;

    public HexagonStaticState staticState;
    public HexagonIdleState idleState;
    public HexagonBorderWallState borderWallState;
    public HexagonMovingState movingState;
    public HexagonEnterExitState enterExitState;
    public HexagonInfectedState infectedState;
    public HexagonWarningState warningState;
    public HexagonBelowAttackState belowAttackState;
    public HexagonBelowAttackAdjacentState belowAttackAdjacentState;
    public HexagonBelowAttackWallState belowAttackWallState;
    public HexagonAboveAttackState aboveAttackState;
    public HexagonAboveAttackAdjacentState aboveAttackAdjacentState;

    [Header("Flags")]
    public bool isStatic = false;
    public bool isWormSelectable = true;
    public bool isBorder = false;

    //Movement variables
    [Header("Movement Settings")]
    public float minHexagonsToPlayer = 4f;
    [HideInInspector]
    public float minDistanceToPlayer;
    public float movementMinWaitTime = 1f;
    public float movementMaxWaitTime = 3f;
    public float upMinMovement = 1f;
    public float upMaxMovement = 5f;
    public float downMinMovement = 1f;
    public float downMaxMovement = 2f;
    public float movementSpeed = 5f;

    [Header("Earthquake Settings")]
    public float earthquakeMinHeight = 1f;
    public float earthquakeMaxHeight = 2f;
    public float earthquakeUpSpeed = 6.5f;
    public float earthquakeDownSpeed = 4;

    [HideInInspector]
    public bool isMoving;
    private int wormProbesInRange;
    //private int enemyProbesInRange;
    private List<GameObject> enemiesInRange = new List<GameObject>();
    private int playerProbesInRange;

    public bool EnemiesInRange { get { return enemiesInRange.Count > 0; } }
    public bool AnyProbeInRange { get { return playerProbesInRange + enemiesInRange.Count + wormProbesInRange > 0; } }

    [Header("Border Settings")]
    public float borderMinHeight = 3f;
    public float borderMaxHeight = 5f;
    public float borderSpeed = 10f;

    [Header("Notification Settings")]
    public float enterExitBlinkInterval = 0.1f;
    public float warningBlinkInterval = 0.2f;
    public float belowAttackBlinkInterval = 0.1f;
    public float aboveAttackBlinkInterval = 0.1f;
    [HideInInspector]
    public bool mainAttackHexagon;

    [Header("Wall Settings")]
    public float wallDuration = 1f;
    public float wallHeight = 1f;
    public float wallSpeed = 2f;
    [HideInInspector]
    public bool shouldBeWall = false;

    [Header("Infection Settings")]
    public float infectionAnimationInterval = 0.1f;
    public bool infectionRandomAnimation = true;
    public bool infectionAnimationRotation = true;
    public float infectionTimeAfterEnterExit = 2f;
    public float infectionTimeAfterContactEnds = 2f;
    public float infectionTimeAfterAttack = 1f;    
    public Vector2 infectionForces = new Vector2(10f, 10f);
    private float auxTimer;
    private float auxHalfTimer;
    public float AuxTimer { get { return auxTimer; } }
    public float AuxHalfTimer { get { return auxHalfTimer; } }
    [HideInInspector]
    public bool isWormTouchingHexagon;

    [Header("Damage Settings")]
    public float enterExitDamage = 10f;
    public float belowAttackCentralDamage = 10f;
    public float belowAttackAdjacentDamage = 10f;
    public float aboveAttackCentralDamage = 10f;
    public float aboveAttackAdjacentDamage = 10f;
    public float infectedCellDamage = 8f;

    [Header("Materials")]
    public Material planeTransparentMat;
    public Material planeMainWarningMat;
    public Material planeSecondaryWarningMat;
    public Material[] planeInfectedMats;

    [Header("Fx's")]
    public GameObject buffPurpleGO;
    public ParticleSystem buffPurple;
    public ParticleSystem continousPurple;
    public ParticleSystem infectionPurple;

    [HideInInspector]
    public GameObject geometryOffset;
    [HideInInspector]
    public float geometryOriginalY;
    [HideInInspector]
    public GameObject spawnPoint;

    [Header("Objects")]
    public GameObject column;
    public Renderer columnRend;
    public BlinkController columnBlinkController;
    private SphereCollider sphereCollider;

    public GameObject navMeshObstacles;

    public GameObject plane;
    public Renderer planeRend;
    public BlinkController planeBlinkController;

    [HideInInspector]
    public Vector3 attackCenter;

    [HideInInspector]
    public List<AIAction> entryActions;
    [HideInInspector]
    public List<AIAction> attackActions;
    [HideInInspector]
    public List<AIAction> infectActions;

    private bool adjacentCellsSetupTop; //true = top, bottom left, bottom right, false = bottom, top left, top right

    void Awake()
    {
        if(hexagonLayer == 0)
            hexagonLayer = 1 << LayerMask.NameToLayer("Hexagon");

        staticState = new HexagonStaticState(this);
        idleState = new HexagonIdleState(this);
        borderWallState = new HexagonBorderWallState(this);
        movingState = new HexagonMovingState(this);
        enterExitState = new HexagonEnterExitState(this);
        infectedState = new HexagonInfectedState(this);
        warningState = new HexagonWarningState(this);
        belowAttackState = new HexagonBelowAttackState(this);
        belowAttackAdjacentState = new HexagonBelowAttackAdjacentState(this);
        belowAttackWallState = new HexagonBelowAttackWallState(this);
        aboveAttackState = new HexagonAboveAttackState(this);
        aboveAttackAdjacentState = new HexagonAboveAttackAdjacentState(this);

        minDistanceToPlayer = minHexagonsToPlayer * DISTANCE_BETWEEN_HEXAGONS;

        sphereCollider = GetComponent<SphereCollider>();

        geometryOffset = transform.FindDeepChild("GeometryOffset").gameObject;
        geometryOriginalY = geometryOffset.transform.position.y;
        spawnPoint = transform.FindDeepChild("SpawnPoint").gameObject;
        wormProbesInRange = 0;
        //enemyProbesInRange = 0;
        enemiesInRange.Clear();
        playerProbesInRange = 0;

        CheckNeighbours();

        if (isStatic)
        {
            currentState = staticState;
            navMeshObstacles.SetActive(true);
        }
        else
        {
            currentState = idleState;
            navMeshObstacles.SetActive(false);
        }
    }

    // Use this for initialization
    void Start () 
	{
        entryActions = rsc.enemyMng.defaultSpiderEntry;
        attackActions = rsc.enemyMng.defaultSpiderAttack;
        infectActions = rsc.enemyMng.defaultSpiderInfect;

        plane.SetActive(false);

        for (int i = 0; i < neighbours.Length; ++i)
        {
            if (neighbours[i] == null)
            {
                isBorder = true;
                break;
            }
        }

        isWormTouchingHexagon = false;

        //Not needed from now on
        if (isStatic)
            sphereCollider.enabled = false;
    }

    public void SetAuxTimer(float time)
    {
        auxTimer = time;
        auxHalfTimer = auxTimer / 2;
    }

    void FixedUpdate()
    {
        if(auxTimer > 0f && !isWormTouchingHexagon)
            auxTimer -= Time.fixedDeltaTime;

        isWormTouchingHexagon = false;

        CheckEnemiesInRange();
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (currentState != null)
        {
            HexagonBaseState newState = currentState.Update();
            if (newState != null)
            {
                ChangeState(newState);
            }
        }   
    }

    public void SetPlaneMaterial(Material mat)
    {
        StopPlaneInfectionAnimation();
        plane.SetActive(true);
        planeRend.sharedMaterial = mat;
        planeBlinkController.InvalidateMaterials();
    }

    public void StartPlaneInfectionAnimation()
    {
        StopPlaneInfectionAnimation();
        infectionPurple.Play();
        plane.SetActive(true);
        StartCoroutine(AnimateInfection());
    }

    public void StopPlaneInfectionAnimation()
    {
        StopAllCoroutines();
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = Vector3.one;
        plane.SetActive(false);
        infectionPurple.Stop();
    }

    private IEnumerator AnimateInfection()
    {
        int frameNumber = planeInfectedMats.Length;

        while (true)
        {
            if (!infectionRandomAnimation)
                frameNumber = (frameNumber + 1) % planeInfectedMats.Length;
            else
                frameNumber = Random.Range(0, planeInfectedMats.Length);

            planeRend.sharedMaterial = planeInfectedMats[frameNumber];

            if(infectionAnimationRotation)
            {
                int rotateIndex = Random.Range(1, 6);
                int rotationAngle = rotateIndex * 60;

                plane.transform.Rotate(0, rotationAngle, 0);
            }

            yield return new WaitForSeconds(infectionAnimationInterval);
        }
    }

    public bool CanExitWorm()
    {
        return !isStatic && isWormSelectable && !isBorder;
    }

    private void ChangeStateIfNotNull(HexagonBaseState newState)
    {
        if (newState != null)
            ChangeState(newState);
    }

    private void ChangeState(HexagonBaseState newState)
    {
        if (isStatic) return;

        if (currentState != null)
        {
            //Debug.Log("Hexagon Exiting: " + currentState.GetType().Name);
            currentState.OnStateExit();
        }
        currentState = newState;
        if (currentState != null)
        {
            //Debug.Log("Hexagon Entering: " + currentState.GetType().Name);
            currentState.OnStateEnter();
        }
    }

    private void CheckEnemiesInRange()
    {
        for(int i = enemiesInRange.Count-1; i >= 0; --i)
        {
            if (!enemiesInRange[i].activeInHierarchy)
                enemiesInRange.RemoveAt(i);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WormHexagonProbe")
        {
            ++wormProbesInRange;
            ChangeStateIfNotNull(currentState.ProbeTouched());           
        }
        else if (other.tag == "EnemyHexagonProbe")
        {
            //++enemyProbesInRange;
            enemiesInRange.Add(other.gameObject);
            ChangeStateIfNotNull(currentState.ProbeTouched());
        }
        else if (other.tag == "PlayerHexagonProbe")
        {
            ++playerProbesInRange;
            ChangeStateIfNotNull(currentState.ProbeTouched());
        }
        else if (other.tag == "WormHead")
        {
            ChangeStateIfNotNull(currentState.WormHeadEntered());
        }
        else if (other.tag == "WormTail")
        {
            ChangeStateIfNotNull(currentState.WormTailEntered());
        }
        else if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            ChangeStateIfNotNull(currentState.PlayerEntered(player));
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            ChangeStateIfNotNull(currentState.PlayerStay(player));
        }
        else if (other.tag == "EnemyHexagonBodyProbe")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();
            ChangeStateIfNotNull(currentState.EnemyStay(enemy));
        }
        else if (other.tag == "WormHead" || other.tag == "WormBody" || other.tag == "WormTail")
        {
            isWormTouchingHexagon = true;
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "WormHexagonProbe")
        {
            --wormProbesInRange;            
        }
        else if (other.tag == "EnemyHexagonProbe")
        {
            //--enemyProbesInRange;
            enemiesInRange.Remove(other.gameObject);
        }
        else if (other.tag == "PlayerHexagonProbe")
        {
            --playerProbesInRange;
        }
        else if (other.tag == "WormHead")
        {
            ChangeStateIfNotNull(currentState.WormHeadExited());
        }
        else if (other.tag == "WormTail")
        {
            ChangeStateIfNotNull(currentState.WormTailExited());
        }
        else if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            ChangeStateIfNotNull(currentState.PlayerExited(player));
        }
    }

    public void WormEnterExit()
    {
        if (isStatic) return;

        ChangeState(enterExitState);
    }


    #region Below Attack
    public void WormBelowAttackWarning(int adjacentCells)
    {
        if (isStatic) return;

        //choose adjacent cells
        int cellNum;
        switch (adjacentCells)
        {
            case 1:
                cellNum = Random.Range(0, 6);
                SetWormAttackAdjacentWarning(neighbours[cellNum]);
                break;

            case 2:
                cellNum = Random.Range(0, 3);
                SetWormAttackAdjacentWarning(neighbours[cellNum]);
                cellNum += 3;
                SetWormAttackAdjacentWarning(neighbours[cellNum]);
                break;

            case 3:
                cellNum = Random.Range(0, 2);
                SetWormAttackAdjacentWarning(neighbours[cellNum]);
                cellNum += 2;
                SetWormAttackAdjacentWarning(neighbours[cellNum]);
                cellNum += 2;
                SetWormAttackAdjacentWarning(neighbours[cellNum]);
                break;

            case 4:
                cellNum = Random.Range(0, 3);
                for(int i = 0; i < neighbours.Length; ++i)
                {
                    if(i != cellNum && i != cellNum + 3)
                        SetWormAttackAdjacentWarning(neighbours[i]);
                }
                break;

            case 5:
                cellNum = Random.Range(0, 6);
                for (int i = 0; i < neighbours.Length; ++i)
                {
                    if (i != cellNum)
                        SetWormAttackAdjacentWarning(neighbours[i]);
                }
                break;

            default:
                break;
        }

        RaiseWallRing();

        mainAttackHexagon = true;
        ChangeState(warningState);
    }

    private void SetWormAttackAdjacentWarning(HexagonController adjacent)
    {
        if (adjacent != null)
            adjacent.WormAttackAdjacentWarning();
    }

    private void WormAttackAdjacentWarning()
    {
        if (isBorder) return;
        if (isStatic) return;

        mainAttackHexagon = false;
        ChangeState(warningState);
    }

    public void WormBelowAttackStart()
    {
        if (isStatic) return;

        //Set adjacent cells state
        for (int i = 0; i < neighbours.Length; ++i)
        {
            SetWormBelowAttackAdjacentStart(neighbours[i]);
        }

        ChangeState(belowAttackState);
    }

    private void SetWormBelowAttackAdjacentStart(HexagonController adjacent)
    {
        if (adjacent != null)
            adjacent.WormBelowAttackAdjacentStart();
    }

    private void WormBelowAttackAdjacentStart()
    {
        if (isBorder) return;
        if (isStatic) return;

        if (currentState == warningState)
            ChangeState(belowAttackAdjacentState);
    }

    private void RaiseWallRing()
    {
        for (int i = 0; i < neighbours.Length; ++i)
            SetWormBelowAttackRaiseWall(neighbours[i], (Neighbour)((i + neighbours.Length / 2) % neighbours.Length));
    }

    private void SetWormBelowAttackRaiseWall(HexagonController adjacent, Neighbour origin)
    {
        if (adjacent != null)
            adjacent.WormBelowAttackRaiseWall(origin);
    }

    private void WormBelowAttackRaiseWall(Neighbour origin)
    {
        int oposite1 = ((int)origin + (neighbours.Length / 2)) % neighbours.Length;
        int oposite2 = ((int)origin + (neighbours.Length / 2) + 1) % neighbours.Length;

        if (neighbours[oposite1] != null)
            neighbours[oposite1].RaiseItself();

        if (neighbours[oposite2] != null)
            neighbours[oposite2].RaiseItself();
    }

    private void RaiseItself()
    {
        if (isBorder) return;
        if (isStatic) return;

        if (!EnemiesInRange)
        {
            shouldBeWall = true;
            ChangeState(belowAttackWallState);
        }
    }

    public void LowerWallRing()
    {
        for (int i = 0; i < neighbours.Length; ++i)
            SetWormBelowAttackLowerWall(neighbours[i], (Neighbour)((i + neighbours.Length / 2) % neighbours.Length));
    }

    private void SetWormBelowAttackLowerWall(HexagonController adjacent, Neighbour origin)
    {
        if (adjacent != null)
            adjacent.WormBelowAttackLowerWall(origin);
    }

    private void WormBelowAttackLowerWall(Neighbour origin)
    {
        int oposite1 = ((int)origin + (neighbours.Length / 2)) % neighbours.Length;
        int oposite2 = ((int)origin + (neighbours.Length / 2) + 1) % neighbours.Length;

        if (neighbours[oposite1] != null)
            neighbours[oposite1].LowerItself();

        if (neighbours[oposite2] != null)
            neighbours[oposite2].LowerItself();
    }

    private void LowerItself()
    {
        shouldBeWall = false;
    }
    #endregion

    #region Above Attack
    public void WormAboveAttackWarning()
    {
        if (isStatic) return;

        for (int i = 0; i < neighbours.Length; ++i)
            SetWormAttackAdjacentWarning(neighbours[i]);

        mainAttackHexagon = true;
        ChangeState(warningState);
    }

    public void WormAboveAttackStart()
    {
        if (isStatic) return;

        //Set adjacent cells state
        for (int i = 0; i < neighbours.Length; ++i)
        {
            SetWormAboveAttackAdjacentStart(neighbours[i]);
        }

        ChangeState(aboveAttackState);
    }

    private void SetWormAboveAttackAdjacentStart(HexagonController adjacent)
    {
        if (adjacent != null)
            adjacent.WormAboveAttackAdjacentStart(transform.position);
    }

    private void WormAboveAttackAdjacentStart(Vector3 attackCenter)
    {
        if (isStatic) return;

        this.attackCenter = attackCenter;
        ChangeState(aboveAttackAdjacentState);
    }
    #endregion

    #region Neighbour Control
    //Neighbour control
    private void CheckNeighbours()
    {
        Vector3 direction = Vector3.forward * DISTANCE_BETWEEN_HEXAGONS;
        Collider[] colliders;

        if (neighbours[(int)Neighbour.TOP] == null)
        {
            colliders = Physics.OverlapSphere(transform.position + direction, 1f, hexagonLayer);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].tag == "Hexagon")
                {
                    HexagonController neighbour = colliders[i].GetComponent<HexagonController>();
                    neighbours[(int)Neighbour.TOP] = neighbour;

                    neighbour.SetNeighbour(this, Neighbour.BOTTOM);

                    break;
                }
            }
        }

        direction = Quaternion.Euler(0, 60, 0) * direction;

        if (neighbours[(int)Neighbour.TOP_RIGHT] == null)
        {
            colliders = Physics.OverlapSphere(transform.position + direction, 1f, hexagonLayer);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].tag == "Hexagon")
                {
                    HexagonController neighbour = colliders[i].GetComponent<HexagonController>();
                    neighbours[(int)Neighbour.TOP_RIGHT] = neighbour;

                    neighbour.SetNeighbour(this, Neighbour.BOTTOM_LEFT);

                    break;
                }
            }
        }

        direction = Quaternion.Euler(0, 60, 0) * direction;

        if (neighbours[(int)Neighbour.BOTTOM_RIGHT] == null)
        {
            colliders = Physics.OverlapSphere(transform.position + direction, 1f, hexagonLayer);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].tag == "Hexagon")
                {
                    HexagonController neighbour = colliders[i].GetComponent<HexagonController>();
                    neighbours[(int)Neighbour.BOTTOM_RIGHT] = neighbour;

                    neighbour.SetNeighbour(this, Neighbour.TOP_LEFT);

                    break;
                }
            }
        }
    }

    private void SetNeighbour(HexagonController neighbour, Neighbour position)
    {
        neighbours[(int)position] = neighbour;
    }
    #endregion
}
