using UnityEngine;
using System.Collections;

public class WormBodySegmentController : MonoBehaviour 
{
    private enum State
    {
        SETTING,
        NORMAL,
        NORMAL_DISABLED,
        DEACTIVATED,
        DESTROYED
    }

    private State state;
    private float currentHealth;
    private float currentHealthWrongColor;

    [SerializeField]
    private ChromaColor color;

    [Header("Fx")]
    public GameObject[] bodyDeactivatePrefabs;
    public GameObject bodyDestructionPrefab;

    private ParticleSystem[] bodyDeactivate;
    private ParticleSystem bodyDestruction;

    private BlinkController blinkController;
    private Renderer rend;
    private BoxCollider col;
    private VoxelizationClient voxelization;

    private WormBlackboard bb;
    private WormAIBehaviour head; //Shortcut

    void Awake()
    {
        blinkController = GetComponent<BlinkController>();
        rend = GetComponentInChildren<Renderer>();
        col = GetComponent<BoxCollider>();
        voxelization = GetComponentInChildren<VoxelizationClient>();
        state = State.NORMAL;

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
    }

    void Start()
    {
        SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(color));
    }

    public void SetBlackboard(WormBlackboard bb)
    {
        this.bb = bb;
        head = bb.head;
        currentHealth = bb.bodyMaxHealth;
        currentHealthWrongColor = bb.bodyMaxHealth;
    }

    public void SetInitialState(ChromaColor c)
    {
        color = c;
        currentHealth = bb.bodyMaxHealth;
        currentHealthWrongColor = bb.bodyMaxHealth;
        SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(color));

        ColorEventInfo.eventInfo.newColor = color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_ACTIVATED, ColorEventInfo.eventInfo);

        state = State.NORMAL;
    }

    public bool IsDestroyed()
    {
        return state == State.DESTROYED;
    }

    public void ResetColor(ChromaColor color)
    {
        if (state == State.DESTROYED) return;

        ChromaColor oldColor = this.color;
        if(state == State.DEACTIVATED)
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
        currentHealth = bb.bodyMaxHealth;
        currentHealthWrongColor = bb.bodyMaxHealth;
        state = State.SETTING;

        StartCoroutine(SetRandomColors());
    }

    public void Consolidate(ChromaColor color)
    {
        if (state == State.DESTROYED) return;

        if (state == State.DEACTIVATED)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormBodyWireframeMaterial());
            bodyDestruction.Play();
            state = State.DESTROYED;
            col.enabled = false;
        }
        else
        {
            ChromaColor oldColor = this.color;
            this.color = color;
            currentHealth = bb.bodyMaxHealth;
            currentHealthWrongColor = bb.bodyMaxHealth;

            ColorEventInfo.eventInfo.oldColor = oldColor;
            ColorEventInfo.eventInfo.newColor = color;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_COLOR_CHANGED, ColorEventInfo.eventInfo);

            state = State.SETTING;
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
        state = State.NORMAL;
    }

    public void Disable()
    {
        if (state != State.NORMAL) return;

        SetMaterial(rsc.coloredObjectsMng.GetWormBodyDimMaterial(color));
        state = State.NORMAL_DISABLED;
    }


    public void Explode()
    {
        StartCoroutine(RandomizeAndExplode());
    }

    private IEnumerator RandomizeAndExplode()
    {
        float elapsedTime = 0;

        while (elapsedTime < bb.bodyColorsCarrouselMinTime)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormBodyMaterial(ChromaColorInfo.Random));

            yield return new WaitForSeconds(bb.bodyColorsCarrouselChangeInterval);
            elapsedTime += bb.bodyColorsCarrouselChangeInterval;
        }

        voxelization.SpawnFakeVoxels();
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public void ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (state != State.NORMAL) return;

        blinkController.BlinkWhiteOnce();

        if (shotColor != color)
        {
            currentHealthWrongColor -= damage * bb.bodyWrongColorDamageModifier;

            if(currentHealthWrongColor <= 0)
            {
                state = State.DEACTIVATED; //not really deactivated but flagged to allow notify properly when colors reset

                EnemyDiedEventInfo.eventInfo.color = color;
                EnemyDiedEventInfo.eventInfo.infectionValue = 0;
                EnemyDiedEventInfo.eventInfo.killerPlayer = player;
                EnemyDiedEventInfo.eventInfo.killedSameColor = (color == shotColor);
                rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDiedEventInfo.eventInfo);

                head.DischargeHead();
            }
        }
        else
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                //Set material grey
                SetMaterial(rsc.coloredObjectsMng.GetWormBodyGreyMaterial());
                bodyDeactivate[(int)color].Play();
                state = State.DEACTIVATED;

                //Explosion FX?

                EnemyDiedEventInfo.eventInfo.color = color;
                EnemyDiedEventInfo.eventInfo.infectionValue = 0;
                EnemyDiedEventInfo.eventInfo.killerPlayer = player;
                EnemyDiedEventInfo.eventInfo.killedSameColor = (color == shotColor);
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
    }
}
