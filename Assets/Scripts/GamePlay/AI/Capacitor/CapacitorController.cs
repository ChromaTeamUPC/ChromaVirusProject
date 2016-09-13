using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CapacitorController : MonoBehaviour {
    public enum CapacitorLevel
    {
        EMPTY,
        ONE_THIRD,
        TWO_THIRDS,
        FULL
    }

    private enum State
    {
        NOT_STARTED,
        IDLE,
        CHARGED,
        EXPLODING
    }

    public float maxCharge = 100;
    public int chargePerShot = 10;
    public bool manualInstantCharge = false;
    public float manualChargePerSecond = 100;
    public float timeToExplode = 5f;
    public int damage = 50;
    public float forceMultiplier = 15f;

    public SphereCollider attractingCollider;
    public float attractingColliderRange;
    public ParticleSystem attractingDome;
    public SphereCollider damageCollider;
    public GameObject detectingPlayerCollider;
    public GameObject modelCollider;
    public float damageColliderRange;
    public GameObject[] explosionBlast = new GameObject[4];

    public Renderer rend;

    private State state;
    private float charge33;
    private float charge66;

    private bool active;

    [HideInInspector]
    public ChromaColor currentColor;
    private float currentCharge;
    private float elapsedTime;
    private float timeToExplodeMinus2;

    private HashSet<EnemyBaseAIBehaviour> enemiesInRange;

    private ColoredObjectsManager colorObjMng;

    private BlinkController blinkController;

    private GameObject model;

    private PlayerController activatingPlayer;


    void Awake()
    {
        attractingCollider.radius = attractingColliderRange;
        damageCollider.radius = damageColliderRange;

        state = State.NOT_STARTED;
        currentCharge = 0;
        elapsedTime = 0f;

        charge33 = maxCharge / 100 * 33;
        charge66 = maxCharge / 100 * 66;

        timeToExplodeMinus2 = timeToExplode -2;

        enemiesInRange = new HashSet<EnemyBaseAIBehaviour>();
        blinkController = GetComponent<BlinkController>();
        model = transform.Find("Model").gameObject;
        active = false;
    }

    void Start()
    {
        colorObjMng = rsc.coloredObjectsMng;
    }

    public void Activate()
    {
        active = true;
    }

    public void Deactivate()
    {
        active = false;
    }

    void Update()
    {
        if (!active) return;

        switch (state)
        {
            case State.IDLE:
                if(currentCharge == maxCharge)
                {
                    attractingCollider.enabled = true;
                    damageCollider.enabled = true;
                    detectingPlayerCollider.SetActive(false);
                    state = State.CHARGED;
                    attractingDome.Play();
                    blinkController.BlinkWhiteIncremental(timeToExplode, 0.03f, 0.5f);
                    elapsedTime = 0f;
                    //Debug.Log("Moving to Charged state");
                }
                break;
            case State.CHARGED:
                elapsedTime += Time.deltaTime;

                if(elapsedTime >= timeToExplode)
                {                    
                    model.SetActive(false);
                    modelCollider.SetActive(false);
                    explosionBlast[(int)currentColor].SetActive(true);
                    state = State.EXPLODING;
                    rsc.rumbleMng.Rumble(0, 0.5f, 0.5f, 0.5f);
                    //Debug.Log("Moving to Exploding state");
                    StartCoroutine(Exploding());
                }
                else if (elapsedTime >= timeToExplodeMinus2)
                {
                    attractingDome.Stop();
                }

                break;
            case State.EXPLODING:
                DestroyObject(gameObject, 5f); //TODO: Adjust time to allow exploding animation to finish;
                break;
            default:
                break;
        }
    }

    private void SetMaterial(Material mat)
    {
        Material[] mats = rend.sharedMaterials;
        mats[0] = mat;
        rend.sharedMaterials = mats;
        blinkController.InvalidateMaterials();
    }

    public void ImpactedByShot(ChromaColor shotColor, PlayerController player)
    {
        if (!active) return;

        switch (state)
        {
            case State.NOT_STARTED:
                state = State.IDLE;               
                currentCharge += chargePerShot;
                currentColor = shotColor;
                SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.EMPTY, currentColor));
                //Debug.Log("Received first shot.");
                //Debug.Log("Current Color = " + ChromaColorInfo.GetColorName(currentColor) + ". Current charge = " + currentCharge);
                //Debug.Log("Moving to Idle state");
                activatingPlayer = player;
                break;

            case State.IDLE:
                if (currentColor == shotColor)
                {
                    if (currentCharge < maxCharge)
                    {
                        float previousCharge = currentCharge;

                        currentCharge += chargePerShot;
                        if (currentCharge > maxCharge)
                            currentCharge = maxCharge;

                        if (previousCharge < maxCharge && currentCharge == maxCharge)
                            SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.FULL, currentColor));
                        else if (previousCharge < charge66 && currentCharge >= charge66)
                            SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.TWO_THIRDS, currentColor));
                        else if (previousCharge < charge33 && currentCharge >= charge33)
                            SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.ONE_THIRD, currentColor));                       
                    }
                }
                else
                {
                    currentCharge = 0;
                    currentColor = shotColor;
                    SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.EMPTY, currentColor));
                }

                activatingPlayer = player;
                //Debug.Log("Current Color = " + ChromaColorInfo.GetColorName(currentColor) + ". Current charge = " + currentCharge);
                break;
        }
    }

    public void ManualCharge(ChromaColor color, PlayerController player)
    {
        if (!active) return;

        switch (state)
        {
            case State.NOT_STARTED:
                state = State.IDLE;
                currentColor = color;
                activatingPlayer = player;

                if (manualInstantCharge)
                {
                    currentCharge = maxCharge;
                    SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.FULL, currentColor));
                }
                else
                {
                    currentCharge += manualChargePerSecond * Time.deltaTime;
                    SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.EMPTY, currentColor));
                }
                //Debug.Log("Received first shot.");
                //Debug.Log("Current Color = " + ChromaColorInfo.GetColorName(currentColor) + ". Current charge = " + currentCharge);
                //Debug.Log("Moving to Idle state");
                break;

            case State.IDLE:
                if (manualInstantCharge)
                {
                    currentColor = color;
                    currentCharge = maxCharge;
                    SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.FULL, currentColor));
                }
                else
                {
                    if (currentColor == color)
                    {
                        if (currentCharge < maxCharge)
                        {
                            float previousCharge = currentCharge;

                            currentCharge += manualChargePerSecond * Time.deltaTime;
                            if (currentCharge > maxCharge)
                                currentCharge = maxCharge;

                            if (previousCharge < maxCharge && currentCharge == maxCharge)
                                SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.FULL, currentColor));
                            else if (previousCharge < charge66 && currentCharge >= charge66)
                                SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.TWO_THIRDS, currentColor));
                            else if (previousCharge < charge33 && currentCharge >= charge33)
                                SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.ONE_THIRD, currentColor));
                        }
                    }
                    else
                    {
                        currentCharge = 0;
                        currentColor = color;
                        SetMaterial(colorObjMng.GetCapacitorMaterial(CapacitorLevel.EMPTY, currentColor));
                    }
                }
                activatingPlayer = player;
                //Debug.Log("Current Color = " + ChromaColorInfo.GetColorName(currentColor) + ". Current charge = " + currentCharge);
                break;
        }
    }

    public void EnemyInRange(EnemyBaseAIBehaviour enemy)
    {
        enemiesInRange.Add(enemy);
    }

    public void EnemyOutOfRange(EnemyBaseAIBehaviour enemy)
    {
        enemiesInRange.Remove(enemy);
    }

    private IEnumerator Exploding()
    {
        yield return new WaitForSeconds(0.1f);
        DamageEnemies();
    }

    private void DamageEnemies()
    {
        foreach(EnemyBaseAIBehaviour enemy in enemiesInRange)
        {
            Vector3 direction = enemy.transform.position - transform.position;
            direction.y = 0;
            direction.Normalize();
            direction *= forceMultiplier;
            enemy.ImpactedByBarrel(currentColor, damage, direction, activatingPlayer);
        }

        attractingCollider.enabled = false;
        damageCollider.enabled = false;
    }
}
