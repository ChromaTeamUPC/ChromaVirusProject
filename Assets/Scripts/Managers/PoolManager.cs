using UnityEngine;
using System.Collections;

//Just a container for the different pools we have in the game
public class PoolManager : MonoBehaviour
{   
    public ObjectPool playerShotPool;
    public ObjectPool spiderPool;

    [SerializeField]
    private ScriptObjectPoolDefiner voxelPoolDefiner;
    [HideInInspector]
    public ScriptObjectPool<VoxelController> voxelPool;
    public ObjectPool voxelColliderPool;

    void Start()
    {
        Debug.Log("Pool Manager created");
        voxelPool = new ScriptObjectPool<VoxelController>();
        if (voxelPoolDefiner != null)
        {
            voxelPool.objectsParent = voxelPoolDefiner.gameObject;
            voxelPool.poolSize = voxelPoolDefiner.poolSize;
            voxelPool.objectWhereScriptIs = voxelPoolDefiner.objectWhereScriptIs;
            voxelPool.Init();
        }
    }

    void OnDestroy()
    {
        Debug.Log("Pool Manager destroyed");
    }
}
