using UnityEngine;
using System.Collections;

public class SpawnerController : MonoBehaviour {

    public int maxHealth;

    public float spawnMinDelay;
    public float spawnMaxDelay;

    private ColoredObjectsManager coloredObjMng;

    private bool active;
    private int currentHealth;

    private float spawnDelay;
    private float elapsedTime;

    public Transform spawnPoint;

    void Start()
    {
        active = false;
        coloredObjMng = rsc.coloredObjectsMng;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(active)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > spawnDelay)
            {
                elapsedTime -= spawnDelay;

                SpiderAIBehaviour enemy = coloredObjMng.GetSpider(ChromaColorInfo.Random);

                if (enemy != null)
                {
                    enemy.Spawn(spawnPoint);
                }

                spawnDelay = Random.Range(spawnMinDelay, spawnMaxDelay);
            }
        }
	}

    public void Activate()
    {
        active = true;
        currentHealth = maxHealth;
        spawnDelay = Random.Range(spawnMinDelay, spawnMaxDelay);
        elapsedTime = 0f;
        rsc.eventMng.TriggerEvent(EventManager.EventType.SPAWNER_ACTIVATED, EventInfo.emptyInfo);
    }

    public void Deactivate()
    {
        active = false;
        rsc.eventMng.TriggerEvent(EventManager.EventType.SPAWNER_DESTROYED, EventInfo.emptyInfo);
        Destroy(this, 2f);
    }


    public void ImpactedByShot(ChromaColor shotColor, int damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        if (!active) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            //Play destroy animation, then destroy object
            Deactivate();
        }
    }
}
