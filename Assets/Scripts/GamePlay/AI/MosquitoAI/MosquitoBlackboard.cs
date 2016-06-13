using UnityEngine;
using System.Collections;

public class MosquitoBlackboard : EnemyBaseBlackboard
{
    public MosquitoAIBehaviour mosquito;

    public float checkPlayerElapsedTime;

    public float timeSinceLastAttack;

    public float initialCheckDelay;
    public float checkAttackingMosquitoesDelay;

    public Transform shotSpawnPoint;

    public MosquitoAIBehaviour.SpawnAnimation spawnAnimation;

    public GameObject[] explosions = new GameObject[4];

    public MosquitoAIBaseState spawningState;
    public MosquitoAIActionsBaseState patrolingState;
    public MosquitoAIActionsBaseState attackingPlayerState;

    public MosquitoAIBaseState dyingState;

    public override void InitialSetup(GameObject e)
    {
        base.InitialSetup(e);

        mosquito = entityGO.GetComponent<MosquitoAIBehaviour>();
        shotSpawnPoint = entityGO.transform.FindDeepChild("ShotSpawnPoint");

        for (int i = 0; i < 4; ++i)
        {
            explosions[i] = GameObject.Instantiate(mosquito.explosionPrefabs[i], mosquito.transform.position, mosquito.transform.rotation) as GameObject;
            explosions[i].transform.parent = mosquito.transform;
            explosions[i].SetActive(false);
        }

        spawningState = new MosquitoSpawningAIState(this);
        patrolingState = new MosquitoPatrolingAIState(this);
        attackingPlayerState = new MosquitoAttackingPlayerAIState(this);
        dyingState = new MosquitoDyingAIState(this);

        ResetValues();
    }

    public override void ResetValues()
    {
        base.ResetValues();

        checkPlayerElapsedTime = 0f;
        timeSinceLastAttack = 100f;

        initialCheckDelay = 0f;
        checkAttackingMosquitoesDelay = 0f;
    }
}
