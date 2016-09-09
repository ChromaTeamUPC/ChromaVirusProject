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
[Serializable] public class MosquitoPool : MonoBehaviourObjectPool<MosquitoAIBehaviour> { }
[Serializable] public class MosquitoWeakShotPool : MonoBehaviourObjectPool<MosquitoWeakShotController> { }
[Serializable] public class MosquitoMainAttackPool : MonoBehaviourObjectPool<MosquitoMainAttackControllerBase> { }
[Serializable] public class EnemyExplosionPool : MonoBehaviourObjectPool<EnemyExplosionController> { }


//Just a container for the different pools we have in the game
public class PoolManager : MonoBehaviour
{
    public GameObject poolContainerPrefab;

    public PlayerShotPool player1ShotRedPool = new PlayerShotPool();
    public PlayerShotPool player1ShotGreenPool = new PlayerShotPool();
    public PlayerShotPool player1ShotBluePool = new PlayerShotPool();
    public PlayerShotPool player1ShotYellowPool = new PlayerShotPool();

    public MuzzlePool player1MuzzleRedPool = new MuzzlePool();
    public MuzzlePool player1MuzzleGreenPool = new MuzzlePool();
    public MuzzlePool player1MuzzleBluePool = new MuzzlePool();
    public MuzzlePool player1MuzzleYellowPool = new MuzzlePool();

    public PlayerSpecialAttackPool player1SpecialAttackPool = new PlayerSpecialAttackPool();

    public VoxelPool voxelPool = new VoxelPool();
    public EnergyVoxelPool energyVoxelPool = new EnergyVoxelPool();
    public EnergyVoxelPool bigEnergyVoxelPool = new EnergyVoxelPool();
    public ObjectPool voxelColliderPool = new ObjectPool();

    public SpiderPool spiderPool = new SpiderPool();
    public SpiderBoltPool spiderBoltPool = new SpiderBoltPool();

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

    void Start()
    {
        player1ShotRedPool.Init(gameObject, poolContainerPrefab);
        player1ShotGreenPool.Init(gameObject, poolContainerPrefab);
        player1ShotBluePool.Init(gameObject, poolContainerPrefab);
        player1ShotYellowPool.Init(gameObject, poolContainerPrefab);

        player1MuzzleRedPool.Init(gameObject, poolContainerPrefab);
        player1MuzzleGreenPool.Init(gameObject, poolContainerPrefab);
        player1MuzzleBluePool.Init(gameObject, poolContainerPrefab);
        player1MuzzleYellowPool.Init(gameObject, poolContainerPrefab);

        player1SpecialAttackPool.Init(gameObject, poolContainerPrefab);

        voxelPool.Init(gameObject, poolContainerPrefab);
        energyVoxelPool.Init(gameObject, poolContainerPrefab);
        bigEnergyVoxelPool.Init(gameObject, poolContainerPrefab);
        voxelColliderPool.Init(gameObject, poolContainerPrefab);

        spiderPool.Init(gameObject, poolContainerPrefab);
        spiderBoltPool.Init(gameObject, poolContainerPrefab);

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

        //Debug.Log("Pool Manager created");
    }

    void OnDestroy()
    {
        //Debug.Log("Pool Manager destroyed");
    }
}
