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
                rsc.poolMng.player1MuzzleRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                rsc.poolMng.player1MuzzleGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                rsc.poolMng.player1MuzzleBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                rsc.poolMng.player1MuzzleYellowPool.AddObject(this);
                break;
        }
    }
}

