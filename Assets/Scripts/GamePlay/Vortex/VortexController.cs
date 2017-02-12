using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VortexController : MonoBehaviour
{
    public const int infectionValue = 5;

    public int maxHealth = 100;

    public float spawnMinDelay = 2;
    public float spawnMaxDelay = 5;

    public int maxEnemiesInScene = 10;

    public Transform modelAndFx;
    public Transform spawnPoint;
    public ParticleSystem particleSys;

    private bool active;
    private bool zoneWavesFinished;
    private int currentHealth;

    private float spawnDelay;
    private float elapsedTime;

    public float blinkSeconds = 0.1f;

    public bool translateVert = true;
    public float translateVertMinDisplacement = 0.5f;
    public float translateVertMaxDisplacement = 1.5f;
    public float translateVertFullCycleMinTime = 7f;
    public float translateVertFullCycleMaxTime = 10f;
    private float transVertHalfDisplacement;
    private float translateVertFullCycleTime;
    private float transVertOriginalY;
    private float translateElapsedTime;

    private PlayerController killerPlayer;

    private ColoredObjectsManager coloredObjMng;

    private List<AIAction> entryActions;
    private List<AIAction> attackActions;
    private List<AIAction> infectActions;

    private BlinkController blinkController;
    private Animator anim;
    private AudioSource audioSource;

    public bool Active { get { return active; } }

    void Awake()
    {
        blinkController = GetComponent<BlinkController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        active = false;
        coloredObjMng = rsc.coloredObjectsMng;
        entryActions = rsc.enemyMng.defaultSpiderEntry;
        attackActions = rsc.enemyMng.defaultSpiderAttack;
        infectActions = rsc.enemyMng.defaultSpiderInfect;
    }

    public void Activate()
    {
        zoneWavesFinished = false;
        rsc.eventMng.StartListening(EventManager.EventType.ZONE_WAVES_FINISHED, ZoneWavesFinished);
        active = true;
        particleSys.Play();
        currentHealth = maxHealth;
        spawnDelay = 0.01f;
        elapsedTime = 0f;

        transVertHalfDisplacement = Random.Range(translateVertMinDisplacement, translateVertMaxDisplacement) / 2;
        translateVertFullCycleTime = Random.Range(translateVertFullCycleMinTime, translateVertFullCycleMaxTime);
        transVertOriginalY = modelAndFx.position.y;
        translateElapsedTime = 0f;

        rsc.eventMng.TriggerEvent(EventManager.EventType.VORTEX_ACTIVATED, EventInfo.emptyInfo);
    }

    public void Deactivate()
    {
        rsc.eventMng.StopListening(EventManager.EventType.ZONE_WAVES_FINISHED, ZoneWavesFinished);
        active = false;
        audioSource.Play();
        particleSys.Stop();
        rsc.rumbleMng.Rumble(killerPlayer.Id, 0.5f, 0f, 0.75f);
        anim.SetTrigger("Destroyed");
        rsc.eventMng.TriggerEvent(EventManager.EventType.VORTEX_DESTROYED, EventInfo.emptyInfo);
    }

    private void ZoneWavesFinished(EventInfo eventInfo)
    {
        //zoneWavesFinished = true;
    }

    public void ImpactedByShot(ChromaColor shotColor, int damage, PlayerController player)
    {
        if (!active) return;

        blinkController.BlinkWhiteOnce();

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            killerPlayer = player;
            //Play destroy animation, then destroy object
            Deactivate();
        }
    }

    public void ImpactedBySpecial(PlayerController player)
    {
        if (!active) return;

        blinkController.BlinkWhiteOnce();

        currentHealth -= maxHealth / 2;

        if (currentHealth <= 0)
        {
            killerPlayer = player;
            //Play destroy animation, then destroy object
            Deactivate();
        }
    }

    // Update is called once per frame
    void Update ()
    {
	    if(active)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > spawnDelay && rsc.enemyMng.bb.activeEnemies < maxEnemiesInScene && !zoneWavesFinished)
            {
                elapsedTime = 0f;

                SpiderAIBehaviour enemy = coloredObjMng.GetSpider(ChromaColorInfo.Random, spawnPoint.position);

                if (enemy != null)
                {
                    enemy.AIInit(SpiderAIBehaviour.SpawnAnimation.VORTEX, entryActions, attackActions, infectActions);
                    enemy.Spawn(spawnPoint);
                    rsc.enemyMng.AddVortexEnemyInfection(SpiderAIBehaviour.infectionValue);
                }

                spawnDelay = Random.Range(spawnMinDelay, spawnMaxDelay);
            }

            if (translateVert)
            {
                translateElapsedTime += Time.deltaTime;

                float factor = Mathf.Sin(translateElapsedTime / translateVertFullCycleTime * Mathf.PI * 2);
                float newRtVertPos = transVertOriginalY + (transVertHalfDisplacement * factor);
                modelAndFx.position = new Vector3(modelAndFx.position.x, newRtVertPos, modelAndFx.position.z);
            }
        }
	}
}
