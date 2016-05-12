using UnityEngine;
using System.Collections;

public class PlayerShotController : MonoBehaviour {

    public ChromaColor color;

    public int speed;
    public int damage;

    [Range(0,20)]
    public float maxDuration;

    public GameObject impactParticlePrefab;
    public GameObject projectileParticlePrefab;

    private GameObject impactParticle;
    private GameObject projectileParticle;

    private Rigidbody rigidBody;

    private Quaternion impactOriginalRotation;
    private Quaternion projectileOriginalRotation;

    private int defaultDamage;
    private float currentDuration;

    void Awake()
    {
        defaultDamage = damage;

        projectileParticle = Instantiate(projectileParticlePrefab, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform;
        projectileParticle.SetActive(false);
        projectileOriginalRotation = projectileParticle.transform.rotation;

        impactParticle = Instantiate(impactParticlePrefab, transform.position, transform.rotation) as GameObject;
        impactParticle.transform.parent = transform;
        impactParticle.SetActive(false);
        impactOriginalRotation = impactParticle.transform.rotation;

        rigidBody = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    public void Shoot()
    {
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
            enemy.ImpactedByShot(color, damage);
        }

        //We let the impactParticle do its job
        StartCoroutine(WaitAndReturnToPool());
    }

    IEnumerator WaitAndReturnToPool()
    {
        yield return new WaitForSeconds(2f);
        ReturnToPool();
    }

    void ReturnToPool()
    {
        damage = defaultDamage;
        projectileParticle.transform.localRotation = Quaternion.identity;
        projectileParticle.SetActive(false);
        impactParticle.transform.localRotation = Quaternion.identity;
        impactParticle.SetActive(false);

        switch (color)
        {
            case ChromaColor.RED:
                rsc.poolMng.playerShotRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                rsc.poolMng.playerShotGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                rsc.poolMng.playerShotBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                rsc.poolMng.playerShotYellowPool.AddObject(this);
                break;
        }
    }
}
