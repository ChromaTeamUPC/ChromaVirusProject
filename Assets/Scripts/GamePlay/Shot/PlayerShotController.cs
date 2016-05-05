using UnityEngine;
using System.Collections;

public class PlayerShotController : MonoBehaviour {

    public int speed;
    public int damage;

    [Range(0,20)]
    public float maxDuration;

    [HideInInspector]
    public ChromaColor color;
    [HideInInspector]
    public Renderer rend;

    private int defaultDamage;
    private float currentDuration;

    void Awake()
    {
        defaultDamage = damage;
        rend = GetComponentInChildren<Renderer>();
    }

    // Use this for initialization
    public void Shot()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        currentDuration = 0f;
    }

    void Update()
    {
        currentDuration += Time.deltaTime;
        if (currentDuration >= maxDuration)
            ReturnToPool();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            SpiderAIBehaviour enemy = other.GetComponent<SpiderAIBehaviour>();
            enemy.ImpactedByShot(color, damage);
            ReturnToPool();
        }
        else if (other.tag != "DestroyerBoundary")
            ReturnToPool();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "DestroyerBoundary")
            ReturnToPool();
    }

    void ReturnToPool()
    {
        damage = defaultDamage;
        rsc.poolMng.playerShotPool.AddObject(gameObject);
    }
}
