using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private bool init = false;

    [SerializeField]
    private int playerId = 0;

    private PlayerBlackboard blackboard;// = new PlayerBlackboard();

    [Range(0f, 1f)]
    public float weak = 0f;
    [Range(0f, 1f)]
    public float strong = 0f;

    //Life
    [Header("Health Settings")]
    public int maxLives = 3; 
    public float maxHealth = 100f;    
    public float invulnerabilityTimeAfterHit = 3f;
    public float maxAngleToShieldBlocking = 30f;
    public float damageRatioWhenBlockedWrongColor = 0.75f;
    public float damageAfterInvulnerability = 10f; 

    //Movement
    [Header("Movement Settings")]
    public float walkSpeed = 10;
    public float moveThreshold = 0.2f;
    public float angularSpeed = 1080;
    public float aimThreshold = 0.2f;

    public float speedRatioReductionOnContact = 0.1f;
    public int damageOnContact = 5;
    public float speedReductionTimeOnContact = 0.3f;
    public float cooldownTime = 1f;

    [Header("Fast Moving & Dash Settings")]
    public float movingRechargeXSecond = 33.3f;
    public float fastMovingCostXSecond = 100f;
    public float fastMovingSpeed = 20f;
    public float dashCost = 25f;
    public float dashDetectionThreshold = 0.2f;
    public float maxDashTime = 1.0f;
    public float initialDashSpeed = 20.0f;
    public float dashDeceleration = 5f;
    public bool isDecelerationRatio = false;
    public float minDashSpeed = 1;
    //public float dashStoppingSpeed = 1.0f;

    private float verticalVelocity = 0f; 

    //Attack
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float energyIncreaseWhenBlockedCorrectColor = 1f;
    public float specialAttackNecessaryEnergy = 50f;


    [Header("Fire Settings")]   
    public float fireRate = 0.25f;
    public float speedRatioReductionWhileFiring = 0.6f;
    public Transform shotSpawn;
    public Transform muzzlePoint;
    public int selfDamageOnColorMismatch = 10;
    public float fireSuppresionTimeOnColorMismatch = 3f;
    public float effectDurationOnColorMismatch = 0.25f;

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
    [SerializeField]
    private ParticleSystem specialPS;
    private AudioSource specialSoundFX;
    [SerializeField]
    private ParticleSystem noShootPS;
    private GameObject noShootPSGO;
    [SerializeField]
    private ParticleSystem attackBlockedPS;


    [SerializeField]
    private LineRenderer mainBeam;
    [SerializeField]
    private LineRenderer fadeBeam;

    //Misc
    [Header("Miscelaneous Settings")]
    public float idleRandomAnimTime = 10f;
    public Transform hintPoint;
    public PlayerEnemyIntersectionDetector enemyDetector;
    public GameObject specialDetector;


    [SerializeField]
    private Renderer bodyRend;
    [SerializeField]
    private GameObject shield;
    [SerializeField]
    private GameObject laserAim;

    private Renderer shieldRend;
    private CharacterController ctrl;
    private ColoredObjectsManager coloredObjMng;
    private VoxelizationClient voxelization;
    private TrailRenderer trail;
   
    private PlayerBaseState currentState;

    private Vector3 totalHorizontalDirection;

    //Properties
    public bool Initialized { get { return init; } }
    public bool Active { get { return blackboard.active; } set { blackboard.active = value; } }
    public bool Alive { get { return blackboard.alive; } }
    public int Id { get { return playerId; } }
    public int Lives { get { return blackboard.currentLives; } }
    public float Health { get { return blackboard.currentHealth; } }
    public float Energy { get { return blackboard.currentEnergy; } }

    void Awake()
    {
        blackboard = new PlayerBlackboard();
        blackboard.Init(this);
        blackboard.shield = shield;
        blackboard.laserAim = laserAim;
        shieldRend = shield.GetComponent<Renderer>();
        voxelization = GetComponentInChildren<VoxelizationClient>();
        ctrl = GetComponent<CharacterController>();

        trail = GetComponentInChildren<TrailRenderer>();

        specialSoundFX = specialPS.GetComponent<AudioSource>();
        noShootPSGO = noShootPS.gameObject;

        init = true;
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

    void OnDisable()
    {
        currentState = null;
        //blackboard.animator.Rebind();
    }

    private void GameStopped(EventInfo eventInfo)
    {
        ChangeState(blackboard.blockedState);
        //blackboard.horizontalDirection = Vector3.zero;
        //currentState = null;
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

        trail.sharedMaterial = coloredObjMng.GetPlayer1TrailMaterial(blackboard.currentColor);

        Color color = rsc.coloredObjectsMng.GetColor(blackboard.currentColor);
        Color alpha = color;
        color.a = 0.5f;
        alpha.a = 0f;
        mainBeam.SetColors(color, color);
        fadeBeam.SetColors(color, alpha);
    }

    public Vector3 PositionPrediction(float secondsPrediction = 1f)
    {
        //This case will be true only when dashing or speedwalking
        if(blackboard.currentSpeed > walkSpeed)
        {
            Vector3 calculatedDirection = totalHorizontalDirection / blackboard.currentSpeed * walkSpeed;
            return transform.position + (calculatedDirection * secondsPrediction);
        }
        else
            return transform.position + (totalHorizontalDirection * secondsPrediction);
    }

    public void GoToIdle()
    {
        ChangeState(blackboard.idleState);
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

    public void AnimationTrigger()
    {
        blackboard.animationTrigger = true;
    }

    public void Reset()
    {
        //blackboard.animator.Rebind();
        blackboard.ResetGameVariables();
        Debug.Log("Player Reset");
    }

    public void Spawn(bool invulnerabilityTime = false)
    {
        blackboard.animator.Rebind();
        blackboard.ResetLifeVariables();

        verticalVelocity = Physics.gravity.y;

        currentState = null;
        ChangeState(blackboard.spawningState);

        if (invulnerabilityTime)
            currentState.StartInvulnerabilityTime();

        Debug.Log("Player Spawn");
    }

    void Update()
    {
        if (blackboard.active && blackboard.alive)
        {
            //Reset flags
            if(!blackboard.shootPressed)
                rsc.rumbleMng.RemoveContinousRumble(2);
            blackboard.ResetFlagVariables();

            if(blackboard.contactFlag && currentState != null)
            {
                PlayerBaseState newState = currentState.EnemyContactOnInvulnerabilityEnd();
                if (newState != null)
                {
                    ChangeState(newState);
                }
            }

            currentState.RetrieveInput();
            RechargeMovingCharge();

            if (currentState != null)
            {
                PlayerBaseState newState = currentState.Update();
                if (newState != null)
                {
                    ChangeState(newState);
                }
            }

            if(gameObject.activeSelf) //Check to avoid a warning trying to update CharacterController if we just died and deactivated gameObject
                UpdatePosition();
        }
    }  

    private void RechargeMovingCharge()
    {
        if (blackboard.fastMovementCharge < 100f)
        {
            blackboard.fastMovementCharge += Time.deltaTime * movingRechargeXSecond;
        }
    }

    public void UpdatePosition()
    {
        totalHorizontalDirection = blackboard.horizontalDirection * blackboard.currentSpeed;

        float otherModifier = 0f;
        if (!blackboard.firstShot)
            otherModifier = Mathf.Max(otherModifier, speedRatioReductionWhileFiring);

        if (blackboard.isAffectedByContact)
            otherModifier = Mathf.Max(otherModifier, speedRatioReductionOnContact);

        if(otherModifier > 0)
            totalHorizontalDirection *= otherModifier;

        Vector3 currentFrameDirection = totalHorizontalDirection;

        if (blackboard.updateVerticalPosition)
        {
            currentFrameDirection.y = verticalVelocity;
        }

        currentFrameDirection *= Time.deltaTime;

        ctrl.Move(currentFrameDirection);

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
        PlayerBaseState newState = currentState.ColorMismatch();
        if (newState != null)
        {
            ChangeState(newState);
        }       
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Border")
        {
            blackboard.isInBorder = true;
        }
        /*else if (other.tag == "Enemy")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponent<EnemyBaseAIBehaviour>();

            //Mosquito has the collider in a children object so we need to search for script in parent
            if (enemy == null)
                enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();

            if(enemy != null)
                blackboard.enemiesInRange.Add(enemy);
        }*/
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
        /*else if (other.tag == "Enemy")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponent<EnemyBaseAIBehaviour>();

            //Mosquito has the collider in a children object so we need to search for script in parent
            if (enemy == null)
                enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();

            if (enemy != null)
                blackboard.enemiesInRange.Remove(enemy);
        }*/
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.tag == "Enemy")
        {
            Debug.Log("Enemy touched!");
            PlayerBaseState newState = currentState.EnemyTouched();
            if (newState != null)
            {
                ChangeState(newState);
            }        
        }
    }

    public void ActivateShield()
    {
        blackboard.shield.SetActive(true);
        blackboard.laserAim.SetActive(true);
    }

    public void DeactivateShield()
    {
        blackboard.shield.SetActive(false);
        blackboard.laserAim.SetActive(false);
    }

    public void SetCapacitor(CapacitorController capacitor)
    {
        ButtonHintEventInfo.eventInfo.playerId = playerId;
        ButtonHintEventInfo.eventInfo.buttonType = ButtonHintEventInfo.ButtonType.COLOR_BUTTONS;
        ButtonHintEventInfo.eventInfo.show = (capacitor != null);

        rsc.eventMng.TriggerEvent(EventManager.EventType.BUTTON_HINT, ButtonHintEventInfo.eventInfo);

        blackboard.capacitor = capacitor;
    }

    public void SetDevice(DeviceController device)
    {
        ButtonHintEventInfo.eventInfo.playerId = playerId;
        ButtonHintEventInfo.eventInfo.buttonType = ButtonHintEventInfo.ButtonType.A;
        ButtonHintEventInfo.eventInfo.show = (device != null);

        rsc.eventMng.TriggerEvent(EventManager.EventType.BUTTON_HINT, ButtonHintEventInfo.eventInfo);

        blackboard.device = device;
    }

    //Particle Systems methods
    public void SpawnDashParticles()
    {
        dashPSs[(int)blackboard.currentColor].Play();
    }

    public void EnteredUSB()
    {
        ChangeState(blackboard.invisibleState);
        trail.enabled = false;
        electricPS.Play();
    }

    public void ExitedUSB()
    {
        electricPS.Stop();
        trail.enabled = true;
        ChangeState(blackboard.idleState);
    }

    public void StartTrail()
    {
        trail.enabled = true;
    }

    public void StopTrail()
    {
        trail.enabled = false;
    }

    public void StartSpecialEnergyCharging()
    {
        chargePS.Play();
    }

    public void StopSpecialEnergyCharging()
    {
        chargePS.Stop();
    }

    public void StartSpecial()
    {
        specialPS.Play();
        specialSoundFX.Play();
    }

    public void StartNoShoot()
    {
        if(!noShootPSGO.activeSelf)
            noShootPSGO.SetActive(true);
    }

    public void StopNoShoot()
    {
        if (noShootPSGO.activeSelf)
            noShootPSGO.SetActive(false);
    }

    public void PlayAttackBlocked()
    {
        attackBlockedPS.Play();
    }
}
