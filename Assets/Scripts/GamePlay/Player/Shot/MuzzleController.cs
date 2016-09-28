using UnityEngine;
using System.Collections;

public class MuzzleController : MonoBehaviour
{
    public ChromaColor color;

    [Range(0, 2)]
    public float duration = 0.1f;

    private float currentDuration;

    public ParticleSystem ps;

    public void Play()
    {
        currentDuration = 0f;
        ps.Play();
    }

    void Update()
    {
        currentDuration += Time.deltaTime;
        if(currentDuration >= duration)
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
                rsc.poolMng.playerMuzzleRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                rsc.poolMng.playerMuzzleGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                rsc.poolMng.playerMuzzleBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                rsc.poolMng.playerMuzzleYellowPool.AddObject(this);
                break;
        }
    }
}

