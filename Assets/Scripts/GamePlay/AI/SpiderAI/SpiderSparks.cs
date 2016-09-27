using UnityEngine;
using System.Collections;

public class SpiderSparks : MonoBehaviour 
{
    private ParticleSystem sparks;

    void Awake()
    {
        sparks = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (!sparks.isPlaying)
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        rsc.poolMng.spiderSparksPool.AddObject(this);
    }
}
