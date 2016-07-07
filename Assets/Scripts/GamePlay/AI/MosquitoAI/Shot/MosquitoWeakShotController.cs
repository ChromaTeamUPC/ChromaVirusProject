using UnityEngine;
using System.Collections;

public class MosquitoWeakShotController : EnemyShotControllerBase
{

    public override void Shoot()
    {
        base.Shoot();

        rsc.enemyMng.blackboard.MosquitoWeakShotSpawned();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        rsc.enemyMng.blackboard.MosquitoWeakShotDestroyed();

        switch (color)
        {
            case ChromaColor.RED:
                rsc.poolMng.mosquitoWeakShotRedPool.AddObject(this);
                break;
            case ChromaColor.GREEN:
                rsc.poolMng.mosquitoWeakShotGreenPool.AddObject(this);
                break;
            case ChromaColor.BLUE:
                rsc.poolMng.mosquitoWeakShotBluePool.AddObject(this);
                break;
            case ChromaColor.YELLOW:
                rsc.poolMng.mosquitoWeakShotYellowPool.AddObject(this);
                break;
        }
    }
}
