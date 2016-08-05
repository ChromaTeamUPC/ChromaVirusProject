using UnityEngine;
using System.Collections;

public class MosquitoSingleProjectileAttackController : MosquitoMainAttackControllerBase
{
    private GameObject projectile;
    private EnemyShotController projController;

	// Use this for initialization
	void Awake ()
    {
        projectile = Instantiate(projectilePrefab, transform.position, transform.rotation) as GameObject;
        projectile.transform.parent = transform;

        projController = projectile.GetComponent<EnemyShotController>();
        projController.damage = damage;
        projController.speed = speed;
        projController.forceMultiplier = forceMultiplier;
        active = false;
	}

    public override void Shoot(Transform s, PlayerController p)
    {
        base.Shoot(s, p);

        rsc.enemyMng.bb.MosquitoMainShotSpawned();
        projectile.transform.position = source.position;
        if (player != null && player.Alive)
            projectile.transform.LookAt(player.transform.position, Vector3.up);
        else
            projectile.transform.rotation = source.rotation;

        projController.Shoot();
        active = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(active)
        {
            if (!projController.Active)
                ReturnToPool();
        }
	}

    private void ReturnToPool()
    {
        rsc.enemyMng.bb.MosquitoMainShotDestroyed();
        active = false;
        rsc.poolMng.mosquitoSingleProjectilePool.AddObject(this);
    }
}
