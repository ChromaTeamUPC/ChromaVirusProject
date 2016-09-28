using UnityEngine;
using System.Collections;
using System;

[Serializable] public class PlayerShotPool : MonoBehaviourObjectPool<PlayerShotController> { }
[Serializable] public class PlayerSpecialAttackPool : MonoBehaviourObjectPool<SpecialAttackController> { }
[Serializable] public class MuzzlePool : MonoBehaviourObjectPool<MuzzleController> { }
[Serializable] public class VoxelPool : MonoBehaviourObjectPool<VoxelController> { }
[Serializable] public class EnergyVoxelPool : MonoBehaviourObjectPool<EnergyVoxelController> { }
[Serializable] public class SpiderPool : MonoBehaviourObjectPool<SpiderAIBehaviour> { }
[Serializable] public class SpiderBoltPool : MonoBehaviourObjectPool<SpiderBolt> { }
[Serializable] public class SpiderSparksPool : MonoBehaviourObjectPool<SpiderSparks> { }
[Serializable] public class MosquitoPool : MonoBehaviourObjectPool<MosquitoAIBehaviour> { }
[Serializable] public class MosquitoWeakShotPool : MonoBehaviourObjectPool<MosquitoWeakShotController> { }
[Serializable] public class MosquitoMainAttackPool : MonoBehaviourObjectPool<MosquitoMainAttackControllerBase> { }
[Serializable] public class EnemyExplosionPool : MonoBehaviourObjectPool<EnemyExplosionController> { }
[Serializable] public class MeteorPool : MonoBehaviourObjectPool<MeteorController> { }
[Serializable] public class ComboIncrementPool : MonoBehaviourObjectPool<ChainIncrementController> { }


//Just a container for the different pools we have in the game
public class PoolManager : MonoBehaviour
{
    public GameObject poolContainerPrefab;

    public PlayerShotPool player1ShotRedPool = new PlayerShotPool();
    public PlayerShotPool player1ShotGreenPool = new PlayerShotPool();
    public PlayerShotPool player1ShotBluePool = new PlayerShotPool();
    public PlayerShotPool player1ShotYellowPool = new PlayerShotPool();

    public PlayerShotPool player2ShotRedPool = new PlayerShotPool();
    public PlayerShotPool player2ShotGreenPool = new PlayerShotPool();
    public PlayerShotPool player2ShotBluePool = new PlayerShotPool();
    public PlayerShotPool player2ShotYellowPool = new PlayerShotPool();

    public MuzzlePool playerMuzzleRedPool = new MuzzlePool();
    public MuzzlePool playerMuzzleGreenPool = new MuzzlePool();
    public MuzzlePool playerMuzzleBluePool = new MuzzlePool();
    public MuzzlePool playerMuzzleYellowPool = new MuzzlePool();

    public PlayerSpecialAttackPool player1SpecialAttackPool = new PlayerSpecialAttackPool();

    public VoxelPool voxelPool = new VoxelPool();
    public EnergyVoxelPool energyVoxelPool = new EnergyVoxelPool();
    public EnergyVoxelPool bigEnergyVoxelPool = new EnergyVoxelPool();
    public ObjectPool voxelColliderPool = new ObjectPool();

    public SpiderPool spiderPool = new SpiderPool();
    public SpiderBoltPool spiderBoltPool = new SpiderBoltPool();
    public SpiderSparksPool spiderSparksPool = new SpiderSparksPool();

    public MosquitoPool mosquitoPool = new MosquitoPool();
    public MosquitoWeakShotPool mosquitoWeakShotRedPool = new MosquitoWeakShotPool();
    public MosquitoWeakShotPool mosquitoWeakShotGreenPool = new MosquitoWeakShotPool();
    public MosquitoWeakShotPool mosquitoWeakShotBluePool = new MosquitoWeakShotPool();
    public MosquitoWeakShotPool mosquitoWeakShotYellowPool = new MosquitoWeakShotPool();
    public MosquitoMainAttackPool mosquitoSingleProjectilePool = new MosquitoMainAttackPool();
    public MosquitoMainAttackPool mosquitoFanProjectilePool = new MosquitoMainAttackPool();
    public MosquitoMainAttackPool mosquitoMultipleProjectilePool = new MosquitoMainAttackPool();
    public MosquitoMainAttackPool mosquitoHomingProjectilePool = new MosquitoMainAttackPool();

    public EnemyExplosionPool enemyExplosionPool = new EnemyExplosionPool();

    public MuzzlePool turretMuzzleRedPool = new MuzzlePool();
    public MuzzlePool turretMuzzleGreenPool = new MuzzlePool();
    public MuzzlePool turretMuzzleBluePool = new MuzzlePool();
    public MuzzlePool turretMuzzleYellowPool = new MuzzlePool();

    public MeteorPool meteorPool = new MeteorPool();

    public ComboIncrementPool comboIncrementPool = new ComboIncrementPool();

    void Start()
    {
        player1ShotRedPool.Init(gameObject, poolContainerPrefab);
        player1ShotGreenPool.Init(gameObject, poolContainerPrefab);
        player1ShotBluePool.Init(gameObject, poolContainerPrefab);
        player1ShotYellowPool.Init(gameObject, poolContainerPrefab);

        player2ShotRedPool.Init(gameObject, poolContainerPrefab);
        player2ShotGreenPool.Init(gameObject, poolContainerPrefab);
        player2ShotBluePool.Init(gameObject, poolContainerPrefab);
        player2ShotYellowPool.Init(gameObject, poolContainerPrefab);

        playerMuzzleRedPool.Init(gameObject, poolContainerPrefab);
        playerMuzzleGreenPool.Init(gameObject, poolContainerPrefab);
        playerMuzzleBluePool.Init(gameObject, poolContainerPrefab);
        playerMuzzleYellowPool.Init(gameObject, poolContainerPrefab);

        player1SpecialAttackPool.Init(gameObject, poolContainerPrefab);

        voxelPool.Init(gameObject, poolContainerPrefab);
        energyVoxelPool.Init(gameObject, poolContainerPrefab);
        bigEnergyVoxelPool.Init(gameObject, poolContainerPrefab);
        voxelColliderPool.Init(gameObject, poolContainerPrefab);

        spiderPool.Init(gameObject, poolContainerPrefab);
        spiderBoltPool.Init(gameObject, poolContainerPrefab);
        spiderSparksPool.Init(gameObject, poolContainerPrefab);

        mosquitoPool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotRedPool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotGreenPool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotBluePool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotYellowPool.Init(gameObject, poolContainerPrefab);
        mosquitoSingleProjectilePool.Init(gameObject, poolContainerPrefab);
        mosquitoFanProjectilePool.Init(gameObject, poolContainerPrefab);
        mosquitoMultipleProjectilePool.Init(gameObject, poolContainerPrefab);
        mosquitoHomingProjectilePool.Init(gameObject, poolContainerPrefab);

        enemyExplosionPool.Init(gameObject, poolContainerPrefab);

        turretMuzzleRedPool.Init(gameObject, poolContainerPrefab);
        turretMuzzleGreenPool.Init(gameObject, poolContainerPrefab);
        turretMuzzleBluePool.Init(gameObject, poolContainerPrefab);
        turretMuzzleYellowPool.Init(gameObject, poolContainerPrefab);

        meteorPool.Init(gameObject, poolContainerPrefab);

        comboIncrementPool.Init(gameObject, poolContainerPrefab);
        //Debug.Log("Pool Manager created");
    }

    void OnDestroy()
    {
        //Debug.Log("Pool Manager destroyed");
    }
}
