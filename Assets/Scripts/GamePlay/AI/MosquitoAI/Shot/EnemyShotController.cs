using UnityEngine;
using System.Collections;

public class EnemyShotController : MonoBehaviour {

    [SerializeField]
    private ChromaColor color;

    public int speed;
    public int damage;
    public float forceMultiplier = 5f;

    [Range(0, 20)]
    public float maxDuration;

    public bool homing = false;
    public float maxHomingDuration = 0f;
    public float torque = 5f;

    [SerializeField]
    private GameObject projectileParticlePrefab;
    [SerializeField]
    private GameObject impactParticlePrefab;

    private GameObject impactParticle;
    private GameObject projectileParticle;

    private Rigidbody rigidBody;
    public Transform target;

    private SphereCollider shotCollider;

    private int defaultDamage;
    private float currentDuration;
    private float homingDuration;
    private bool active;

    public int Damage { set { damage = value; } }
    public bool Active { get { return active; } set { active = value; } }

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
        homingDuration = 0f;
        active = true;
    }

    void Update()
    {
        if (active)
        {
            currentDuration += Time.deltaTime;
            if (currentDuration >= maxDuration)
                Deactivate();
        }
    }

    void FixedUpdate()
    {
        if(active && homing && shotCollider.enabled && target != null)
        {
            homingDuration += Time.fixedDeltaTime; 
            if(homingDuration <= maxHomingDuration)
            {
                rigidBody.velocity = transform.forward * speed;

                Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
                rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, torque));
            }
        }
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
        StartCoroutine(WaitAndDeactivate());
    }

    private IEnumerator WaitAndDeactivate()
    {
        yield return new WaitForSeconds(2f);
        Deactivate();
    }

    private void Deactivate()
    {
        target = null;
        damage = defaultDamage;
        projectileParticle.SetActive(false);
        impactParticle.transform.localRotation = Quaternion.identity;
        impactParticle.SetActive(false);
        active = false;
    }
}
