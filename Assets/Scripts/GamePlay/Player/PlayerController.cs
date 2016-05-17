using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    private enum PLAYER_MOVING_DIRECTION
    {
        NONE,
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT
    }

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

    private int playerRayCastMask;

    //Attack
    [Header("Energy Settings")]
    public float maxEnergy = 100;
    private float currentEnergy;

    [Header("Fire Settings")]
    public float fireRate = 0.25f;
    public Transform shotSpawn;
    public Light shotLight;

    private float nextFire;
    private bool isFirstShot = true;
    private const float maxSideOffset = 0.4f;
    private const float minSideOffset = 0.2f;
    private float shotSideOffset = minSideOffset;
    private float sideOffsetVariation = -0.05f;

    //Misc
    [Header("Miscelaneous Settings")]
    public float idleRandomAnimTime = 10f;

    //Private atributes
    private string moveHorizontal;
    private string moveVertical;
    private string aimHorizontal;
    private string aimVertical;
    private string fire;
    private string dash;
    private string special;

    private bool currentShootingStatus = false;
    private bool newShootingStatus = false;

    private Renderer rend;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public CharacterController ctrl;
    private ColoredObjectsManager coloredObjMng;
    [HideInInspector]
    public VoxelizationClient voxelization;
    private ChromaColor currentColor;

    [HideInInspector]
    public bool isInBorder;

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

    //Unity methods
    void Awake()
    {
        Debug.Log("Player " + playerId + " created.");
        rend = GetComponentInChildren<Renderer>();
        ctrl = GetComponent<CharacterController>();
        voxelization = GetComponent<VoxelizationClient>();

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
        animator = GetComponent<Animator>();
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
        Material mat;
        switch (playerId)
        {
            case 1:
                mat = rsc.coloredObjectsMng.GetPlayer1Material(currentColor);
                break;
            case 2:
                mat = rsc.coloredObjectsMng.GetPlayer1Material(currentColor); //TODO: Change to player2 material
                break;
            default:
                mat = rsc.coloredObjectsMng.GetPlayer1Material(currentColor);
                break;
        }
        Material[] mats = rend.materials;
        mats[1] = mat;
        rend.materials = mats;
    }


    void FixedUpdate()
    {
        //Move();
        //Turn();
        //Shoot();
        //Debug
        if(Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(25);
            return;
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            TakeDamage(100);
            return;
        }

        if (currentState != null)
        {
            PlayerBaseState newState = currentState.Update();
            if (newState != null)
            {
                ChangeState(newState);
            }
        }

        if(currentShootingStatus != newShootingStatus)
        {
            currentShootingStatus = newShootingStatus;
            newShootingStatus = false;
            animator.SetBool("Shooting", currentShootingStatus);
            //Debug.Log(currentShootingStatus);
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

    public void InitPlayer()
    {        
        //ResetPlayer();
        //gameObject.SetActive(true);
        //rsc.eventMng.TriggerEvent(EventMng.EventType.PLAYER_SPAWNED, new PlayerSpawnedEventInfo { player = this });
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
        currentState = spawningState;
        currentState.OnStateEnter();
        currentShootingStatus = false;
        newShootingStatus = false;
        PlayerSpawnedEventInfo.eventInfo.player = this;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_SPAWNED, PlayerSpawnedEventInfo.eventInfo);
    }

    public void AnimationEnded()
    {
        if (currentState != null)
            currentState.AnimationEnded();
    }

    public Vector3 GetMovingVector()
    {
        float h = Input.GetAxisRaw(moveHorizontal);
        float v = Input.GetAxisRaw(moveVertical);

        Vector3 result = Vector3.zero;

        if (Mathf.Abs(v) > 0 || Mathf.Abs(h) > 0)
        {
            result.x = h;
            result.y = v;
        }

        return result;
    }

    public Vector3 GetAimingDirection()
    {
        float h = Input.GetAxisRaw(aimHorizontal);
        float v = Input.GetAxisRaw(aimVertical);

        Vector3 result = Vector3.zero;

        if (Mathf.Abs(v) >= aimThreshold || Mathf.Abs(h) >= aimThreshold)
        {
            result.x = h;
            result.y = v;
        }

        return result;
    }

    public Vector3 GetScreenRelativeDirection(Vector3 direction)
    {
        return rsc.camerasMng.GetDirection(transform.position, direction, playerRayCastMask);
    }

    public bool Move()
    {
        Vector3 direction = GetMovingVector();

        bool moving = false;

        if (direction != Vector3.zero)
        {
            moving = true;

            direction = GetScreenRelativeDirection(direction);

            //If we are not aiming, rotate towards direction
            if (GetAimingDirection() == Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(direction);
                newRotation = Quaternion.RotateTowards(transform.rotation, newRotation, angularSpeed * Time.deltaTime);
                transform.rotation = newRotation;
                animator.SetInteger("WalkingMode", (int)PLAYER_MOVING_DIRECTION.NONE);
            }
            else
            {
                int angleBetweenSticks = AngleBetween360(GetMovingVector(), GetAimingDirection());

                if (angleBetweenSticks <= 45 || angleBetweenSticks > 315)
                    animator.SetInteger("WalkingMode", (int)PLAYER_MOVING_DIRECTION.FORWARD);
                else if (angleBetweenSticks > 135 && angleBetweenSticks <= 225)
                    animator.SetInteger("WalkingMode", (int)PLAYER_MOVING_DIRECTION.BACKWARD);
                else if (angleBetweenSticks > 45 && angleBetweenSticks <= 135)
                    animator.SetInteger("WalkingMode", (int)PLAYER_MOVING_DIRECTION.RIGHT);
                else if (angleBetweenSticks > 225 && angleBetweenSticks <= 315)
                    animator.SetInteger("WalkingMode", (int)PLAYER_MOVING_DIRECTION.LEFT);
            }
            // TODELETE
            //Debug.Log(AngleBetween360(GetMovingVector(), GetAimingDirection()));
        }
        else if (GetAimingDirection() != Vector3.zero)
            animator.SetInteger("WalkingMode", (int)PLAYER_MOVING_DIRECTION.FORWARD);

        ctrl.SimpleMove(direction * speed);

        return moving;
    }
    

    public bool Turn()
    {
        Vector3 direction = GetAimingDirection();

        bool aiming = false;

        if (direction != Vector3.zero)
        {
            aiming = true;

            direction = GetScreenRelativeDirection(direction);

            Quaternion newRotation = Quaternion.LookRotation(direction);
            newRotation = Quaternion.RotateTowards(transform.rotation, newRotation, angularSpeed * Time.deltaTime);
            transform.rotation = newRotation;
        }

        return aiming;
    }

    public bool Shoot()
    {
        bool shooting = false;
        shotLight.enabled = false;

        if (Input.GetAxisRaw(fire) > 0.1f)
        { 
            shooting = true;
            if (Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                shotLight.color = coloredObjMng.GetPlayerShotLightColor();
                shotLight.enabled = true;

                // check if it's first shot (single projectile)...
                if (isFirstShot)
                {
                    //Get a shot from pool
                    PlayerShotController shot = coloredObjMng.GetPlayerShot();

                    if (shot != null)
                    {
                        shot.transform.position = shotSpawn.position;
                        shot.transform.rotation = shotSpawn.rotation;
                        shot.damage *= 2;
                        shot.Shoot();
                    }
                    isFirstShot = false;
                }
                // ...or not (double projectile)
                else
                {
                    //Get two shots from pool
                    PlayerShotController shot1 = coloredObjMng.GetPlayerShot();
                    PlayerShotController shot2 = coloredObjMng.GetPlayerShot();

                    if (shot1 != null && shot2 != null)
                    {
                        shot1.transform.rotation = shotSpawn.rotation;
                        shot1.transform.position = shotSpawn.position;
                        shot1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                        shot1.Shoot();

                        shot2.transform.rotation = shotSpawn.rotation;
                        shot2.transform.position = shotSpawn.position;
                        shot2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                        shot2.Shoot();

                        if (shotSideOffset <= minSideOffset || shotSideOffset >= maxSideOffset)
                            sideOffsetVariation *= -1;

                        shotSideOffset += sideOffsetVariation;
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
        return Input.GetButtonDown(dash);
    }

    public bool SpecialPressed()
    {
        return false;
    }

    public void TakeDamage(int damage)
    {
        if (!canTakeDamage) return;

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
            //voxelization.CalculateVoxelsGrid();
            //voxelization.SpawnVoxels();
            //rsc.eventMng.TriggerEvent(EventMng.EventType.PLAYER_DIED, new PlayerSpawnedEventInfo { player = this });
            //gameObject.SetActive(false);
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
        else if (other.tag == "DestroyerBoundary")
        {
            TakeDamage(currentHealth);
        }
    }

    private int  AngleBetween360(Vector3 v1, Vector3 v2)
    {
        Vector3 n = new Vector3(0, 0, 1);

        float signedAngle = Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)),Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

        if (signedAngle >= 0)
            return (int) signedAngle;
        else
            return (int) (360 + signedAngle);
    }
}
