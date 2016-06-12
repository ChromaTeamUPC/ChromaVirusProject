using UnityEngine;
using System.Collections;
using System;

[Serializable] public class PlayerShotPool : MonoBehaviourObjectPool<PlayerShotController> { }
[Serializable] public class PlayerMuzzlePool : MonoBehaviourObjectPool<MuzzleController> { }
[Serializable] public class VoxelPool : MonoBehaviourObjectPool<VoxelController> { }
[Serializable] public class EnergyVoxelPool : MonoBehaviourObjectPool<EnergyVoxelController> { }
[Serializable] public class SpiderPool : MonoBehaviourObjectPool<SpiderAIBehaviour> { }
[Serializable] public class SpiderBoltPool : MonoBehaviourObjectPool<SpiderBolt> { }
[Serializable] public class MosquitoPool : MonoBehaviourObjectPool<MosquitoAIBehaviour> { }
[Serializable] public class MosquitoWeakShotPool : MonoBehaviourObjectPool<MosquitoWeakShotController> { }

//Just a container for the different pools we have in the game
public class PoolManager : MonoBehaviour
{
    public GameObject poolContainerPrefab;

    public PlayerShotPool player1ShotRedPool = new PlayerShotPool();
    public PlayerShotPool player1ShotGreenPool = new PlayerShotPool();
    public PlayerShotPool player1ShotBluePool = new PlayerShotPool();
    public PlayerShotPool player1ShotYellowPool = new PlayerShotPool();

    public PlayerMuzzlePool player1MuzzleRedPool = new PlayerMuzzlePool();
    public PlayerMuzzlePool player1MuzzleGreenPool = new PlayerMuzzlePool();
    public PlayerMuzzlePool player1MuzzleBluePool = new PlayerMuzzlePool();
    public PlayerMuzzlePool player1MuzzleYellowPool = new PlayerMuzzlePool();

    public VoxelPool voxelPool = new VoxelPool();
    public EnergyVoxelPool energyVoxelPool = new EnergyVoxelPool();
    public ObjectPool voxelColliderPool = new ObjectPool();

    public SpiderPool spiderPool = new SpiderPool();
    public SpiderBoltPool spiderBoltPool = new SpiderBoltPool();

    public MosquitoPool mosquitoPool = new MosquitoPool();
    public MosquitoWeakShotPool mosquitoWeakShotRedPool = new MosquitoWeakShotPool();
    public MosquitoWeakShotPool mosquitoWeakShotGreenPool = new MosquitoWeakShotPool();
    public MosquitoWeakShotPool mosquitoWeakShotBluePool = new MosquitoWeakShotPool();
    public MosquitoWeakShotPool mosquitoWeakShotYellowPool = new MosquitoWeakShotPool();

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

        voxelPool.Init(gameObject, poolContainerPrefab);
        energyVoxelPool.Init(gameObject, poolContainerPrefab);
        voxelColliderPool.Init(gameObject, poolContainerPrefab);

        spiderPool.Init(gameObject, poolContainerPrefab);
        spiderBoltPool.Init(gameObject, poolContainerPrefab);

        mosquitoPool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotRedPool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotGreenPool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotBluePool.Init(gameObject, poolContainerPrefab);
        mosquitoWeakShotYellowPool.Init(gameObject, poolContainerPrefab);

        Debug.Log("Pool Manager created");
    }

    void OnDestroy()
    {
        Debug.Log("Pool Manager destroyed");
    }
}
