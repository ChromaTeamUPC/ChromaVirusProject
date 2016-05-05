using UnityEngine;
using System.Collections;

public class PlayerController2 : MonoBehaviour {

    [SerializeField]
    private int playerId;

    private bool active = false;

    //Life
    public int maxLives = 3;
    public int maxHealth = 100;
    public bool canTakeDamage = true;
    private int currentLives;
    private int currentHealth;

    //Movement
    public float speed = 10;
    public float moveThreshold = 0.2f;
    public float angularSpeed = 1080;
    public float aimThreshold = 0.2f;

    //Attack
    public float maxEnergy = 100;
    //public float energyLostPerShot = 5;
    //public float energyRecoveryPerSecond = 5;
    public float fireRate = 0.25f;
    public Transform shotSpawn;

    private float currentEnergy;

    private bool isFirstShot = true;
    private float nextFire;
    private const float maxSideOffset = 0.4f;
    private const float minSideOffset = 0.2f;
    private float shotSideOffset = minSideOffset;
    private float sideOffsetVariation = -0.05f;

    public Light shotLight;



    //Private atributes
    private string moveHorizontal;
    private string moveVertical;
    private string aimHorizontal;
    private string aimVertical;
    private string fire;
    private string dash;

    //Misc
    public float idleRandomAnimTime = 10f;
    private Renderer rend;
    private CharacterController ctrl;
    private ColoredObjectsManager coloredObjMng;
    private VoxelizationClient voxelization;
    private ChromaColor currentColor;

    private int playerRayCastMask;

    //Properties
    public bool Active { get { return active; } set { active = value; } }
    public int Id { get { return playerId; } }
    public int Lives { get { return currentLives; } }
    public int Health { get { return currentHealth; } }
    public int Energy { get { return (int)currentEnergy; } }

    void Awake()
    {
        Debug.Log("Player " + playerId + " created.");
        rend = GetComponentInChildren<Renderer>();
        
        ctrl = GetComponent<CharacterController>();
        voxelization = GetComponent<VoxelizationClient>();

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
        playerRayCastMask = LayerMask.GetMask(player + "RayCast");
    }

    // Use this for initialization
    void Start ()
    {
	
	}

    public void InitPlayer(int playerNumber)
    {
    }

    public void ResetPlayer()
    {
        currentLives = maxLives;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
    }

    // Update is called once per frame
    void Update ()
    {
        Move();
        Turn();
        Shoot();
	}

    public bool Move()
    {
        float h = Input.GetAxisRaw(moveHorizontal);
        float v = Input.GetAxisRaw(moveVertical);

        Vector3 direction = Vector3.zero;
        bool moved = false;

        if (Mathf.Abs(v) > 0 || Mathf.Abs(h) > 0)
        {
            moved = true;
            direction.x = h;
            direction.y = v;

            direction = rsc.camerasMng.GetDirection(transform.position, direction, playerRayCastMask);

            //If we are not aiming, rotate towards direction
            float ah = Input.GetAxisRaw(aimHorizontal);
            float av = Input.GetAxisRaw(aimVertical);
            if (Mathf.Abs(av) < aimThreshold && Mathf.Abs(ah) < aimThreshold)
            {
                Quaternion newRotation = Quaternion.LookRotation(direction);
                newRotation = Quaternion.RotateTowards(transform.rotation, newRotation, angularSpeed * Time.deltaTime);
                transform.rotation = newRotation;
            }

        }

        ctrl.SimpleMove(direction * speed);

        return moved;
    }

    public bool Turn()
    {
        float h = Input.GetAxisRaw(aimHorizontal);
        float v = Input.GetAxisRaw(aimVertical);

        bool aiming = false;

        if (Mathf.Abs(v) >= aimThreshold || Mathf.Abs(h) >= aimThreshold)
        {
            aiming = true;
            Vector3 direction = new Vector3(h, v, 0);

            direction = rsc.camerasMng.GetDirection(transform.position, direction, playerRayCastMask);

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
                    GameObject shot = coloredObjMng.GetPlayerShot();

                    if (shot != null)
                    {
                        shot.transform.position = shotSpawn.position;
                        shot.transform.rotation = shotSpawn.rotation;
                        PlayerShotController controller = shot.GetComponent<PlayerShotController>();
                        controller.damage *= 2;
                        controller.Shot();
                    }
                    isFirstShot = false;
                }
                // ...or not (double projectile)
                else
                {
                    //Get two shots from pool
                    GameObject shot1 = coloredObjMng.GetPlayerShot();
                    GameObject shot2 = coloredObjMng.GetPlayerShot();

                    if (shot1 != null && shot2 != null)
                    {
                        shot1.transform.rotation = shotSpawn.rotation;
                        shot1.transform.position = shotSpawn.position;
                        shot1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                        shot1.GetComponent<PlayerShotController>().Shot();

                        shot2.transform.rotation = shotSpawn.rotation;
                        shot2.transform.position = shotSpawn.position;
                        shot2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                        shot2.GetComponent<PlayerShotController>().Shot();

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

        return shooting;
    }

    public void TakeDamage(int damage)
    {
        
    }

    public void TakeDamage(int damage, ChromaColor color)
    {
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "DestroyerBoundary")
        {
            TakeDamage(currentHealth);
        }
    }
}
