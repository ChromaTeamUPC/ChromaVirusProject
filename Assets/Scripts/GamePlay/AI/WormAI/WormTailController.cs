using UnityEngine;
using System.Collections;

public class WormTailController : MonoBehaviour 
{
    public float tailHeight;
    private Transform trf;
    private VoxelizationClient voxelization;
    private Renderer rend;

    private WormBlackboard bb;

    void Awake()
    {
        trf = gameObject.transform;
        voxelization = GetComponentInChildren<VoxelizationClient>();
        rend = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        if(bb!= null)
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

    public void SetVisible(bool visible)
    {
        rend.enabled = visible;
    }
}
