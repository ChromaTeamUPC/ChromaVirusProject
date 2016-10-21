using UnityEngine;
using System.Collections;

public class PlayerDashController : MonoBehaviour {

    public ChromaColor color;
    private ParticleSystem ps;

	// Use this for initialization
	void Awake () {
        ps = GetComponent<ParticleSystem>();
	}

    void OnEnable()
    {
        if (!ps.isPlaying)
            ps.Play();
    }
	
	// Update is called once per frame
	void Update () {
        if (!ps.isPlaying)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        ps.Stop();

        switch (color)
        {
            case ChromaColor.RED:
                rsc.poolMng.playerDashRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                rsc.poolMng.playerDashGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                rsc.poolMng.playerDashBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                rsc.poolMng.playerDashYellowPool.AddObject(this);
                break;
        }
    }
}
