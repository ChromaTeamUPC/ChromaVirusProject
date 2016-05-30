using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private int playerId = 0;
        
    private bool active = false;

    //Life
    [Header("Health Settings")]
    public int maxLives = 3; 
    public int maxHealth = 100;
    public bool canTakeDamage = true;
    private int currentLives;
    public int currentHealth;

    public float fallDamage = 10;
    public float damageEveryXUnits = 10;

    //Movement
    [Header("Movement Settings")]
    public float speed = 10;
    public float moveThreshold = 0.2f;
    public float angularSpeed = 1080;
    public float aimThreshold = 0.2f;
    public float maxDashTime = 1.0f;
    public float initialDashSpeed = 20.0f;
    public float dashDeceleration = 5f;
    public bool decelerationRatio = false;
    public float minDashSpeed = 1;
    //public float dashStoppingSpeed = 1.0f;

    [HideInInspector]
    public float currentSpeed = 0f;
    [HideInInspector]
    public bool isGrounded = true;
    [HideInInspector]
    public bool isInBorder;
    private float verticalVelocity = 0f;
    private Vector3 horizontalDirection = Vector3.zero;
    private Vector3 aimingDirection = Vector3.zero;    

    private int playerRayCastMask;

    //Attack
    [Header("Energy Settings")]
    public float maxEnergy = 100;
    private float currentEnergy;

    [Header("Fire Settings")]
    public float fireRate = 0.25f;
    public float speedRatioWhileFiring = 0.6f;
    public Transform shotSpawn;
    public Transform muzzlePoint;

    private float nextFire;
    private bool isFirstShot = true;
    private const float maxSideOffset = 0.4f;
    private const float minSideOffset = 0.2f;
    private float shotSideOffset = minSideOffset;
    private float sideOffsetVariation = -0.05f;

    //Misc
    [Header("Miscelaneous Settings")]
    public float idleRandomAnimTime = 10f;

    [SerializeField]
    private ParticleSystem[] dashPSs = new ParticleSystem[4];
    private Transform dashPSRotator;

    //Private atributes
    [HideInInspector]
    public bool keyPressed = false;

    private string moveHorizontal;
    private string moveVertical;
    private string aimHorizontal;
    private string aimVertical;
    private string fire;
    private string dash;
    private string special;

    [HideInInspector]
    public bool animationEnded;

    private bool currentShootingStatus = false;
    private bool newShootingStatus = false;
    private bool doubleShooting = false;

    private ChromaColor currentColor;

    [HideInInspector]
    public Animator animator;
    [SerializeField]
    private Renderer bodyRend;
    [SerializeField]
    private Renderer shieldRend;
    private CharacterController ctrl;
    private ColoredObjectsManager coloredObjMng;
    private VoxelizationClient voxelization;
    
    private BlinkController blinkController;

    //Player states
    public PlayerSpawningState spawningState;
    public PlayerIdleState idleState;
    public PlayerLongIdleState longIdleState;
    public PlayerMovingState movingState;
    public PlayerDashingState dashingState;
    public PlayerSpecialState specialState;
    public PlayerSwingingState swingingState;
    public PlayerReceivingDamageState receivingDamageState;
    public PlayerFallingState fallingState;
    public PlayerDyingState dyingState;

    private PlayerBaseState currentState;  

    //Properties
    public bool Active { get { return active; } set { active = value; } }
    public int Id { get { return playerId; } }
    public int Lives {  get { return currentLives; } }
    public int Health { get { return currentHealth; } }
    public int Energy { get { return (int)currentEnergy; } }

    void Awake()
    {
        voxelization = GetComponentInChildren<VoxelizationClient>();
        ctrl = GetComponent<CharacterController>();
        blinkController = GetComponent<BlinkController>();
        animator = GetComponent<Animator>();

        dashPSRotator = transform.Find("DashPSRotation");

        spawningState = new PlayerSpawningState();
        idleState = new PlayerIdleState();
        longIdleState = new PlayerLongIdleState();
        movingState = new PlayerMovingState();
        dashingState = new PlayerDashingState();
        specialState = new PlayerSpecialState();
        swingingState = new PlayerSwingingState();
        receivingDamageState = new PlayerReceivingDamageState();
        fallingState = new PlayerFallingState();
        dyingState = new PlayerDyingState();

        spawningState.Init(this);
        idleState.Init(this);
        longIdleState.Init(this);
        movingState.Init(this);
        dashingState.Init(this);
        specialState.Init(this);
        swingingState.Init(this);
        receivingDamageState.Init(this);
        fallingState.Init(this);
        dyingState.Init(this);

        currentState = spawningState;

        string player = "";
        switch (playerId)
        {
            case 1: player = "P1"; break;
            case 2: player = "P2"; break;
        }

        moveHorizontal = player + "_Horizontal";
        moveVertical = player + "_Vertical";
        aimHorizontal = player + "_AimHorizontal";
        aimVertical = player + "_AimVertical";
        fire = player + "_Fire";
        dash = player + "_Dash";
        special = player + "_Special";

        playerRayCastMask = LayerMask.GetMask(player + "RayCast");

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
        Material[] mats = bodyRend.materials;
        mats[1] = bodyMat;
        bodyRend.materials = mats;

        shieldRend.material = shieldMat;

        blinkController.InvalidateMaterials();
    }

    public void ResetPlayer()
    {
        currentLives = maxLives;
        currentHealth = maxHealth;
        currentEnergy = 0;
    }

    public void Spawn()
    {
        currentHealth = maxHealth;
        currentEnergy = 0;
        verticalVelocity = Physics.gravity.y;

        ChangeState(spawningState);
        currentShootingStatus = false;
        newShootingStatus = false;
        doubleShooting = false;
        PlayerSpawnedEventInfo.eventInfo.player = this;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNED, PlayerSpawnedEventInfo.eventInfo);
    }

    public void AnimationEnded()
    {
        animationEnded = true;
    }

    void Update()
    {
        //Reset flags
        keyPressed = false;
        newShootingStatus = false;

        if (currentState != null)
        {
            PlayerBaseState newState = currentState.Update();
            if (newState != null)
            {
                ChangeState(newState);
            }
        }

        UpdatePosition();

        if(currentShootingStatus != newShootingStatus)
        {
            currentShootingStatus = newShootingStatus;
            animator.SetBool("Shooting", currentShootingStatus);
            if (!currentShootingStatus)
                doubleShooting = false;
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

    public bool GetAimingDirectionFromInput()
    {
        float h = Input.GetAxisRaw(aimHorizontal);
        float v = Input.GetAxisRaw(aimVertical);

        aimingDirection = Vector3.zero;
        bool result = false;

        if (Mathf.Abs(v) >= aimThreshold || Mathf.Abs(h) >= aimThreshold)
        {
            aimingDirection.x = h;
            aimingDirection.y = v;

            aimingDirection = GetScreenRelativeDirection(aimingDirection);
            keyPressed = true;
            result = true;
        }

        return result;
    }

    public void UpdateLookAt()
    {
        if (aimingDirection != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(aimingDirection);
            newRotation = Quaternion.RotateTowards(transform.rotation, newRotation, angularSpeed * Time.deltaTime);
            transform.rotation = newRotation;
        }
    }

    public bool Turn()
    {
        bool result = GetAimingDirectionFromInput();
        UpdateLookAt();

        return result;
    }

    public bool GetHorizontalDirectionFromInput()
    {
        float h = Input.GetAxisRaw(moveHorizontal);
        float v = Input.GetAxisRaw(moveVertical);

        horizontalDirection = Vector3.zero;
        bool result = false;

        if (Mathf.Abs(v) > 0 || Mathf.Abs(h) > 0)
        {
            horizontalDirection.x = h;
            horizontalDirection.y = v;

            horizontalDirection = GetScreenRelativeDirection(horizontalDirection);
            keyPressed = true;
            result = true;
        }

        return result;
    }

    public void UpdateMovingAnimatorFlags()
    {
        if (horizontalDirection != Vector3.zero)
        {
            //If we are not aiming, rotate towards direction
            if (aimingDirection == Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(horizontalDirection);
                newRotation = Quaternion.RotateTowards(transform.rotation, newRotation, angularSpeed * Time.deltaTime);
                transform.rotation = newRotation;
                animator.SetBool("WalkingAiming", false);
            }
            else
            {
                int angleBetweenSticks = AngleBetween360(aimingDirection, horizontalDirection);

                float angleRad = angleBetweenSticks * Mathf.Deg2Rad;
                float forward = Mathf.Cos(angleRad);
                float lateral = Mathf.Sin(angleRad);
                animator.SetFloat("Forward", forward);
                animator.SetFloat("Lateral", lateral);

                animator.SetBool("WalkingAiming", true);

                //Debug.Log("Moving: " + horizontalDirection + " // Aiming: " + aimingDirection);
                //Debug.Log("Angle: " + angleBetweenSticks + " // Forward: " + forward + " // Lateral: " + lateral);
            }
        }
    }

    public bool Move()
    {
        bool result = GetHorizontalDirectionFromInput();
        UpdateMovingAnimatorFlags();

        return result;
    }

    private int AngleBetween360(Vector3 v1, Vector3 v2)
    {
        Vector3 n = new Vector3(0, 1, 0);

        float signedAngle = Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

        if (signedAngle >= 0)
            return (int)signedAngle;
        else
            return (int)(360 + signedAngle);
    }

    public void SetDashDirection()
    {
        GetHorizontalDirectionFromInput();

        if (horizontalDirection == Vector3.zero)
        {
            horizontalDirection = transform.TransformDirection(Vector3.forward);
        }

        dashPSRotator.rotation = Quaternion.LookRotation(horizontalDirection);

    }

    public void SpawnDashParticles()
    {
        dashPSs[(int)currentColor].Play();
    }

    public void UpdatePosition()
    {
        Vector3 totalDirection = horizontalDirection * currentSpeed;

        if (doubleShooting)
            totalDirection *= speedRatioWhileFiring;

        totalDirection.y = verticalVelocity;

        totalDirection *= Time.deltaTime;

        ctrl.Move(totalDirection);

        isGrounded = ctrl.isGrounded;

        UpdateVerticalVelocity();
    }

    private void UpdateVerticalVelocity()
    {
        if (isGrounded)
        {
            verticalVelocity = Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }


    public Vector3 GetScreenRelativeDirection(Vector3 direction)
    {
        return rsc.camerasMng.GetDirection(transform.position, direction, playerRayCastMask);
    }


    public bool Shoot()
    {
        bool shooting = false;

        if (Input.GetAxisRaw(fire) > 0.1f)
        { 
            shooting = true;
            keyPressed = true;

            if (Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;

                // check if it's first shot (single projectile)...
                if (isFirstShot)
                {
                    //Get a shot from pool
                    PlayerShotController shot = coloredObjMng.GetPlayer1Shot();
                    MuzzleController muzzle = coloredObjMng.GetPlayer1Muzzle();

                    if (shot != null && muzzle != null)
                    {
                        shot.transform.position = shotSpawn.position;
                        shot.transform.rotation = shotSpawn.rotation;
                        shot.damage *= 2;
                        shot.Shoot();

                        muzzle.transform.SetParent(muzzlePoint);
                        muzzle.transform.position = muzzlePoint.position;
                        muzzle.transform.rotation = muzzlePoint.rotation;
                        muzzle.Play();
                    }
                    isFirstShot = false;
                }
                // ...or not (double projectile)
                else
                {
                    //Get two shots from pool
                    PlayerShotController shot1 = coloredObjMng.GetPlayer1Shot();
                    PlayerShotController shot2 = coloredObjMng.GetPlayer1Shot();

                    MuzzleController muzzle1 = coloredObjMng.GetPlayer1Muzzle();
                    MuzzleController muzzle2 = coloredObjMng.GetPlayer1Muzzle();

                    if (shot1 != null && shot2 != null && muzzle1 != null && muzzle2 != null)
                    {
                        shot1.transform.rotation = shotSpawn.rotation;
                        shot1.transform.position = shotSpawn.position;
                        shot1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                        shot1.Shoot();

                        muzzle1.transform.position = muzzlePoint.position;
                        muzzle1.transform.rotation = muzzlePoint.rotation;
                        muzzle1.transform.SetParent(muzzlePoint);
                        muzzle1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                        muzzle1.Play();

                        shot2.transform.rotation = shotSpawn.rotation;
                        shot2.transform.position = shotSpawn.position;
                        shot2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                        shot2.Shoot();

                        muzzle2.transform.position = muzzlePoint.position;
                        muzzle2.transform.rotation = muzzlePoint.rotation;
                        muzzle2.transform.SetParent(muzzlePoint);
                        muzzle2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                        muzzle2.Play();

                        if (shotSideOffset <= minSideOffset || shotSideOffset >= maxSideOffset)
                            sideOffsetVariation *= -1;

                        shotSideOffset += sideOffsetVariation;

                        doubleShooting = true;
                    }
                }
            }
        }
        else
        {
            isFirstShot = true;
        }

        newShootingStatus = shooting;

        return shooting;
    }

    public bool DashPressed()
    {
        bool result = Input.GetButtonDown(dash);

        if (result)
            keyPressed = true;

        return result;
    }

    public bool SpecialPressed()
    {
        bool result = Input.GetButtonDown(special);

        if (result)
            keyPressed = true;

        return false; //TODO: remove false when implemented
    }

    public void TakeDamage(int damage)
    {
        if (!canTakeDamage) return;

        blinkController.Blink();

        currentHealth -= damage;

        if (currentHealth <= 0) currentHealth = 0;

        //Send event
        PlayerDamagedEventInfo.eventInfo.damage = damage;
        PlayerDamagedEventInfo.eventInfo.currentHealth = currentHealth;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DAMAGED, PlayerDamagedEventInfo.eventInfo);

        if (currentHealth == 0)
        {
            currentLives--;
            ChangeState(dyingState);
        }
        else
        {
            ChangeState(receivingDamageState);
        }
    }

    public void TakeDamage(int damage, ChromaColor color)
    {
        // only for enemy bullets
        /*
        if(color != currentColor)
        {
            TakeDamage(damage);
        }
        */

        TakeDamage(damage);

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Border")
        {
            isInBorder = true;
        }
        else if (other.tag == "DeathZone")
        {
            TakeDamage(1000);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Border")
        {
            isInBorder = false;
        }
    }

    
}
