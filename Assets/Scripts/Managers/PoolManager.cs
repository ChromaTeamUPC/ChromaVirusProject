using UnityEngine;
using System.Collections;

//Just a container for the different pools we have in the game
public class PoolManager : MonoBehaviour
{   
    [SerializeField]
    private ScriptObjectPoolDefiner player1ShotRedPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> player1ShotRedPool;

    [SerializeField]
    private ScriptObjectPoolDefiner player1ShotGreenPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> player1ShotGreenPool;

    [SerializeField]
    private ScriptObjectPoolDefiner player1ShotBluePoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> player1ShotBluePool;

    [SerializeField]
    private ScriptObjectPoolDefiner player1ShotYellowPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> player1ShotYellowPool;

    [SerializeField]
    private ScriptObjectPoolDefiner player1MuzzleRedPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<MuzzleController> player1MuzzleRedPool;

    [SerializeField]
    private ScriptObjectPoolDefiner player1MuzzleGreenPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<MuzzleController> player1MuzzleGreenPool;

    [SerializeField]
    private ScriptObjectPoolDefiner player1MuzzleBluePoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<MuzzleController> player1MuzzleBluePool;

    [SerializeField]
    private ScriptObjectPoolDefiner player1MuzzleYellowPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<MuzzleController> player1MuzzleYellowPool;

    public ObjectPool spiderPool;

    [SerializeField]
    private ScriptObjectPoolDefiner spiderBoltPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<SpiderBolt> spiderBoltPool;

    [SerializeField]
    private ScriptObjectPoolDefiner voxelPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<VoxelController> voxelPool;
    public ObjectPool voxelColliderPool;

    void Awake()
    {
        player1ShotRedPool = CreateScriptObjectPool<PlayerShotController>(player1ShotRedPool, player1ShotRedPoolDefiner);
        player1ShotGreenPool = CreateScriptObjectPool<PlayerShotController>(player1ShotGreenPool, player1ShotGreenPoolDefiner);
        player1ShotBluePool = CreateScriptObjectPool<PlayerShotController>(player1ShotBluePool, player1ShotBluePoolDefiner);
        player1ShotYellowPool = CreateScriptObjectPool<PlayerShotController>(player1ShotYellowPool, player1ShotYellowPoolDefiner);

        player1MuzzleRedPool = CreateScriptObjectPool<MuzzleController>(player1MuzzleRedPool, player1MuzzleRedPoolDefiner);
        player1MuzzleGreenPool = CreateScriptObjectPool<MuzzleController>(player1MuzzleGreenPool, player1MuzzleGreenPoolDefiner);
        player1MuzzleBluePool = CreateScriptObjectPool<MuzzleController>(player1MuzzleBluePool, player1MuzzleBluePoolDefiner);
        player1MuzzleYellowPool = CreateScriptObjectPool<MuzzleController>(player1MuzzleYellowPool, player1MuzzleYellowPoolDefiner);

        spiderBoltPool = CreateScriptObjectPool<SpiderBolt>(spiderBoltPool, spiderBoltPoolDefiner);

        voxelPool = CreateScriptObjectPool<VoxelController>(voxelPool, voxelPoolDefiner);
    }

    void Start()
    {
        Debug.Log("Pool Manager created");
    }

    private ScriptObjectPool<T> CreateScriptObjectPool<T>(ScriptObjectPool<T> scriptObjectPool, ScriptObjectPoolDefiner objectPoolDefiner) where T : MonoBehaviour
    {
        scriptObjectPool = new ScriptObjectPool<T>();
        scriptObjectPool.objectsParent = objectPoolDefiner.gameObject;
        scriptObjectPool.poolSize = objectPoolDefiner.poolSize;
        scriptObjectPool.objectWhereScriptIs = objectPoolDefiner.objectWhereScriptIs;
        scriptObjectPool.Init();

        return scriptObjectPool;
    }

    void OnDestroy()
    {
        Debug.Log("Pool Manager destroyed");
    }
}
