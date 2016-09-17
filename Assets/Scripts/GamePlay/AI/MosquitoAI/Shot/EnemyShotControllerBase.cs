using UnityEngine;
using System.Collections;

public class EnemyShotControllerBase : MonoBehaviour 
{
    [SerializeField]
    protected ChromaColor color;

    public int speed;
    public int damage;
    public float forceMultiplier = 5f;

    [SerializeField]
    [Range(0, 20)]
    private float maxDuration;

    [SerializeField]
    private GameObject projectileParticlePrefab;
    [SerializeField]
    private GameObject impactParticlePrefab;

    private GameObject impactParticle;
    private GameObject projectileParticle;

    protected Rigidbody rigidBody;
    protected SphereCollider shotCollider;

    private int defaultDamage;
    private float currentDuration;
    protected bool active;

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

    void OnEnable()
    {
        rsc.eventMng.StartListening(EventManager.EventType.KILL_ENEMIES, KillMySelf);
    }

    void OnDisable()
    {
        rsc.eventMng.StopListening(EventManager.EventType.KILL_ENEMIES, KillMySelf);
    }

    private void KillMySelf(EventInfo eventInfo)
    {
        if(active)
        {
            Impact();
        }
    }

    public virtual void Shoot()
    {
        shotCollider.enabled = true;
        projectileParticle.SetActive(true);
        rigidBody.velocity = transform.forward * speed;
        currentDuration = 0f;
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player1" || collision.collider.tag == "Player2")
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            player.ReceiveAttack(damage, color, transform.position);
        }

        if (collision.contacts.Length > 0)
            impactParticle.transform.rotation = Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal);

        Impact();

    }

    public void Impact()
    {
        //Stop shot
        rigidBody.velocity = Vector3.zero;
        projectileParticle.SetActive(false);
        impactParticle.SetActive(true);

        shotCollider.enabled = false;
        //We let the impactParticle do its job
        StartCoroutine(WaitAndDeactivate());
    }

    private IEnumerator WaitAndDeactivate()
    {
        yield return new WaitForSeconds(2f);
        Deactivate();
    }

    public virtual void Deactivate()
    {
        damage = defaultDamage;
        projectileParticle.SetActive(false);
        impactParticle.transform.localRotation = Quaternion.identity;
        impactParticle.SetActive(false);
        active = false;
    }
}
