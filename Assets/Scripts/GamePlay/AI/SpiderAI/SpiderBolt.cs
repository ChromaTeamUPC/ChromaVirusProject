using UnityEngine;
using System.Collections;

public class SpiderBolt : MonoBehaviour
{
    public int damage;
    public float duration;

    public ChromaColor color;

    private SphereCollider boltCollider;
    private float currentDuration;

    void Awake()
    {
        boltCollider = GetComponent<SphereCollider>();
    }

    public void Spawn()
    {
        boltCollider.enabled = true;
        currentDuration = 0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        currentDuration += Time.deltaTime;
        if (currentDuration >= duration)
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        rsc.poolMng.spiderBoltPool.AddObject(this);        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            boltCollider.enabled = false;
            PlayerController player = other.GetComponent<PlayerController>();
            player.TakeDamage(damage, color);
        }
    }
}
