using UnityEngine;
using System.Collections;

public class SpecialAttackController : MonoBehaviour 
{
    private ParticleSystem mainPs;

    void Awake()
    {
        mainPs = GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (!mainPs.isPlaying)
            rsc.poolMng.player1SpecialAttackPool.AddObject(this);
	}
}
