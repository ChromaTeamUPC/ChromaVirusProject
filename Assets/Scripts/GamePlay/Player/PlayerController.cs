using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private int playerId = 0;
        
    private bool active = false; //Player is participating in current game (not necesarily alive)
    private bool alive = false; //Player is alive at the moment

    private PlayerBlackboard blackboard = new PlayerBlackboard();

    //Life
    [Header("Health Settings")]
    public int maxLives = 3; 
    public int maxHealth = 100;    
    public float invulnerabilityTimeAfterHit = 3f;

    //Movement
    [Header("Movement Settings")]
    public float walkSpeed = 10;
    public float moveThreshold = 0.2f;
    public float angularSpeed = 1080;
    public float aimThreshold = 0.2f;
    public float maxDashTime = 1.0f;
    public float initialDashSpeed = 20.0f;
    public float dashDeceleration = 5f;
    public bool isDecelerationRatio = false;
    public float minDashSpeed = 1;
    //public float dashStoppingSpeed = 1.0f;

    public float speedRatioReductionOnContact = 0.1f;
    public int damageOnContact = 5;
    public float speedReductionTimeOnContact = 0.3f;
    public float cooldownTime = 1f;

    private float verticalVelocity = 0f; 

    //Attack
    [Header("Energy Settings")]
    public float maxEnergy = 100;


    [Header("Fire Settings")]   
    public float fireRate = 0.25f;
    public float speedRatioReductionWhileFiring = 0.6f;
    public Transform shotSpawn;
    public Transform muzzlePoint;
    public int selfDamageOnColorMismatch = 10;
    public float fireSuppresionTimeOnColorMismatch = 3f;

    //Misc
    [Header("Miscelaneous Settings")]
    public float idleRandomAnimTime = 10f;

    [SerializeField]
    private ParticleSystem[] dashPSs = new ParticleSystem[4];

    private ChromaColor currentColor;

    [SerializeField]
    private Renderer bodyRend;
    [SerializeField]
    private Renderer shieldRend;
    private CharacterController ctrl;
    private ColoredObjectsManager coloredObjMng;
    private VoxelizationClient voxelization;
   
    private PlayerBaseState currentState;

    //Properties
    public bool Active { get { return active; } set { active = value; } }
    public bool Alive { get { return alive; } }
    public int Id { get { return playerId; } }
    public int Lives { get { return blackboard.currentLives; } }
    public int Health { get { return blackboard.currentHealth; } }
    public int Energy { get { return (int)blackboard.currentEnergy; } }

    void Awake()
    {
        blackboard.Init(this);
        
        voxelization = GetComponentInChildren<VoxelizationClient>();
        ctrl = GetComponent<CharacterController>();   

        Debug.Log("Player " + playerId + " created.");
    }

    void Start()
    {
        coloredObjMng = rsc.coloredObjectsMng;
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        currentColor = rsc.colorMng.CurrentColor;
        SetMaterial();
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        }
        Debug.Log("Player " + playerId + " destroyed.");
    }

    void ColorChanged(EventInfo eventInfo)
    {
        currentColor = ((ColorEventInfo)eventInfo).newColor;
        SetMaterial();
    }

    private void SetMaterial()
    {
        Material bodyMat;
        Material shieldMat;

        switch (playerId)
        {
            case 1:
                bodyMat = coloredObjMng.GetPlayer1Material(currentColor);
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(currentColor);
                break;
            case 2:
                bodyMat = coloredObjMng.GetPlayer1Material(currentColor); //TODO: Change to player2 material
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(currentColor);
                break;
            default:
                bodyMat = coloredObjMng.GetPlayer1Material(currentColor);
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(currentColor);
                break;
        }
        Material[] mats = bodyRend.sharedMaterials;
        mats[1] = bodyMat;
        bodyRend.sharedMaterials = mats;

        shieldRend.sharedMaterial = shieldMat;

        blackboard.blinkController.InvalidateMaterials();
    }

    public void AnimationEnded()
    {
        blackboard.animationEnded = true;
    }

    public void Reset()
    {
        blackboard.ResetGameVariables();
    }

    public void Spawn()
    {
        blackboard.ResetLifeVariables();

        verticalVelocity = Physics.gravity.y;

        ChangeState(blackboard.spawningState);       
    }

    void Update()
    {
        if (active)
        {
            //Reset flags
            blackboard.ResetFlagVariables();

            if (currentState != null)
            {
                PlayerBaseState newState = currentState.Update();
                if (newState != null)
                {
                    ChangeState(newState);
                }
            }

            UpdatePosition();

            if (blackboard.currentShootingStatus != blackboard.newShootingStatus)
            {
                blackboard.currentShootingStatus = blackboard.newShootingStatus;
                blackboard.animator.SetBool("Shooting", blackboard.currentShootingStatus);
                if (!blackboard.currentShootingStatus)
                    blackboard.doubleShooting = false;
            }
        }
    }

    public void UpdatePosition()
    {
        Vector3 totalDirection = blackboard.horizontalDirection * blackboard.currentSpeed;

        float otherModifier = 0f;
        if (blackboard.doubleShooting)
            otherModifier = Mathf.Max(otherModifier, speedRatioReductionWhileFiring);

        if (blackboard.isAffectedByContact)
            otherModifier = Mathf.Max(otherModifier, speedRatioReductionOnContact);

        if(otherModifier > 0)
            totalDirection *= otherModifier;

        totalDirection.y = verticalVelocity;

        totalDirection *= Time.deltaTime;

        ctrl.Move(totalDirection);

        blackboard.isGrounded = ctrl.isGrounded;

        UpdateVerticalVelocity();
    }

    private void UpdateVerticalVelocity()
    {
        if (blackboard.isGrounded)
        {
            verticalVelocity = Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void ChangeState(PlayerBaseState newState)
    {
        if (currentState != null)
        {
            Debug.Log("Player " + playerId + " Exiting: " + currentState.GetType().Name);
            currentState.OnStateExit();
        }
        currentState = newState;
        if (currentState != null)
        {
            Debug.Log("Player " + playerId + " Entering: " + currentState.GetType().Name);
            currentState.OnStateEnter();
        }
    }

    public void SpawnVoxels()
    {
        voxelization.CalculateVoxelsGrid();
        voxelization.SpawnVoxels();
    }

    public void SpawnDashParticles()
    {
        dashPSs[(int)currentColor].Play();
    }

    public void TakeDamage(int damage, bool triggerDamageAnim = true)
    {
        PlayerBaseState newState = currentState.TakeDamage(damage, triggerDamageAnim);
        if (newState != null)
        {
            ChangeState(newState);
        }     
    }

    public void TakeDamage(int damage, ChromaColor color, bool triggerDamageAnim = true)
    {
        PlayerBaseState newState = currentState.TakeDamage(damage, color, triggerDamageAnim);
        if (newState != null)
        {
            ChangeState(newState);
        }

    }

    public void ReceiveAttack(int damage, ChromaColor color)
    {
        PlayerBaseState newState = currentState.AttackReceived(damage, color);
        if (newState != null)
        {
            ChangeState(newState);
        }
    }

    public void ColorMismatch()
    {
        currentState.ColorMismatch();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Border")
        {
            blackboard.isInBorder = true;
        }
        else if (other.tag == "DeathZone")
        {           
            if(rsc.debugMng.godMode)
            {
                PlayerEventInfo.eventInfo.player = blackboard.player;
                rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerEventInfo.eventInfo);
            }
            else
                TakeDamage(1000);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Border")
        {
            blackboard.isInBorder = false;
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.tag == "Enemy")
        {
            currentState.EnemyTouched();
        }
    }
}
