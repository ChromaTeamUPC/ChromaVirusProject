using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VortexController : MonoBehaviour {

    public int maxHealth = 100;

    public float spawnMinDelay = 2;
    public float spawnMaxDelay = 5;

    public int maxEnemiesInScene = 10;

    public Transform spawnPoint;
    public ParticleSystem particleSystem;

    private bool active;
    private int currentHealth;

    private float spawnDelay;
    private float elapsedTime;

    public float blinkSeconds = 0.1f;

    private ColoredObjectsManager coloredObjMng;

    private List<AIAction> entryActions;
    private List<AIAction> attackActions;

    private BlinkController blinkController;

    void Awake()
    {
        blinkController = GetComponent<BlinkController>();
    }

    void Start()
    {
        active = false;
        coloredObjMng = rsc.coloredObjectsMng;
        entryActions = rsc.enemyMng.defaultSpiderEntry;
        attackActions = rsc.enemyMng.defaulSpiderAttack;
    }

    public void Activate()
    {
        active = true;
        particleSystem.Play();
        currentHealth = maxHealth;
        spawnDelay = 0.01f;
        elapsedTime = 0f;
        rsc.eventMng.TriggerEvent(EventManager.EventType.VORTEX_ACTIVATED, EventInfo.emptyInfo);
    }

    public void Deactivate()
    {
        active = false;
        particleSystem.Stop();
        rsc.eventMng.TriggerEvent(EventManager.EventType.VORTEX_DESTROYED, EventInfo.emptyInfo);
        Destroy(gameObject, 3f);
    }

    public void ImpactedByShot(ChromaColor shotColor, int damage)
    {
        if (!active) return;

        blinkController.Blink();

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
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
            if (elapsedTime > spawnDelay && rsc.enemyMng.blackboard.activeEnemies < maxEnemiesInScene)
            {
                elapsedTime = 0f;

                SpiderAIBehaviour enemy = coloredObjMng.GetSpider(ChromaColorInfo.Random);

                if (enemy != null)
                {
                    //TODO, change spawn animation to vortex specific one when we have it
                    enemy.AIInit(SpiderAIBehaviour.SpawnAnimation.FLOOR, entryActions, attackActions);
                    enemy.Spawn(spawnPoint);
                }

                spawnDelay = Random.Range(spawnMinDelay, spawnMaxDelay);
            }
        }
	}
}
