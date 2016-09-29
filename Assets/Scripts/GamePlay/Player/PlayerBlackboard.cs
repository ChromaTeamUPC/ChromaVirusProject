using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

//Class to encapsulate all variables and objects that needs to be shared between PlayerController and its states
public class PlayerBlackboard
{
    //Components, gameObjects and scripts
    public PlayerController player; //Never reset
    public BlinkController blinkController; //Never reset
    public Animator animator; //Never reset
    public GameObject shield; //Never reset
    public GameObject laserAim; //Never reset
    public InputDevice controller; //Never reset

    public int playerRayCastMask; //Never reset
    public int playerPhysicsLayer; //Never reset
    public int enemyPhysicsPlayer; //Never reset

    //Player states
    public PlayerSpawningState spawningState; //Never reset
    public PlayerIdleState idleState; //Never reset
    public PlayerLongIdleState longIdleState; //Never reset
    public PlayerMovingState movingState; //Never reset
    public PlayerDashingState dashingState; //Never reset
    public PlayerSpeedBumpState speedBumpState; //Never reset
    public PlayerSpecialState specialState; //Never reset
    public PlayerReceivingDamageState receivingDamageState; //Never reset
    public PlayerPushedState pushedState; //Never reset
    public PlayerFallingState fallingState; //Never reset
    public PlayerDyingState dyingState; //Never reset
    public PlayerBlockedState blockedState; //Never reset
    public PlayerInvisibleState invisibleState; //Never reset
    public PlayerLevelClearedState levelClearedState; //Never reset

    //Controller mapped attributes
    public string moveHorizontal; //Never reset
    public string moveVertical; //Never reset
    public string aimHorizontal; //Never reset
    public string aimVertical; //Never reset
    public string fire; //Never reset
    public string dash; //Never reset
    public string special; //Never reset
    public string greenButton; //Never reset
    public string redButton; //Never reset
    public string blueButton; //Never reset
    public string yellowButton; //Never reset

    //Input variables
    private bool keyPressed; //Reset per frame
    public Vector2 screenVector; //Reset per frame
    public Vector3 moveVector; //Reset per frame
    public bool movePressed; //Reset per frame
    public Vector3 aimVector; //Reset per frame
    public bool aimPressed; //Reset per frame
    public bool shootPressed; //Reset per frame
    public bool dashPressed; //Reset per frame
    public bool dashWasPressed; //Reset per frame
    public bool specialPressed; //Reset per frame
    public bool greenPressed; //Reset per frame
    public bool redPressed; //Reset per frame
    public bool bluePressed; //Reset per frame
    public bool yellowPressed; //Reset per frame
    public bool colorButtonsPressed; //Reset per frame

    //State control variables
    public bool active;     //Player is participating in current game (not necesarily alive) //Reset per game
    public bool alive;      //Player is alive at the moment //Reset per game
    public bool playing;    //Player is playing (game started and not all lives lost)
    public bool animationTrigger; //Reset per needed state
    public bool animationEnded; //Reset per needed state
    public bool isGrounded;
    public bool falling; //Reset when player returns back to idle, but not when die

    //Health variables
    public int currentLives;
    public float currentHealth;
    public float currentEnergy;
    public bool isInvulnerable;
    public ChromaColor currentColor;
    public bool contactFlag;

    //Movement variables
    public Vector3 horizontalDirection;
    public float currentSpeed;
    public bool isAffectedByContact;
    public bool isContactCooldown;
    public bool updateVerticalPosition;
    public float verticalVelocity;
    public float currentGravity;
    //public float fastMovementCharge;

    //Shoot variables
    public bool canShoot;
    public bool firstShot;

    //Special attack variables
    public List<EnemyBaseAIBehaviour> enemiesInRange = new List<EnemyBaseAIBehaviour>();
    public List<EnemyShotControllerBase> shotsInRange = new List<EnemyShotControllerBase>();
    public List<VortexController> vortexInRange = new List<VortexController>();
    public List<TurretAIBehaviour> turretsInRange = new List<TurretAIBehaviour>();
    public WormAIBehaviour worm;
    public GameObject specialAttackDetector;
    public bool specialAttackTutorialTriggered;

    //Other gameplay variables
    public CapacitorController capacitor;
    public DeviceController device;
    public List<HexagonController> hexagons;
    public Vector3 infectionOrigin;
    public Vector2 infectionForces;
    public Vector3 destinationPoint;

    public void Init(PlayerController pl)
    {
        player = pl;
        blinkController = player.GetComponent<BlinkController>();

        animator = player.GetComponent<Animator>();

        hexagons = new List<HexagonController>();

        specialAttackDetector = player.specialDetector;
        specialAttackDetector.GetComponent<PlayerSpecialAttackDetector>().Blackboard = this;
        specialAttackDetector.GetComponent<SphereCollider>().radius = player.specialAttackAffectationRadius;

        if (InputManager.Devices.Count >= player.Id)
            controller = InputManager.Devices[player.Id - 1];

        spawningState = new PlayerSpawningState();
        idleState = new PlayerIdleState();
        longIdleState = new PlayerLongIdleState();
        movingState = new PlayerMovingState();
        dashingState = new PlayerDashingState();
        speedBumpState = new PlayerSpeedBumpState();
        specialState = new PlayerSpecialState();
        receivingDamageState = new PlayerReceivingDamageState();
        pushedState = new PlayerPushedState();
        fallingState = new PlayerFallingState();
        dyingState = new PlayerDyingState();
        blockedState = new PlayerBlockedState();
        invisibleState = new PlayerInvisibleState();
        levelClearedState = new PlayerLevelClearedState();

        spawningState.Init(this);
        idleState.Init(this);
        longIdleState.Init(this);
        movingState.Init(this);
        dashingState.Init(this);
        speedBumpState.Init(this);
        specialState.Init(this);
        receivingDamageState.Init(this);
        pushedState.Init(this);
        fallingState.Init(this);
        dyingState.Init(this);
        blockedState.Init(this);
        invisibleState.Init(this);
        levelClearedState.Init(this);

        string playerStr = "";
        switch (player.Id)
        {
            case 1: playerStr = "P1"; break;
            case 2: playerStr = "P2"; break;
        }

        moveHorizontal = playerStr + "_Horizontal";
        moveVertical = playerStr + "_Vertical";
        aimHorizontal = playerStr + "_AimHorizontal";
        aimVertical = playerStr + "_AimVertical";
        fire = playerStr + "_Fire";
        dash = playerStr + "_Dash";
        special = playerStr + "_Special";
        greenButton = playerStr + "_Green";
        redButton = playerStr + "_Red";
        blueButton = playerStr + "_Blue";
        yellowButton = playerStr + "_Yellow";

        playerRayCastMask = LayerMask.GetMask(playerStr + "RayCast");
        playerPhysicsLayer = LayerMask.NameToLayer("Player");
        enemyPhysicsPlayer = LayerMask.NameToLayer("Enemy");

        ResetLifeVariables();
    }

    //This variables have to be reset every game
    public void ResetGameVariables()
    {
        active = false;
        alive = false;
        playing = false;
        currentLives = player.maxLives;
        specialAttackTutorialTriggered = false;
        hexagons.Clear();     
    }

    //This variables have to be reset every spawn
    public void ResetLifeVariables()
    {
        alive = true;
        playing = true;

        animationEnded = false;
        isGrounded = true;
        falling = false;

        currentHealth = player.maxHealth;
        currentEnergy = 0;
        player.CheckEnergyFullPS();
        isInvulnerable = false;
        contactFlag = false;

        horizontalDirection = Vector3.zero;
        currentSpeed = 0f;
        isAffectedByContact = false;
        isContactCooldown = false;
        updateVerticalPosition = true;
        currentGravity = Physics.gravity.y;
        //fastMovementCharge = 0f;

        canShoot = true;
        firstShot = true;

        enemiesInRange.Clear();
        shotsInRange.Clear();
        vortexInRange.Clear();
        turretsInRange.Clear();
        worm = null;
        specialAttackDetector.SetActive(false);

        capacitor = null;
        device = null;
        hexagons.Clear();
        infectionOrigin = Vector3.zero;
        infectionForces = Vector2.zero;
        destinationPoint = Vector3.zero;
    }

    //This variables have to be reset every update
    public void ResetFlagVariables()
    {
        keyPressed = false;
        movePressed = false;
        aimPressed = false;
        shootPressed = false;
        dashPressed = false;
        dashWasPressed = false;
        specialPressed = false;
        greenPressed = false;
        redPressed = false;
        bluePressed = false;
        yellowPressed = false;
        colorButtonsPressed = false;

        animator.SetBool("KeyPressed", false);
        animator.SetBool("Walking", false);
        animator.SetBool("Aiming", false);
        animator.SetBool("Shooting", false);
    }

    public Vector3 GetScreenRelativeDirection(Vector3 direction)
    {
        if (rsc.camerasMng == null) return Vector3.zero;

        return rsc.camerasMng.GetDirection(player.transform.position, direction, playerRayCastMask);
    }

    public bool KeyPressed
    {
        get { return keyPressed; }
        set
        {
            keyPressed = value;
        }
    }
}
