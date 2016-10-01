using UnityEngine;
using System.Collections;

public class PlayerShotController : MonoBehaviour
{
    public int playerId = 1;

    public ChromaColor color;

    [SerializeField]
    private int speed;

    public int damage;

    [SerializeField]
    private float forceMultiplier = 5f;

    [SerializeField]
    [Range(0,20)]
    private float maxDuration;

    [HideInInspector]
    public PlayerController player;

    [SerializeField]
    private GameObject impactParticlePrefab;
    [SerializeField]
    private GameObject projectileParticlePrefab;

    [SerializeField]
    private AudioClip goodImpact;
    [SerializeField]
    private AudioClip wrongImpact;

    private GameObject impactParticle;
    private GameObject projectileParticle;

    private Rigidbody rigidBody;

    private SphereCollider shotCollider;

    private int defaultDamage;
    private float currentDuration;

    void Awake()
    {
        defaultDamage = damage;      

        shotCollider = GetComponent<SphereCollider>();

        projectileParticle = Instantiate(projectileParticlePrefab, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform;
        projectileParticle.SetActive(false);

        impactParticle = Instantiate(impactParticlePrefab, transform.position, transform.rotation) as GameObject;
        impactParticle.transform.parent = transform;
        impactParticle.SetActive(false);

        rigidBody = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    public void Shoot()
    {
        shotCollider.enabled = true;
        projectileParticle.SetActive(true);
        rigidBody.velocity = transform.forward * speed;
        currentDuration = 0f;
    }

    void Update()
    {
        currentDuration += Time.deltaTime;
        if (currentDuration >= maxDuration)
            ReturnToPool();
    }

    public Vector3 GetImpactVector()
    {
        return transform.forward * forceMultiplier;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Stop shot
        rigidBody.velocity = Vector3.zero;
        projectileParticle.SetActive(false);

        impactParticle.GetComponent<AudioSource>().clip = wrongImpact;

        if (collision.collider.tag == "Enemy")
        {
            EnemyBaseAIBehaviour enemy = collision.collider.GetComponent<EnemyBaseAIBehaviour>();
            if (enemy == null) 
                enemy = collision.collider.GetComponentInParent<EnemyBaseAIBehaviour>();

            if (enemy != null)
            {
                if (enemy.color == color)
                    impactParticle.GetComponent<AudioSource>().clip = goodImpact;

                enemy.ImpactedByShot(color, damage, transform.forward * forceMultiplier, player);
            }
        }
        else if (collision.collider.tag == "Vortex")
        {
            VortexController vortex = collision.collider.GetComponent<VortexController>();
            if (vortex != null)
            {
                vortex.ImpactedByShot(color, damage, player);
                if(vortex.Active)
                    impactParticle.GetComponent<AudioSource>().clip = goodImpact;
                else
                    impactParticle.GetComponent<AudioSource>().clip = wrongImpact;
            }
        }
        else if (collision.collider.tag == "Barrel")
        {
            CapacitorImpacted barrel = collision.collider.GetComponent<CapacitorImpacted>();
            if (barrel != null)
                barrel.controller.ImpactedByShot(color, player);

            impactParticle.GetComponent<AudioSource>().clip = goodImpact;
        }
        else if (collision.collider.tag == "WormBody")
        {
            WormBodySegmentController worm = collision.collider.GetComponent<WormBodySegmentController>();
            if (worm != null)
            {
                if (worm.Color == color)
                    impactParticle.GetComponent<AudioSource>().clip = goodImpact;

                worm.ImpactedByShot(color, damage, player);
            }
        }
        else if (collision.collider.tag == "WormHead")
        {
            WormAIBehaviour worm = collision.collider.GetComponent<WormAIBehaviour>();
            if (worm != null)
                worm.ImpactedByShot(color, damage, player);

            impactParticle.GetComponent<AudioSource>().clip = goodImpact;
        }
        else if (collision.collider.tag == "Hexagon")
        {
            HexagonController hexagon = collision.collider.GetComponentInParent<HexagonController>();
            if (hexagon != null)
            {
                hexagon.ImpactedByShot(player);
                if(hexagon.HasActiveTurret())
                    impactParticle.GetComponent<AudioSource>().clip = goodImpact;
                else
                    impactParticle.GetComponent<AudioSource>().clip = wrongImpact;
            }
        }

        if(collision.contacts.Length > 0)
            impactParticle.transform.rotation = Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal);
        impactParticle.SetActive(true);

        shotCollider.enabled = false;
        //We let the impactParticle do its job
        StartCoroutine(WaitAndReturnToPool());
    }

    private IEnumerator WaitAndReturnToPool()
    {
        yield return new WaitForSeconds(2f);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        damage = defaultDamage;
        projectileParticle.SetActive(false);
        impactParticle.transform.localRotation = Quaternion.identity;
        impactParticle.SetActive(false);

        switch (color)
        {
            case ChromaColor.RED:
                if (playerId == 1)
                    rsc.poolMng.player1ShotRedPool.AddObject(this);
                else
                    rsc.poolMng.player2ShotRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                if (playerId == 1)
                    rsc.poolMng.player1ShotGreenPool.AddObject(this);
                else
                    rsc.poolMng.player2ShotGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                if (playerId == 1)
                    rsc.poolMng.player1ShotBluePool.AddObject(this);
                else
                    rsc.poolMng.player2ShotBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                if (playerId == 1)
                    rsc.poolMng.player1ShotYellowPool.AddObject(this);
                else
                    rsc.poolMng.player2ShotYellowPool.AddObject(this);
                break;
        }
    }
}
