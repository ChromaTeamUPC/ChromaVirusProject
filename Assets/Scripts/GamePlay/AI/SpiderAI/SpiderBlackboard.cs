using UnityEngine;
using System.Collections;

public class SpiderBlackboard: EnemyBaseBlackboard
{
    public SpiderAIBehaviour spider;

    public float timeSinceLastAttack;
    public int currentHealth;

    public SpiderAIBehaviour.SpawnAnimation spawnAnimation; 

    public override void InitialSetup(GameObject e)
    {
        base.InitialSetup(e);

        spider = entityGO.GetComponent<SpiderAIBehaviour>();

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
