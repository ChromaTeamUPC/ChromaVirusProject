using UnityEngine;
using System.Collections;

public class WormTailController : MonoBehaviour 
{
    private VoxelizationClient voxelization;

    void Awake()
    {
        voxelization = GetComponentInChildren<VoxelizationClient>();
    }
    public void Explode()
    {
        voxelization.SpawnVoxels();
        Destroy(gameObject);
    }
}
