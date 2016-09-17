using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelizationInfo
{
    public List<VoxelizationServer.AABCGrid> aABCGrids;
    public Vector3 voxelScale;
    public bool randomMaterial;
    public Material mat;
    public bool spawnCollider;
    public Vector3 voxelColliderScale;

    public VoxelizationInfo(List<VoxelizationServer.AABCGrid> aABCGrids,
                            Vector3 voxelScale,
                            bool randomMaterial,
                            Material mat,
                            bool spawnCollider,
                            Vector3 voxelColliderScale)
    {
        this.aABCGrids = aABCGrids;
        this.voxelScale = voxelScale;
        this.randomMaterial = randomMaterial;
        this.mat = mat;
        this.spawnCollider = spawnCollider;
        this.voxelColliderScale = voxelColliderScale;
    }
}

public class VoxelizationManager : MonoBehaviour 
{

    [SerializeField]
    private int maxVoxelizationGridsPerFrame = 1;
    private Queue<VoxelizationInfo> pendingSpawns = new Queue<VoxelizationInfo>();

    private ColoredObjectsManager colorObjMng;
    private VoxelPool voxelPool;
    private ObjectPool voxelColliderPool;
    private VoxelController voxelController;

    void Start()
    {
        colorObjMng = rsc.coloredObjectsMng;
        voxelPool = rsc.poolMng.voxelPool;
        voxelColliderPool = rsc.poolMng.voxelColliderPool;

        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, CancelPendingSpawns);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_LOADED, CancelPendingSpawns);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_UNLOADED, CancelPendingSpawns);
    }

    void OnDestroy()
    {
        if(rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, CancelPendingSpawns);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_LOADED, CancelPendingSpawns);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_UNLOADED, CancelPendingSpawns);
        }
    }


    private void CancelPendingSpawns(EventInfo eventInfo)
    {
        pendingSpawns.Clear();
    }

    public void AddInfoToPendingSpawns(VoxelizationInfo spawnInfo)
    {
        pendingSpawns.Enqueue(spawnInfo);
    }

    void Update()
    {
        int totalSpawnedThisFrame = 0;

        while (totalSpawnedThisFrame < maxVoxelizationGridsPerFrame && pendingSpawns.Count > 0)
        {
            ++totalSpawnedThisFrame;

            SpawnVoxels(pendingSpawns.Dequeue());
        }

    }

    public void SpawnVoxels(VoxelizationInfo info)
    {
        List<VoxelizationServer.AABCGrid> aABCGrids = info.aABCGrids;

        //int total = 0;
        if (aABCGrids != null)
        {
            foreach (VoxelizationServer.AABCGrid aABCGrid in aABCGrids)
            {
                Vector3 preCalc = aABCGrid.GetOrigin();
                for (short x = 0; x < aABCGrid.GetWidth(); ++x)
                {
                    for (short y = 0; y < aABCGrid.GetHeight(); ++y)
                    {
                        for (short z = 0; z < aABCGrid.GetDepth(); ++z)
                        {
                            if (aABCGrid.IsAABCActiveUnsafe(x, y, z))
                            {
                                Vector3 cubeCenter = aABCGrid.GetAABCCenterUnsafe(x, y, z) + preCalc;

                                voxelController = voxelPool.GetObject();
                                if (voxelController != null)
                                {
                                    //++total;
                                    Transform voxelTrans = voxelController.gameObject.transform;
                                    voxelTrans.position = cubeCenter;
                                    voxelTrans.rotation = Quaternion.identity;
                                    //voxelTrans.rotation = Random.rotation;
                                    voxelTrans.localScale = info.voxelScale;
                                    if (!info.randomMaterial)
                                    {
                                        voxelController.GetComponent<Renderer>().sharedMaterial = info.mat;
                                    }
                                    else
                                    {
                                        voxelController.GetComponent<Renderer>().sharedMaterial = colorObjMng.GetVoxelRandomMaterial();
                                    }

                                    voxelController.spawnLevels = 1;
                                }
                            }
                        }
                    }
                }

                //Set a collider in place to make voxels "explode"
                if (info.spawnCollider)
                {
                    GameObject voxelCollider = voxelColliderPool.GetObject();
                    if (voxelCollider != null)
                    {
                        voxelCollider.transform.localScale = info.voxelColliderScale;
                        voxelCollider.transform.position = aABCGrid.GetCenter();
                    }
                }
            }
        }
        //Debug.Log("Spider spawned: " + total);
    }
}
