using UnityEngine;
using System.Collections;

public class WormBodySegmentController : MonoBehaviour 
{
    public enum BodySubState
    {
        SETTING,
        NORMAL,
        NORMAL_DISABLED,
        DEACTIVATED,
        DESTROYED
    }

    private BodySubState bodyState;
    public BodySubState BodyState { get { return bodyState; } }

    private float currentDamage;
    private float currentDamageWrongColor;

    [SerializeField]
    private ChromaColor color;

    public ChromaColor Color { get { return color; } }

    [Header("Fx")]
    public GameObject[] bodyDeactivatePrefabs;
    public GameObject bodyDestructionPrefab;

    [Header("Sound Fx")]
    public AudioSource inOutSoundFx;
    public AudioSource explosionOkSoundFx;
    public AudioSource explosionWrongSoundFx;
    public AudioClip finalExplosionSoundFx;

    private ParticleSystem[] bodyDeactivate;
    private ParticleSystem bodyDestruction;

    private BlinkController blinkController;
    private Renderer rend;
    private BoxCollider col;
    private VoxelizationClient voxelization;

    private WormBlackboard bb;
    private WormAIBehaviour head; //Shortcut

    private bool overground;

    void Awake()
    {
        blinkController = GetComponent<BlinkController>();
        rend = GetComponentInChildren<Renderer>();
        col = GetComponent<BoxCollider>();
        voxelization = GetComponentInChildren<VoxelizationClient>();
        bodyState = BodySubState.NORMAL;

        bodyDeactivate = new ParticleSystem[bodyDeactivatePrefabs.Length];

        Transform fx = transform.FindDeepChild("FX");

        GameObject temp;
        for (int i = 0; i < bodyDeactivatePrefabs.Length; ++i)
        {
            temp = Instantiate(bodyDeactivatePrefabs[i]);
            temp.transform.SetParent(fx);
            temp.transform.localPosition = Vector3.zero;
            bodyDeactivate[i] = temp.GetComponent<ParticleSystem>();
        }

        temp = Instantiate(bodyDestructionPrefab);
        temp.transform.SetParent(fx);
        temp.transform.localPosition = Vector3.zero;
        bodyDestruction = temp.GetComponent<ParticleSystem>();

        overground = false;
    }

    void Start()
    {
        SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(color));
    }

    void Update()
    {
        if ((transform.position.y > -1) != overground)
        {
            inOutSoundFx.Play();
            overground = transform.position.y > -1;
        }
    }

    public void SetBlackboard(WormBlackboard bb)
    {
        this.bb = bb;
        head = bb.head;
        currentDamage = 0;
        currentDamageWrongColor = 0;
    }

    public void SetInitialState(ChromaColor c)
    {
        color = c;
        currentDamage = 0;
        currentDamageWrongColor = 0;
        SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(color));

        ColorEventInfo.eventInfo.newColor = color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_ACTIVATED, ColorEventInfo.eventInfo);

        bodyState = BodySubState.NORMAL;
    }

    public bool IsDestroyed()
    {
        return bodyState == BodySubState.DESTROYED;
    }

    public void ResetColor(ChromaColor color)
    {
        if (bodyState == BodySubState.DESTROYED) return;

        ChromaColor oldColor = this.color;
        if(bodyState == BodySubState.DEACTIVATED)
        {
            ColorEventInfo.eventInfo.newColor = color;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_ACTIVATED, ColorEventInfo.eventInfo);
        }
        else
        {
            ColorEventInfo.eventInfo.oldColor = oldColor;
            ColorEventInfo.eventInfo.newColor = color;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_COLOR_CHANGED, ColorEventInfo.eventInfo);
        }

        this.color = color;
        currentDamage = 0;
        currentDamageWrongColor = 0;
        bodyState = BodySubState.SETTING;

        StartCoroutine(SetRandomColors());
    }

    public void Consolidate(ChromaColor color)
    {
        if (bodyState == BodySubState.DESTROYED) return;

        if (bodyState == BodySubState.DEACTIVATED)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormBodyWireframeMaterial());
            bodyDestruction.Play();
            bodyState = BodySubState.DESTROYED;
            //col.enabled = false;
            col.isTrigger = true;
        }
        else
        {
            ChromaColor oldColor = this.color;
            this.color = color;
            currentDamage = 0;
            currentDamageWrongColor = 0;

            ColorEventInfo.eventInfo.oldColor = oldColor;
            ColorEventInfo.eventInfo.newColor = color;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_COLOR_CHANGED, ColorEventInfo.eventInfo);

            bodyState = BodySubState.SETTING;
            StartCoroutine(SetRandomColors());
        }
    }

    private IEnumerator SetRandomColors()
    {
        float duration = Random.Range(bb.bodyColorsCarrouselMinTime, bb.bodyColorsCarrouselMaxTime);

        float elapsedTime = 0;

        while(elapsedTime < duration)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(ChromaColorInfo.Random));

            yield return new WaitForSeconds(bb.bodyColorsCarrouselChangeInterval);
            elapsedTime += bb.bodyColorsCarrouselChangeInterval;
        }

        SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(color));
        bodyState = BodySubState.NORMAL;
    }

    public void Disable()
    {
        if (bodyState != BodySubState.NORMAL) return;

        SetMaterial(rsc.coloredObjectsMng.GetWormBodyDimMaterial(color));
        bodyState = BodySubState.NORMAL_DISABLED;
    }


    public void Explode()
    {
        StartCoroutine(RandomizeAndExplode());
    }

    private IEnumerator RandomizeAndExplode()
    {
        float elapsedTime = 0;
        ChromaColor randomColor = ChromaColorInfo.Random;

        while (elapsedTime < bb.bodyColorsCarrouselMinTime)
        {
            randomColor = ChromaColorInfo.Random;
            SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(randomColor));

            yield return new WaitForSeconds(bb.bodyColorsCarrouselChangeInterval);
            elapsedTime += bb.bodyColorsCarrouselChangeInterval;
        }

        bodyDeactivate[(int)randomColor].Play();
        voxelization.SpawnFakeVoxels();

        EnemyExplosionController explosion = rsc.poolMng.enemyExplosionPool.GetObject();

        if (explosion != null)
        {
            explosion.transform.position = transform.position;
            explosion.PlayAll(finalExplosionSoundFx);
        }

        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public void ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (bodyState != BodySubState.NORMAL) return;

        blinkController.BlinkWhiteOnce();

        if (shotColor != color && !rsc.debugMng.alwaysKillOk)
        {
            currentDamageWrongColor += damage * bb.HealthSettingsPhase.bodyWrongColorDamageModifier;

            if(currentDamageWrongColor >= bb.HealthSettingsPhase.bodyMaxHealth)
            {
                bodyState = BodySubState.DEACTIVATED; //not really deactivated but flagged to allow notify properly when colors reset

                explosionWrongSoundFx.Play();
                rsc.rumbleMng.Rumble(0, 0.25f, 0f, 0.5f);

                EnemyDiedEventInfo.eventInfo.color = color;
                EnemyDiedEventInfo.eventInfo.infectionValue = 0;
                EnemyDiedEventInfo.eventInfo.killerPlayer = player;
                EnemyDiedEventInfo.eventInfo.killedSameColor = (color == shotColor);
                EnemyDiedEventInfo.eventInfo.specialKill = false;
                rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDiedEventInfo.eventInfo);

                player.ColorMismatch();

                head.DischargeHead();
            }
        }
        else
        {
            currentDamage += damage;

            if (currentDamage >= bb.HealthSettingsPhase.bodyMaxHealth)
            {
                //Set material grey
                SetMaterial(rsc.coloredObjectsMng.GetWormBodyGreyMaterial());
                bodyDeactivate[(int)color].Play();
                bodyState = BodySubState.DEACTIVATED;

                explosionOkSoundFx.Play();
                rsc.rumbleMng.Rumble(0, 0.25f, 0f, 0.5f);

                EnemyDiedEventInfo.eventInfo.color = color;
                EnemyDiedEventInfo.eventInfo.infectionValue = 0;
                EnemyDiedEventInfo.eventInfo.killerPlayer = player;
                EnemyDiedEventInfo.eventInfo.killedSameColor = (color == shotColor);
                EnemyDiedEventInfo.eventInfo.specialKill = false;
                rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDiedEventInfo.eventInfo);

                head.ChargeHead();
            }
        }
    }

    private void SetMaterial(Material material)
    {
        Material[] mats = rend.sharedMaterials;

        if (mats[1] != material)
        {
            mats[1] = material;
            rend.sharedMaterials = mats;

            blinkController.InvalidateMaterials();
        }
    }

    private void SetMaterial(Material[] materials)
    {
        Material[] mats = rend.sharedMaterials;

        if (mats[0] != materials[0])
        {
            mats[0] = materials[0];
            rend.sharedMaterials = mats;

            blinkController.InvalidateMaterials();
        }

        if (mats[1] != materials[1])
        {
            mats[1] = materials[1];
            rend.sharedMaterials = mats;

            blinkController.InvalidateMaterials();
        }
    }

    public void SetVisible(bool visible)
    {
        rend.enabled = visible;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            head.PlayerTouched(player, transform.position);
        }
        else if (other.tag == "EnemyHexagonBodyProbe")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponent<EnemyBaseAIBehaviour>();

            if (enemy == null)
                enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();

            if (enemy != null)
                enemy.InstantKill();
        }
    }
}
