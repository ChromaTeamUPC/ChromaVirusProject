using UnityEngine;
using System.Collections;

public class MosquitoMultipleProjectileAttackController : MosquitoMainAttackControllerBase
{
    public int numberOfProjectiles = 3;
    public float timeBetweenProjectiles = 0.3f;
    public float secondsAheadPredictionPerDistanceUnit = 0.05f;

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

        //Activate all at once
        for (int i = 0; i < numberOfProjectiles; ++i)
        {
            projController[i].Active = true;
            projectile[i].SetActive(false);
        }

        //First projectile
        projectile[0].transform.position = source.position;
        if (player != null && player.Alive)
        {
            float secondsAheadPrediction = (player.transform.position - source.position).magnitude * secondsAheadPredictionPerDistanceUnit;
            Vector3 destination = player.PositionPrediction(secondsAheadPrediction);
            //destination.y = source.position.y; //horizontal direction
            projectile[0].transform.LookAt(destination, Vector3.up);
        }
        else
            projectile[0].transform.rotation = source.rotation;
        projectile[0].SetActive(true);

        projController[0].Shoot();

        StartCoroutine(ShootDelayed());

        active = true;
        oneDown = false;      
    }

    private IEnumerator ShootDelayed()
    {
        for (int i = 1; i < numberOfProjectiles; ++i)
        {
            yield return new WaitForSeconds(timeBetweenProjectiles);

            projectile[i].transform.position = source.position;
            if (player != null && player.Alive)
            {
                float secondsAheadPrediction = (player.transform.position - source.position).magnitude * secondsAheadPredictionPerDistanceUnit;
                Vector3 destination = player.PositionPrediction(secondsAheadPrediction);
                //destination.y = source.position.y; //horizontal direction
                projectile[i].transform.LookAt(destination, Vector3.up);
            }
            else
                projectile[i].transform.rotation = source.rotation;
            projectile[i].SetActive(true);

            projController[i].Shoot();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
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
        rsc.poolMng.mosquitoMultipleProjectilePool.AddObject(this);
    }
}
