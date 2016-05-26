using UnityEngine;
using System.Collections;

public class SpiderBlackboard: EnemyBaseBlackboard
{
    public SpiderAIBehaviour spider;

    public float timeSinceLastAttack;
    public int currentHealth;

    public Transform boltSpawnPoint;

    public SpiderAIBehaviour.SpawnAnimation spawnAnimation;

    public GameObject[] explosions = new GameObject[4];

    public override void InitialSetup(GameObject e)
    {
        base.InitialSetup(e);

        spider = entityGO.GetComponent<SpiderAIBehaviour>();
        boltSpawnPoint = entityGO.transform.Find("BoltSpawnPoint");

        for(int i = 0; i < 4; ++i)
        {
            explosions[i] = GameObject.Instantiate(spider.explosionPrefabs[i], spider.transform.position, spider.transform.rotation) as GameObject;
            explosions[i].transform.parent = spider.transform;
            explosions[i].SetActive(false);
        }

        ResetValues();
    }

    public override void ResetValues()
    {
        base.ResetValues();

        timeSinceLastAttack = 100f;
        currentHealth = spider.maxHealth;
        spawnAnimation = SpiderAIBehaviour.SpawnAnimation.FLOOR;   
    }
}
