using UnityEngine;
using System.Collections;

//Just a container for the different pools we have in the game
public class PoolManager : MonoBehaviour
{   
    [SerializeField]
    private ScriptObjectPoolDefiner playerShotRedPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> playerShotRedPool;

    [SerializeField]
    private ScriptObjectPoolDefiner playerShotGreenPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> playerShotGreenPool;

    [SerializeField]
    private ScriptObjectPoolDefiner playerShotBluePoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> playerShotBluePool;

    [SerializeField]
    private ScriptObjectPoolDefiner playerShotYellowPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<PlayerShotController> playerShotYellowPool;


    public ObjectPool spiderPool;

    [SerializeField]
    private ScriptObjectPoolDefiner voxelPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<VoxelController> voxelPool;
    public ObjectPool voxelColliderPool;

    void Awake()
    {
        playerShotRedPool = CreateScriptObjectPool<PlayerShotController>(playerShotRedPool, playerShotRedPoolDefiner);
        playerShotGreenPool = CreateScriptObjectPool<PlayerShotController>(playerShotGreenPool, playerShotGreenPoolDefiner);
        playerShotBluePool = CreateScriptObjectPool<PlayerShotController>(playerShotBluePool, playerShotBluePoolDefiner);
        playerShotYellowPool = CreateScriptObjectPool<PlayerShotController>(playerShotYellowPool, playerShotYellowPoolDefiner);

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
