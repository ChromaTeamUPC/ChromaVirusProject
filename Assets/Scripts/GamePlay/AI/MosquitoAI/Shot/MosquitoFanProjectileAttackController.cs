using UnityEngine;
using System.Collections;

public class MosquitoFanProjectileAttackController : MosquitoMainAttackControllerBase
{
    public int numberOfProjectiles = 3;
    public int angleBetweenProjectiles = 15;

    private bool oneDown;
    private GameObject[] projectile;
    private EnemyShotController[] projController;

    // Use this for initialization
    void Awake()
    {
        projectile = new GameObject[numberOfProjectiles];
        projController = new EnemyShotController[numberOfProjectiles];

        for (int i = 0; i < numberOfProjectiles; ++i)
        {
            projectile[i] = Instantiate(projectilePrefab, transform.position, transform.rotation) as GameObject;
            projectile[i].transform.parent = transform;

            projController[i] = projectile[i].GetComponent<EnemyShotController>();
            projController[i].damage = damage;
            projController[i].speed = speed;
            projController[i].forceMultiplier = forceMultiplier;
        }

        active = false;
    }

    public override void Shoot(Transform s, PlayerController p)
    {
        base.Shoot(s, p);

        rsc.enemyMng.blackboard.MosquitoShotSpawned();

        bool left = true;
        int currentAngle = 0;

        //First projectile straight
        projectile[0].transform.position = source.position;
        projectile[0].transform.rotation = source.rotation;

        projController[0].Shoot();

        for (int i = 1; i < numberOfProjectiles; ++i)
        {
            projectile[i].transform.position = source.position;
            projectile[i].transform.rotation = source.rotation;

            if (left)
            {
                currentAngle += angleBetweenProjectiles;
                projectile[i].transform.Rotate(0, currentAngle, 0, Space.World);
            }
            else
            {
                projectile[i].transform.Rotate(0, currentAngle * -1, 0, Space.World);
            }

            projController[i].Shoot();

            left = !left;
        }
        active = true;
        oneDown = false;
    }

    // Update is called once per frame
    void Update ()
    {
	    if(active)
        {
            bool allInactive = true;

            for (int i = 0; i < numberOfProjectiles; ++i)
            {
                allInactive &= !projController[i].Active;
                //The first that inactivates, decrements this group counter
                if (!oneDown && !projController[i].Active)
                {
                    oneDown = true;
                    rsc.enemyMng.blackboard.MosquitoShotDestroyed();
                }
            }

            if (allInactive)
                ReturnToPool();
        }
	}

    private void ReturnToPool()
    {      
        active = false;
        rsc.poolMng.mosquitoFanProjectilePool.AddObject(this);
    }
}
