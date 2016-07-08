using UnityEngine;
using System.Collections;

public class MosquitoBlackboard : EnemyBaseBlackboard
{
    //Non reseteable values
    public MosquitoAIBehaviour mosquito;

    public Transform shotSpawnPoint;

    public MosquitoAIBaseState spawningState;
    public MosquitoAIActionsBaseState patrolingState;
    public MosquitoAIActionsBaseState attackingPlayerState;
    public MosquitoAIBaseState attractedToBarrelState;
    public MosquitoAIBaseState dyingState;

    //Reseteable values
    public float checkPlayerElapsedTime;

    public float timeSinceLastAttack;

    public float initialCheckDelay;
    public float checkAttackingMosquitoesDelay;

    public MosquitoAIBehaviour.SpawnAnimation spawnAnimation;

    public override void InitialSetup(GameObject e)
    {
        base.InitialSetup(e);

        mosquito = entityGO.GetComponent<MosquitoAIBehaviour>();
        shotSpawnPoint = entityGO.transform.FindDeepChild("ShotSpawnPoint");

        spawningState = new MosquitoSpawningAIState(this);
        patrolingState = new MosquitoPatrolingAIState(this);
        attackingPlayerState = new MosquitoAttackingPlayerAIState(this);
        attractedToBarrelState = new MosquitoAttractedToBarrelAIState(this);
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
