using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class to encapsulate all variables and objects that needs to be shared between PlayerController and its states
public class PlayerBlackboard
{
    //Components, gameObjects and scripts
    public PlayerController player;
    public BlinkController blinkController;
    public Animator animator;

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
    public PlayerBlockedState blockedState;
    public PlayerInvisibleState invisibleState;

    //State control variables
    public bool active;     //Player is participating in current game (not necesarily alive)
    public bool alive;      //Player is alive at the moment
    public bool animationEnded;
    public bool keyPressed;
    public bool isGrounded;
    public bool isInBorder;

    //Health variables
    public int currentLives;
    public float currentHealth;
    public float currentEnergy;
    public bool isInvulnerable;
    public ChromaColor currentColor;

    //Movement variables
    public Vector3 horizontalDirection;
    public Vector3 aimingDirection;
    public float currentSpeed;
    public bool isAffectedByContact;
    public bool isContactCooldown;
    public bool updateVerticalPosition;
    public bool walkingAiming;

    //Shoot variables
    public bool canShoot;
    public bool currentShootingStatus = false;
    public bool newShootingStatus = false;
    public bool doubleShooting = false;

    //Special attack variables
    public List<EnemyBaseAIBehaviour> enemiesInRange = new List<EnemyBaseAIBehaviour>();
    public SphereCollider specialAttackCollider;


    public void Init(PlayerController pl)
    {
        player = pl;
        blinkController = player.GetComponent<BlinkController>();

        animator = player.GetComponent<Animator>();

        specialAttackCollider = player.GetComponent<SphereCollider>();
        specialAttackCollider.radius = player.specialAttackAffectationRadius;

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
        blockedState = new PlayerBlockedState();
        invisibleState = new PlayerInvisibleState();

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
        blockedState.Init(this);
        invisibleState.Init(this);

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

        horizontalDirection = Vector3.zero;
        aimingDirection = Vector3.zero;
        currentSpeed = 0f;
        isAffectedByContact = false;
        isContactCooldown = false;
        updateVerticalPosition = true;

        canShoot = true;
        currentShootingStatus = false;
        doubleShooting = false;

        enemiesInRange.Clear();
        specialAttackCollider.enabled = false;
}

    //This variables have to be reset every update
    public void ResetFlagVariables()
    {
        keyPressed = false;
        newShootingStatus = false;
        walkingAiming = false;
    }
}
