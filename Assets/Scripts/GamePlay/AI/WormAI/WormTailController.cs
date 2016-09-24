using UnityEngine;
using System.Collections;

public class WormTailController : MonoBehaviour 
{
    public float tailHeight;
    private VoxelizationClient voxelization;
    private Renderer rend;

    private WormBlackboard bb;

    void Awake()
    {
        voxelization = GetComponentInChildren<VoxelizationClient>();
        rend = GetComponentInChildren<Renderer>();
    }

    public void SetBlackboard(WormBlackboard bb)
    {
        this.bb = bb;
    }

    public void Explode()
    {
        EnemyExplosionController explosion = rsc.poolMng.enemyExplosionPool.GetObject();

        if (explosion != null)
        {
            explosion.transform.position = transform.position;
            explosion.PlayAll();
        }

        voxelization.SpawnVoxels();
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        rend.enabled = visible;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            bb.head.PlayerTouched(player, transform.position);
        }
        else if (other.tag == "EnemyHexagonBodyProbe")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponent<EnemyBaseAIBehaviour>();

            if (enemy == null)
                enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();

            if (enemy != null)
                enemy.InstantKill();
        }
    }
}
