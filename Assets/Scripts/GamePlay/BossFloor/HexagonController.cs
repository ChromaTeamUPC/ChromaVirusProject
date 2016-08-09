using UnityEngine;
using System.Collections;

public class HexagonController : MonoBehaviour 
{
    private enum Neighbour
    {
        TOP,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM,
        BOTTOM_LEFT,
        BOTTOM_RIGHT
    }

    public const float DISTANCE_BETWEEN_HEXAGONS = 7.225f;
    public static int hexagonLayer = 1 << LayerMask.NameToLayer("Hexagon");

    private HexagonController[] neighbours = new HexagonController[6];

    private HexagonBaseState currentState;

    public HexagonIdleState idleState;
    public HexagonMovingState movingState;
    public HexagonInfectedState infectedState;
    public HexagonBelowAttackWarningState belowAttackWarningState;
    public HexagonBelowAttackState belowAttackState;
    public HexagonBelowAttackAdjacentState belowAttackAdjacentState;


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
    private bool up;
    private float totalMovement;
    private float currentMovement;
    [HideInInspector]
    public bool isMoving;
    [HideInInspector]
    public int probesInRange;

    [Header("Notification Settings")]
    public float belowAttackWarnInterval = 0.2f;
    public float belowAttackBlinkInterval = 0.1f;

    [Header("Infection Settings")]
    public float infectionTimeAfterContactEnds = 2f;
    public float infectionTimeAfterAttack = 1f;
    [HideInInspector]
    public float currentInfectionDuration;
    [HideInInspector]
    public bool countingInfectionTime;

    [Header("Damage Settings")]
    public float belowAttackDamage = 10f;
    public float infectedCellDamage = 8f;

    [Header("Materials")]
    public Material planeTransparentMat;
    public Material planeInfectedMaterial;
    public Material planeBelowAttackWarningMat;

    [Header("Fx's")]
    public GameObject buffPurpleGO;
    public ParticleSystem buffPurple;
    public ParticleSystem continousPurple;

    [HideInInspector]
    public GameObject geometryOffset;
    [HideInInspector]
    public float geometryOriginalY;

    //Infection variables

    [Header("Objects")]
    public GameObject column;
    public Renderer columnRend;
    public BlinkController columnBlinkController;

    public GameObject plane;
    public Renderer planeRend;
    public BlinkController planeBlinkController;

    private bool adjacentCellsSetupTop; //true = top, bottom left, bottom right, false = bottom, top left, top right

    void Awake()
    {
        idleState = new HexagonIdleState(this);
        movingState = new HexagonMovingState(this);
        infectedState = new HexagonInfectedState(this);
        belowAttackWarningState = new HexagonBelowAttackWarningState(this);
        belowAttackState = new HexagonBelowAttackState(this);
        belowAttackAdjacentState = new HexagonBelowAttackAdjacentState(this);

        minDistanceToPlayer = minHexagonsToPlayer * DISTANCE_BETWEEN_HEXAGONS;

        geometryOffset = transform.FindDeepChild("GeometryOffset").gameObject;
        geometryOriginalY = geometryOffset.transform.position.y;
        probesInRange = 0;

        currentState = idleState;
    }

    // Use this for initialization
    void Start () 
	{
        plane.SetActive(false);
        CheckNeighbours();
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
        planeRend.sharedMaterial = mat;
        planeBlinkController.InvalidateMaterials();
        plane.SetActive(true);
    }

    public void SetColumnMaterial(Material mat)
    {
        columnRend.sharedMaterial = mat;
        columnBlinkController.InvalidateMaterials();
    }

    private void ChangeStateIfNotNull(HexagonBaseState newState)
    {
        if (newState != null)
            ChangeState(newState);
    }

    private void ChangeState(HexagonBaseState newState)
    {
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

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "HexagonProbe")
        {
            ++probesInRange;
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
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "HexagonProbe")
        {
            --probesInRange;            
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

    public void WormBelowAttackWarning()
    {
        //choose adjacent cells
        adjacentCellsSetupTop = Random.Range(0f, 1f) < 0.5f;

        //Set adjacent cells state
        //true = top, bottom left, bottom right, false = bottom, top left, top right
        if (adjacentCellsSetupTop)
        {
            SetWormBelowAttackAdjacentWarning(neighbours[(int)Neighbour.TOP]);
            SetWormBelowAttackAdjacentWarning(neighbours[(int)Neighbour.BOTTOM_LEFT]);
            SetWormBelowAttackAdjacentWarning(neighbours[(int)Neighbour.BOTTOM_RIGHT]);
        }
        else
        {
            SetWormBelowAttackAdjacentWarning(neighbours[(int)Neighbour.BOTTOM]);
            SetWormBelowAttackAdjacentWarning(neighbours[(int)Neighbour.TOP_LEFT]);
            SetWormBelowAttackAdjacentWarning(neighbours[(int)Neighbour.TOP_RIGHT]);
        }

        ChangeState(belowAttackWarningState);
    }

    private void SetWormBelowAttackAdjacentWarning(HexagonController adjacent)
    {
        if (adjacent != null)
            adjacent.WormBelowAttackAdjacentWarning();
    }

    private void WormBelowAttackAdjacentWarning()
    {
        ChangeState(belowAttackWarningState);
    }

    public void WormBelowAttackStart()
    {
        //Set adjacent cells state
        //true = top, bottom left, bottom right, false = bottom, top left, top right
        if (adjacentCellsSetupTop)
        {
            SetWormBelowAttackAdjacentStart(neighbours[(int)Neighbour.TOP]);
            SetWormBelowAttackAdjacentStart(neighbours[(int)Neighbour.BOTTOM_LEFT]);
            SetWormBelowAttackAdjacentStart(neighbours[(int)Neighbour.BOTTOM_RIGHT]);
        }
        else
        {
            SetWormBelowAttackAdjacentStart(neighbours[(int)Neighbour.BOTTOM]);
            SetWormBelowAttackAdjacentStart(neighbours[(int)Neighbour.TOP_LEFT]);
            SetWormBelowAttackAdjacentStart(neighbours[(int)Neighbour.TOP_RIGHT]);
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
        ChangeState(belowAttackAdjacentState);
    }

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
}
