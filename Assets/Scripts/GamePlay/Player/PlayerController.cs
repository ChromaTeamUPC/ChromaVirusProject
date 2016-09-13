using UnityEngine;
using System.Collections;
using InControl;

public class PlayerController : MonoBehaviour
{
    private bool init = false;

    [SerializeField]
    private int playerId = 0;

    public PlayerBlackboard bb;

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
    public float gravityRatioWhenPushed = 2f;

    [HideInInspector]
    public bool forceMovementEnabled = false;
    private float forceMovementForward = 1f;
    private float forceMovementRatio = 0.0005f;

    [Header("Fast Moving & Dash Settings")]
    public float fastMovingSpeed = 20f;
    public float fastMovingMaxSeconds = 0.75f;
    [Range(0f,1f)]
    public float fastMovingSpeedReductionOn = 0.75f;
    //public float fastMovingCostXSecond = 100f;
    //public float movingRechargeXSecond = 33.3f;
    //public float dashCost = 25f;
    //public float dashDetectionThreshold = 0.2f;
    public float maxDashSeconds = 0.1f;
    public float initialDashSpeed = 20.0f;
    public float dashDeceleration = 5f;
    public bool isDecelerationRatio = false;
    public float minDashSpeed = 1;
    //public float dashStoppingSpeed = 1.0f;

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
    private ParticleSystem lifeChargehargePS;
    [SerializeField]
    private ParticleSystem energyChargehargePS;
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
    private Material bodyBaseMaterial;
    [SerializeField]
    private float standardBodyBrightness = 0.4f;
    [SerializeField]
    private GameObject shield;
    [SerializeField]
    private GameObject laserAim;
    [SerializeField]
    private GameObject ui;
    [SerializeField]
    private GameObject bossIndicator;

    private Renderer shieldRend;
    private CharacterController ctrl;
    private ColoredObjectsManager coloredObjMng;
    private VoxelizationClient voxelization;
    private TrailRenderer trail;
   
    private PlayerBaseState currentState;

    private Vector3 totalHorizontalDirection;

    //Properties
    public bool Initialized { get { return init; } }
    public bool Active { get { return bb.active; } set { bb.active = value; } }
    public bool Alive { get { return bb.alive; } }
    public int Id { get { return playerId; } }
    public int Lives { get { return bb.currentLives; } }
    public float Health { get { return bb.currentHealth; } }
    public float Energy { get { return bb.currentEnergy; } }

    void Awake()
    {
        bb = new PlayerBlackboard();
        bb.Init(this);
        bb.shield = shield;
        bb.laserAim = laserAim;
        shieldRend = shield.GetComponent<Renderer>();
        voxelization = GetComponentInChildren<VoxelizationClient>();
        ctrl = GetComponent<CharacterController>();

        trail = GetComponentInChildren<TrailRenderer>();

        noShootPSGO = noShootPS.gameObject;   

        init = true;
        //Debug.Log("Player " + playerId + " created.");
    }

    void Start()
    {
        coloredObjMng = rsc.coloredObjectsMng;
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_OVER, GameStopped);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_FINISHED, GameStopped);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, GameReset);
        bb.currentColor = rsc.colorMng.CurrentColor;
        SetMaterial();
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_OVER, GameStopped);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_FINISHED, GameStopped);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, GameReset);
        }
        //Debug.Log("Player " + playerId + " destroyed.");
    }

    void OnDisable()
    {
        currentState = null;
        //blackboard.animator.Rebind();
    }

    private void GameStopped(EventInfo eventInfo)
    {
        ChangeState(bb.blockedState);
        //blackboard.horizontalDirection = Vector3.zero;
        //currentState = null;
    }

    private void GameReset(EventInfo eventInfo)
    {
        transform.SetParent(null); //To avoid destruction if parented with some scene game object
        gameObject.SetActive(false);
    }

    private void ColorChanged(EventInfo eventInfo)
    {
        bb.currentColor = ((ColorEventInfo)eventInfo).newColor;
        SetMaterial();
    }

    private void SetMaterial()
    {
        Material bodyMat;
        Material shieldMat;

        switch (playerId)
        {
            case 1:
                bodyMat = coloredObjMng.GetPlayer1Material(bb.currentColor);
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(bb.currentColor);
                break;
            case 2:
                bodyMat = coloredObjMng.GetPlayer1Material(bb.currentColor); //TODO: Change to player2 material
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(bb.currentColor);
                break;
            default:
                bodyMat = coloredObjMng.GetPlayer1Material(bb.currentColor);
                shieldMat = coloredObjMng.GetPlayer1ShieldMaterial(bb.currentColor);
                break;
        }
        Material[] mats = bodyRend.sharedMaterials;
        mats[1] = bodyMat;
        bodyRend.sharedMaterials = mats;

        shieldRend.sharedMaterial = shieldMat;

        bb.blinkController.InvalidateMaterials();

        trail.sharedMaterial = coloredObjMng.GetPlayer1TrailMaterial(bb.currentColor);

        Color color = rsc.coloredObjectsMng.GetColor(bb.currentColor);
        Color alpha = color;
        color.a = 0.5f;
        alpha.a = 0f;
        mainBeam.SetColors(color, color);
        fadeBeam.SetColors(color, alpha);
    }

    public void ForcePositionUpdate()
    {
        if (forceMovementEnabled && !bb.movePressed)
        {
            ctrl.Move(Vector3.forward * forceMovementForward * forceMovementRatio);
            forceMovementForward *= -1;
        }
        //ctrl.attachedRigidbody.WakeUp();
    }

    public Vector3 PositionPrediction(float secondsPrediction = 1f)
    {
        //This case will be true only when dashing or speedwalking
        if(bb.currentSpeed > walkSpeed)
        {
            Vector3 calculatedDirection = totalHorizontalDirection / bb.currentSpeed * walkSpeed;
            return transform.position + (calculatedDirection * secondsPrediction);
        }
        else
            return transform.position + (totalHorizontalDirection * secondsPrediction);
    }

    public void GoToIdle()
    {
        ChangeState(bb.idleState);
    }

    public void RechargeLife(bool extraLife, bool heal)
    {
        lifeChargehargePS.Play();

        if (extraLife && bb.currentLives < maxLives)
            bb.currentLives++;

        if (heal)
            bb.currentHealth = maxHealth;
    }

    public void RechargeEnergy(float energy)
    {
        if (bb.currentEnergy == bb.player.maxEnergy) return;

        if(!bb.specialAttackTutorialTriggered)
        {
            bb.specialAttackTutorialTriggered = true;
            TutorialEventInfo.eventInfo.type = TutorialManager.Type.SPECIAL_ATTACK;
            rsc.eventMng.TriggerEvent(EventManager.EventType.SHOW_TUTORIAL, TutorialEventInfo.eventInfo);
        }

        bb.currentEnergy += energy;
        if (bb.currentEnergy > bb.player.maxEnergy)
            bb.currentEnergy = bb.player.maxEnergy;
    }

    public void SpendEnergy(float energy)
    {
        if (bb.currentEnergy == 0) return;

        bb.currentEnergy -= energy;
        if (bb.currentEnergy < 0)
            bb.currentEnergy = 0;
    }

    public void MakeVisible()
    {
        bodyRend.enabled = true;
        shieldRend.enabled = true;
        ui.SetActive(true);
    }

    public void MakeInvisible()
    {
        bodyRend.enabled = false;
        shieldRend.enabled = false;
        ui.SetActive(false);
    }

    public void AnimationEnded()
    {
        bb.animationEnded = true;
    }

    public void AnimationTrigger()
    {
        bb.animationTrigger = true;
    }

    public void Reset()
    {
        //blackboard.animator.Rebind();
        bb.ResetGameVariables();
        //Debug.Log("Player Reset");
    }

    public void Spawn(bool invulnerabilityTime = false)
    {
        forceMovementEnabled = false;
        bb.animator.Rebind();
        bb.ResetLifeVariables();

        bb.verticalVelocity = Physics.gravity.y;

        currentState = null;
        ChangeState(bb.spawningState);

        if (invulnerabilityTime)
            currentState.StartInvulnerabilityTime();

        //Debug.Log("Player Spawn");
    }

    void Update()
    {
        //TEST
        if (bb.controller.GetControl(InputControlType.Back).IsPressed)
            rsc.rumbleMng.AddContinousRumble(RumbleType.TEST, 0, weak, strong);
        else
            rsc.rumbleMng.RemoveContinousRumble(RumbleType.TEST);
        //END TEST

        if (bb.active && bb.alive)
        {
            //Reset flags
            if (!bb.shootPressed)
                rsc.rumbleMng.RemoveContinousRumble(RumbleType.PLAYER_SHOOT);
            rsc.rumbleMng.RemoveContinousRumble(RumbleType.PLAYER_DISINFECT);

            bb.ResetFlagVariables();

            if(bb.contactFlag && currentState != null)
            {
                ChangeStateIfNotNull(currentState.EnemyContactOnInvulnerabilityEnd());
            }

            currentState.RetrieveInput();
            //RechargeMovingCharge();

            if (currentState != null)
            {
                ChangeStateIfNotNull(currentState.Update());
            }

            if(gameObject.activeSelf) //Check to avoid a warning trying to update CharacterController if we just died and deactivated gameObject
                UpdatePosition();

            //Show arrow to boss if needed
            GameObject worm = rsc.enemyMng.GetWormGOIfNotVisible();
            if (worm != null)
            {
                bossIndicator.SetActive(true);
                Vector3 wormPos = worm.transform.position;
                wormPos.y = bossIndicator.transform.position.y;

                bossIndicator.transform.LookAt(wormPos);
            }
            else
                bossIndicator.SetActive(false);
        }
    }  

    private void RechargeMovingCharge()
    {
        /*if (blackboard.fastMovementCharge < 100f)
        {
            blackboard.fastMovementCharge += Time.deltaTime * movingRechargeXSecond;
        }*/
    }

    public void UpdatePosition()
    {
        totalHorizontalDirection = bb.horizontalDirection * bb.currentSpeed;

        float otherModifier = 0f;
        if (!bb.firstShot)
            otherModifier = Mathf.Max(otherModifier, speedRatioReductionWhileFiring);

        if (bb.isAffectedByContact)
            otherModifier = Mathf.Max(otherModifier, speedRatioReductionOnContact);

        if(otherModifier > 0)
            totalHorizontalDirection *= otherModifier;

        Vector3 currentFrameDirection = totalHorizontalDirection;

        if (bb.updateVerticalPosition)
        {
            currentFrameDirection.y = bb.verticalVelocity;
        }

        currentFrameDirection *= Time.deltaTime;

        ctrl.Move(currentFrameDirection);

        bb.isGrounded = ctrl.isGrounded;

        UpdateVerticalVelocity();
    }

    private void UpdateVerticalVelocity()
    {
        if (bb.isGrounded)
        {
            bb.verticalVelocity = bb.currentGravity * Time.deltaTime;
        }
        else
        {
            bb.verticalVelocity += bb.currentGravity * Time.deltaTime;
        }
    }

    private void ChangeStateIfNotNull(PlayerBaseState newState)
    {
        if (newState != null)
            ChangeState(newState);
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

        EnemyExplosionController explosion = rsc.poolMng.enemyExplosionPool.GetObject();

        if (explosion != null)
        {
            explosion.transform.position = transform.position;
            explosion.PlayAll();
        }
    }

    public void TakeDamage(float damage)
    {
        ChangeStateIfNotNull(currentState.TakeDamage(damage, bb.receivingDamageState));
    }

    public void ReceiveAttack(float damage, ChromaColor color, Vector3 origin)
    {
        ChangeStateIfNotNull(currentState.AttackReceived(damage, color, origin));
    }

    public void ReceiveInfection(float damage, Vector3 origin, Vector2 infectionForces)
    {
        ChangeStateIfNotNull(currentState.InfectionReceived(damage, origin, infectionForces));
    }

    public void ReceiveInfection(float damage)
    {
        ChangeStateIfNotNull(currentState.InfectionReceived(damage));
    }

    public void ColorMismatch()
    {
        ChangeStateIfNotNull(currentState.ColorMismatch());       
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hexagon")
        {
            HexagonController hex = other.GetComponent<HexagonController>();
            bb.hexagons.Add(hex);
            //Debug.Log("Hexagon added: " + bb.hexagons.Count);
        }
        else if (other.tag == "Border")
        {
            bb.isInBorder = true;
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
            if (rsc.debugMng.godMode)
            {
                PlayerEventInfo.eventInfo.player = bb.player;
                rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_OUT_OF_ZONE, PlayerEventInfo.eventInfo);
            }
            else
                TakeDamage(1000f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Hexagon")
        {
            HexagonController hex = other.GetComponent<HexagonController>();
            bb.hexagons.Remove(hex);
            //Debug.Log("Hexagon removed: " + bb.hexagons.Count);
        }
        else if (other.tag == "Border")
        {
            bb.isInBorder = false;
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.tag == "Enemy")
        {
            ChangeStateIfNotNull(currentState.EnemyTouched());        
        }
    }

    public HexagonController GetNearestHexagon()
    {
        if (bb.hexagons.Count == 0) return null;

        HexagonController result = bb.hexagons[0];
        float distance = (result.transform.position - transform.position).sqrMagnitude;

        for(int i = 1; i < bb.hexagons.Count; ++i)
        {
            float newDistance = (bb.hexagons[i].transform.position - transform.position).sqrMagnitude;

            if(newDistance < distance)
            {
                distance = newDistance;
                result = bb.hexagons[i];
            }
        }

        return result;
    }

    public void ActivateShield()
    {
        bb.shield.SetActive(true);
        bb.laserAim.SetActive(true);
    }

    public void DeactivateShield()
    {
        bb.shield.SetActive(false);
        bb.laserAim.SetActive(false);
    }

    public void SetCapacitor(CapacitorController capacitor)
    {
        ButtonHintEventInfo.eventInfo.playerId = playerId;
        ButtonHintEventInfo.eventInfo.buttonType = ButtonHintEventInfo.ButtonType.COLOR_BUTTONS;
        ButtonHintEventInfo.eventInfo.show = (capacitor != null);

        rsc.eventMng.TriggerEvent(EventManager.EventType.BUTTON_HINT, ButtonHintEventInfo.eventInfo);

        bb.capacitor = capacitor;
    }

    public void SetDevice(DeviceController device)
    {
        ButtonHintEventInfo.eventInfo.playerId = playerId;
        ButtonHintEventInfo.eventInfo.buttonType = ButtonHintEventInfo.ButtonType.A;
        ButtonHintEventInfo.eventInfo.show = (device != null);

        rsc.eventMng.TriggerEvent(EventManager.EventType.BUTTON_HINT, ButtonHintEventInfo.eventInfo);

        bb.device = device;
    }

    //Particle Systems methods
    public void SpawnDashParticles()
    {
        dashPSs[(int)bb.currentColor].Play();
    }

    public void EnteredUSB()
    {
        ChangeState(bb.invisibleState);
        rsc.rumbleMng.RemoveContinousRumble(RumbleType.PLAYER_SHOOT);
        rsc.rumbleMng.AddContinousRumble(RumbleType.PLAYER_USB, Id, 0.2f, 0f);
        DeactivateShield();
        trail.enabled = false;
        ui.SetActive(false);
        lifeChargehargePS.Stop();
        electricPS.Play();
    }

    public void ExitedUSB()
    {
        electricPS.Stop();
        trail.enabled = true;
        ui.SetActive(true);
        rsc.rumbleMng.RemoveContinousRumble(RumbleType.PLAYER_USB);
        ChangeState(bb.idleState);
    }

    public void StartTrail()
    {
        trail.enabled = true;
    }

    public void StopTrail()
    {
        trail.enabled = false;
    }

    public void EnableUI()
    {
        ui.SetActive(true);
    }

    public void DisableUI()
    {
        ui.SetActive(false);
    }

    public void StartSpecialEnergyCharging()
    {
        energyChargehargePS.Play();
    }

    public void StopSpecialEnergyCharging()
    {
        energyChargehargePS.Stop();
    }

    public void StartSpecial()
    {
        SpecialAttackController special = rsc.poolMng.player1SpecialAttackPool.GetObject();

        if(special != null)
        {
            special.transform.position = transform.position;
        }
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
