using UnityEngine;
using System.Collections;

public class PlayerShotController : MonoBehaviour {

    public ChromaColor color;

    public int speed;
    public int damage;
    public float forceMultiplier = 5f;

    [Range(0,20)]
    public float maxDuration;

    public GameObject impactParticlePrefab;
    public GameObject projectileParticlePrefab;

    private GameObject impactParticle;
    private GameObject projectileParticle;

    private Rigidbody rigidBody;

    //private Quaternion impactOriginalRotation;
    //private Quaternion projectileOriginalRotation;

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
        //projectileOriginalRotation = projectileParticle.transform.rotation;

        impactParticle = Instantiate(impactParticlePrefab, transform.position, transform.rotation) as GameObject;
        impactParticle.transform.parent = transform;
        impactParticle.SetActive(false);
        //impactOriginalRotation = impactParticle.transform.rotation;

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

    void OnCollisionEnter(Collision collision)
    {
        //Stop shot
        rigidBody.velocity = Vector3.zero;
        projectileParticle.SetActive(false);
        if(collision.contacts.Length > 0)
            impactParticle.transform.rotation = Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal);
        impactParticle.SetActive(true);

        if (collision.collider.tag == "Enemy")
        {
            SpiderAIBehaviour enemy = collision.collider.GetComponent<SpiderAIBehaviour>();
            enemy.ImpactedByShot(color, damage, transform.forward * forceMultiplier);
        }
        else if (collision.collider.tag == "Vortex")
        {
            VortexController vortex = collision.collider.GetComponent<VortexController>();
            vortex.ImpactedByShot(color, damage);
        }
        else if (collision.collider.tag == "Barrel")
        {
            BarrelImpacted barrel = collision.collider.GetComponent<BarrelImpacted>();
            barrel.controller.ImpactedByShot(color);
        }

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
        //projectileParticle.transform.localRotation = Quaternion.identity;
        projectileParticle.SetActive(false);
        impactParticle.transform.localRotation = Quaternion.identity;
        impactParticle.SetActive(false);

        switch (color)
        {
            case ChromaColor.RED:
                rsc.poolMng.player1ShotRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                rsc.poolMng.player1ShotGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                rsc.poolMng.player1ShotBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                rsc.poolMng.player1ShotYellowPool.AddObject(this);
                break;
        }
    }
}
