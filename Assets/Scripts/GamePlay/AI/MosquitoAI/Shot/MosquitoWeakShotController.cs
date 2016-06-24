using UnityEngine;
using System.Collections;

public class MosquitoWeakShotController : MonoBehaviour {

    [SerializeField]
    private ChromaColor color;

    [SerializeField]
    private int speed;

    public int damage;

    [SerializeField]
    private float forceMultiplier = 5f;

    [SerializeField]
    [Range(0, 20)]
    private float maxDuration;

    [SerializeField]
    private GameObject projectileParticlePrefab;
    [SerializeField]
    private GameObject impactParticlePrefab;

    private GameObject impactParticle;
    private GameObject projectileParticle;

    private Rigidbody rigidBody;

    private SphereCollider shotCollider;

    private int defaultDamage;
    private float currentDuration;

    public int Damage { set { damage = value; } }

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
        rsc.enemyMng.blackboard.MosquitoWeakShotSpawned();
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
        if (collision.contacts.Length > 0)
            impactParticle.transform.rotation = Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal);
        impactParticle.SetActive(true);

        if (collision.collider.tag == "Player1" || collision.collider.tag == "Player2")
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            player.ReceiveAttack(damage, color, transform.position);
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
        rsc.enemyMng.blackboard.MosquitoWeakShotDestroyed();
        damage = defaultDamage;
        projectileParticle.SetActive(false);
        impactParticle.transform.localRotation = Quaternion.identity;
        impactParticle.SetActive(false);

        switch (color)
        {
            case ChromaColor.RED:
                rsc.poolMng.mosquitoWeakShotRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                rsc.poolMng.mosquitoWeakShotGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                rsc.poolMng.mosquitoWeakShotBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                rsc.poolMng.mosquitoWeakShotYellowPool.AddObject(this);
                break;
        }
    }
}
