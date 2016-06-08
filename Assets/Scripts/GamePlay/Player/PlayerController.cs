using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private int playerId = 0;
       
    private PlayerBlackboard blackboard = new PlayerBlackboard();

    //Life
    [Header("Health Settings")]
    public int maxLives = 3; 
    public float maxHealth = 100f;    
    public float invulnerabilityTimeAfterHit = 3f;
    public float maxAngleToShieldBlocking = 30f;
    public float damageRatioWhenBlockedWrongColor = 0.75f; 

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
    public float maxEnergy = 100f;
    public float energyIncreaseWhenBlockedCorrectColor = 10f;
    public float specialAttackNecessaryEnergy = 50f;


    [Header("Fire Settings")]   
    public float fireRate = 0.25f;
    public float speedRatioReductionWhileFiring = 0.6f;
    public Transform shotSpawn;
    public Transform muzzlePoint;
    public int selfDamageOnColorMismatch = 10;
    public float fireSuppresionTimeOnColorMismatch = 3f;

    [Header("Special Attack Settings")]
    public float specialAttackDamage = 30f;
    public float specialAttackAffectationRadius = 6f;
    public float forceMultiplierAtDistance1 = 10;
    public float forceAttenuationByDistance = 1.25f;

    [Header("Particle Systems")]
    [SerializeField]
    private ParticleSystem[] dashPSs = new ParticleSystem[4];
    [SerializeField]
    private ParticleSystem electricPS;
    [SerializeField]
    private ParticleSystem chargePS;

    //Misc
    [Header("Miscelaneous Settings")]
    public float idleRandomAnimTime = 10f;


    [SerializeField]
    private Renderer bodyRend;
    [SerializeField]
    private Renderer shieldRend;
    private CharacterController ctrl;
    private ColoredObjectsManager coloredObjMng;
    private VoxelizationClient voxelization;
   
    private PlayerBaseState currentState;

    //Properties
    public bool Active { get { return blackboard.active; } set { blackboard.active = value; } }
    public bool Alive { get { return blackboard.alive; } }
    public int Id { get { return playerId; } }
    public int Lives { get { return blackboard.currentLives; } }
    public float Health { get { return blackboard.currentHealth; } }
    public float Energy { get { return blackboard.currentEnergy; } }

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
        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameStopped);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameStopped);
        blackboard.currentColor = rsc.colorMng.CurrentColor;
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

    private void GameStopped(EventInfo eventInfo)
    {
        ChangeState(blackboard.blockedState);
    }

    private void ColorChanged(EventInfo eventInfo)
    {
        blackboard.currentColor = ((ColorEventInfo)eventInfo).newColor;
        SetMaterial();
    }

    private void SetMaterial()
    {
        Material bodyMat;
        Material shieldMat;

        switch (playerId)
        {
            case 1:
                bodyMat = coloredObjMng.GetPlayer1Material(blackboard.currentColor);
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(blackboard.currentColor);
                break;
            case 2:
                bodyMat = coloredObjMng.GetPlayer1Material(blackboard.currentColor); //TODO: Change to player2 material
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(blackboard.currentColor);
                break;
            default:
                bodyMat = coloredObjMng.GetPlayer1Material(blackboard.currentColor);
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(blackboard.currentColor);
                break;
        }
        Material[] mats = bodyRend.sharedMaterials;
        mats[1] = bodyMat;
        bodyRend.sharedMaterials = mats;

        shieldRend.sharedMaterial = shieldMat;

        blackboard.blinkController.InvalidateMaterials();
    }

    public void RechargeEnergy(float energy)
    {
        if (blackboard.currentEnergy == blackboard.player.maxEnergy) return;

        blackboard.currentEnergy += energy;
        if (blackboard.currentEnergy > blackboard.player.maxEnergy)
            blackboard.currentEnergy = blackboard.player.maxEnergy;
    }

    public void SpendEnergy(float energy)
    {
        if (blackboard.currentEnergy == 0) return;

        blackboard.currentEnergy -= energy;
        if (blackboard.currentEnergy < 0)
            blackboard.currentEnergy = 0;
    }

    public void MakeVisible()
    {
        bodyRend.enabled = true;
        shieldRend.enabled = true;
    }

    public void MakeInvisible()
    {
        bodyRend.enabled = false;
        shieldRend.enabled = false;
    }

    public void AnimationEnded()
    {
        blackboard.animationEnded = true;
    }

    public void Reset()
    {
        blackboard.animator.Rebind();
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
        if (blackboard.active)
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

        if (blackboard.updateVerticalPosition)
        {
            totalDirection.y = verticalVelocity;

            totalDirection *= Time.deltaTime;
        }

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
            //Debug.Log("Player " + playerId + " Exiting: " + currentState.GetType().Name);
            currentState.OnStateExit();
        }
        currentState = newState;
        if (currentState != null)
        {
            //Debug.Log("Player " + playerId + " Entering: " + currentState.GetType().Name);
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
        dashPSs[(int)blackboard.currentColor].Play();
    }

    public void TakeDamage(float damage, bool triggerDamageAnim = true)
    {
        PlayerBaseState newState = currentState.TakeDamage(damage, triggerDamageAnim);
        if (newState != null)
        {
            ChangeState(newState);
        }     
    }

    public void TakeDamage(float damage, ChromaColor color, bool triggerDamageAnim = true)
    {
        PlayerBaseState newState = currentState.TakeDamage(damage, color, triggerDamageAnim);
        if (newState != null)
        {
            ChangeState(newState);
        }

    }

    public void ReceiveAttack(float damage, ChromaColor color, Vector3 origin)
    {
        PlayerBaseState newState = currentState.AttackReceived(damage, color, origin);
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
        else if (other.tag == "Enemy")
        {
            blackboard.enemiesInRange.Add(other.GetComponent<EnemyBaseAIBehaviour>());
        }
        else if (other.tag == "DeathZone")
        {           
            if(rsc.debugMng.godMode)
            {
                PlayerEventInfo.eventInfo.player = blackboard.player;
                rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerEventInfo.eventInfo);
            }
            else
                TakeDamage(1000f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Border")
        {
            blackboard.isInBorder = false;
        }
        else if (other.tag == "Enemy")
        {
            blackboard.enemiesInRange.Remove(other.GetComponent<EnemyBaseAIBehaviour>());
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.tag == "Enemy")
        {
            currentState.EnemyTouched();
        }
    }

    public void EnteredUSB()
    {
        ChangeState(blackboard.invisibleState);
        electricPS.Play();
    }

    public void ExitedUSB()
    {
        electricPS.Stop();
        ChangeState(blackboard.idleState);
    }

    public void StartSpecialEnergyCharging()
    {
        chargePS.Play();
    }

    public void StopSpecialEnergyCharging()
    {
        chargePS.Stop();
    }
}
