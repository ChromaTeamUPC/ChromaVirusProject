using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

//Class to encapsulate all variables and objects that needs to be shared between PlayerController and its states
public class PlayerBlackboard
{
    //Components, gameObjects and scripts
    public PlayerController player;
    public BlinkController blinkController;
    public Animator animator;
    public GameObject shield;
    public GameObject laserAim;
    public InputDevice controller;

    public int playerRayCastMask;
    public int playerPhysicsLayer;
    public int enemyPhysicsPlayer;

    //Player states
    public PlayerSpawningState spawningState;
    public PlayerIdleState idleState;
    public PlayerLongIdleState longIdleState;
    public PlayerMovingState movingState;
    public PlayerDashingState dashingState;
    public PlayerSpeedBumpState speedBumpState;
    public PlayerSpecialState specialState;
    public PlayerSwingingState swingingState;
    public PlayerReceivingDamageState receivingDamageState;
    public PlayerFallingState fallingState;
    public PlayerDyingState dyingState;
    public PlayerBlockedState blockedState;
    public PlayerInvisibleState invisibleState;

    //Controller mapped attributes
    public string moveHorizontal;
    public string moveVertical;
    public string aimHorizontal;
    public string aimVertical;
    public string fire;
    public string dash;
    public string special;
    public string greenButton;
    public string redButton;
    public string blueButton;
    public string yellowButton;

    //Input variables
    public bool keyPressed;
    public Vector3 moveVector;
    public bool movePressed;
    public Vector3 aimVector;
    public bool aimPressed;
    public bool shootPressed;
    public bool dashPressed;
    public bool specialPressed;
    public bool greenPressed;
    public bool redPressed;
    public bool bluePressed;
    public bool yellowPressed;
    public bool colorButtonsPressed;

    //State control variables
    public bool active;     //Player is participating in current game (not necesarily alive)
    public bool alive;      //Player is alive at the moment
    public bool animationTrigger;
    public bool animationEnded;
    public bool isGrounded;
    public bool isInBorder;

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
    public float fastMovementCharge;

    //Shoot variables
    public bool canShoot;
    public bool firstShot;

    //Special attack variables
    public List<EnemyBaseAIBehaviour> enemiesInRange = new List<EnemyBaseAIBehaviour>();
    public GameObject specialAttackDetector;

    //Other gameplay variables
    public CapacitorController capacitor;
    public DeviceController device;

    public void Init(PlayerController pl)
    {
        player = pl;
        blinkController = player.GetComponent<BlinkController>();

        animator = player.GetComponent<Animator>();

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
        swingingState = new PlayerSwingingState();
        receivingDamageState = new PlayerReceivingDamageState();
        fallingState = new PlayerFallingState();
        dyingState = new PlayerDyingState();
        blockedState = new PlayerBlockedState();
        invisibleState = new PlayerInvisibleState();

        spawningState.Init(this);
        idleState.Init(this);
        longIdleState.Init(this);
        movingState.Init(this);
        dashingState.Init(this);
        speedBumpState.Init(this);
        specialState.Init(this);
        swingingState.Init(this);
        receivingDamageState.Init(this);
        fallingState.Init(this);
        dyingState.Init(this);
        blockedState.Init(this);
        invisibleState.Init(this);

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
        currentLives = player.maxLives;       
    }

    //This variables have to be reset every spawn
    public void ResetLifeVariables()
    {
        animationEnded = false;
        isGrounded = true;
        isInBorder = false;

        currentHealth = player.maxHealth;
        currentEnergy = 0;
        isInvulnerable = false;
        contactFlag = false;

        horizontalDirection = Vector3.zero;
        currentSpeed = 0f;
        isAffectedByContact = false;
        isContactCooldown = false;
        updateVerticalPosition = true;
        fastMovementCharge = 0f;

        canShoot = true;
        firstShot = true;

        enemiesInRange.Clear();
        specialAttackDetector.SetActive(false);

        capacitor = null;
        device = null;
    }

    //This variables have to be reset every update
    public void ResetFlagVariables()
    {
        keyPressed = false;
        movePressed = false;
        aimPressed = false;
        shootPressed = false;
        dashPressed = false;
        specialPressed = false;
        greenPressed = false;
        redPressed = false;
        bluePressed = false;
        yellowPressed = false;
        colorButtonsPressed = false;

        animator.SetBool("Walking", false);
        animator.SetBool("Aiming", false);
        animator.SetBool("Shooting", false);
    }
}
