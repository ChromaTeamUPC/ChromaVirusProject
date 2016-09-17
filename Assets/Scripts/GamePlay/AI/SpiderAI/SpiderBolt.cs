using UnityEngine;
using System.Collections;

public class SpiderBolt : MonoBehaviour
{
    public float damage;
    public float duration;
    public Vector3 origin;

    public ChromaColor color;

    private AudioSource soundFX;
    private SphereCollider boltCollider;
    private float currentDuration;

    void Awake()
    {
        soundFX = GetComponent<AudioSource>();
        boltCollider = GetComponent<SphereCollider>();
    }

    public void Spawn(bool playSoundFX = true)
    {
        soundFX.playOnAwake = playSoundFX;
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
            player.ReceiveAttack(damage, color, origin);
        }
    }
}
