using UnityEngine;
using System.Collections;

public class MosquitoPatrolingAIState : MosquitoAIActionsBaseState
{
    private float elapsedTime;

    public MosquitoPatrolingAIState(MosquitoBlackboard bb) : base(bb)
    {
        elapsedTime = 0f;
    }

    public override AIBaseState Update()
    {
        /*When mosquito is in this state could happen this:
        1-Every X seconds:
            1a-Target selection
            1b-Check global blackboard. 
                If there are less than X active attacks:
                    Calculate chances (every active attack drops x% chances) and shoot shot
                Else
                    If there are less than X attacking mosquitoes
                        Calculate chances (every attacking mosquito drops x% chances) and go to Attacking Player State
                    Else
                        Keep patroling
        */

        elapsedTime += Time.deltaTime;

        if(elapsedTime >= mosquitoBlackboard.mosquito.checkAttackEverySeconds)
        {
            elapsedTime = 0f;
            if(rsc.enemyMng.blackboard.activeMosquitoShots < mosquitoBlackboard.mosquito.activeAttacksThreshold)
            {
                int chances = 100 - (rsc.enemyMng.blackboard.activeMosquitoShots * mosquitoBlackboard.mosquito.chancesReductionForEachActiveAttack);

                if(RollADice(chances))
                {
                    ShootWeak();
                }
            }
            else if(rsc.enemyMng.blackboard.attackingPlayerMosquitoes < mosquitoBlackboard.mosquito.mosquitoesAttackingThreshold)
            {
                int chances = 100 - (rsc.enemyMng.blackboard.attackingPlayerMosquitoes * mosquitoBlackboard.mosquito.chancesReductionForEachAttackingMosquito);

                if (RollADice(chances))
                {
                    //return mosquitoBlackboard.attackingPlayerState;
                }
            }
        }

        int updateResult = UpdateExecution();
        return ProcessUpdateExecutionResult(updateResult);
    }

    private bool RollADice(int chances)
    {
        return Random.Range(1, 101) <= chances;
    }

    private void ShootWeak()
    {
        MosquitoWeakShotController shot = rsc.coloredObjectsMng.GetMosquitoWeakShot(mosquitoBlackboard.mosquito.color);

        if (shot != null)
        {
            Transform shotSpawn = mosquitoBlackboard.shotSpawnPoint;
            shot.transform.position = shotSpawn.position;
            shot.transform.rotation = shotSpawn.rotation;

            shot.Shoot();
        }
    }
}
