using UnityEngine;
using System.Collections;

public class SpawnerController : MonoBehaviour {

    public int maxHealth;

    public bool spawnSpiders;
    public bool spawnMosquitos;

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

                /*GameObject enemy = coloredObjMng.GetSpider(ChromaColorInfo.Random);

                if (enemy != null)
                {
                    enemy.transform.position = spawnPoint.position;
                    enemy.transform.rotation = spawnPoint.rotation;
                    enemy.SetActive(true);
                }*/

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
    }


    public void ImpactedByShot(ChromaColor shotColor, int damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Deactivate();
        }
    }
}
