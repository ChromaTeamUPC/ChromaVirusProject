using UnityEngine;
using System.Collections;

public class MosquitoShotExecutor : BaseExecutor
{
    private MosquitoBlackboard mosquitoBlackboard;
    private MosquitoShotAIAction shotAction;

    public override void Init(EnemyBaseBlackboard bb)
    {
        base.Init(bb);
        mosquitoBlackboard = (MosquitoBlackboard)bb;
    }

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        shotAction = (MosquitoShotAIAction)act;

        blackBoard.agent.Stop();

        //Start shot animation
        blackBoard.attackAnimationEnded = false;
        //blackBoard.attackAnimationTrigger = false;

        //TODO: Remove when having proper animations
        blackBoard.attackAnimationTrigger = true; 
    }

    public override int Execute()
    {
        if (blackBoard.attackAnimationEnded)
        {
            return action.nextAction;
        }
        else if (blackBoard.attackAnimationTrigger)
        {
            blackBoard.attackAnimationTrigger = false;

            MosquitoMainAttackControllerBase attack;

            switch (blackBoard.entity.color)
            {
                case ChromaColor.RED:
                    attack = rsc.poolMng.mosquitoHomingProjectilePool.GetObject();
                    break;

                case ChromaColor.GREEN:
                    attack = rsc.poolMng.mosquitoFanProjectilePool.GetObject();
                    break;

                case ChromaColor.BLUE:
                    attack = rsc.poolMng.mosquitoMultipleProjectilePool.GetObject();
                    break;

                case ChromaColor.YELLOW:
                    attack = rsc.poolMng.mosquitoSingleProjectilePool.GetObject();
                    break;

                default:
                    attack = null;
                    break;
            }

            if (attack != null)
            {
                attack.Shoot(mosquitoBlackboard.shotSpawnPoint, mosquitoBlackboard.playerController);
            }
            //TODO: Remove when having proper animations
            blackBoard.attackAnimationEnded = true;
        }

        return AIAction.ACTION_NOT_FINISHED;
    }
}
