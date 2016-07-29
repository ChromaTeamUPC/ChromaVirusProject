using UnityEngine;
using System.Collections;

public class WormTailController : MonoBehaviour 
{
    public float tailHeight;
    private Transform trf;
    private VoxelizationClient voxelization;

    private WormBlackboard bb;

    void Awake()
    {
        trf = gameObject.transform;
        voxelization = GetComponentInChildren<VoxelizationClient>();
    }

    void Update()
    {
        bb.tailIsUnderground = (trf.position.y < 0 - tailHeight);
    }

    public void SetBlackboard(WormBlackboard bb)
    {
        this.bb = bb;
    }

    public void Explode()
    {
        voxelization.SpawnVoxels();
        Destroy(gameObject);
    }
}
