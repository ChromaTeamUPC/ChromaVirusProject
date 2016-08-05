using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MosquitoPatrolingAIState : MosquitoAIActionsBaseState
{
    private float elapsedTime;

    public MosquitoPatrolingAIState(MosquitoBlackboard bb) : base(bb)
    {
        elapsedTime = 0f;
    }

    public override void Init(List<AIAction> actions)
    {
        base.Init(actions);
        currentActionIndex = 0;
        currentExecutor = null;
        UpdateExecutor();
    }

    public override void OnStateEnter()
    {
        //Don't reset executor index
        currentExecutor = null;
        UpdateExecutor();
        elapsedTime = 0f;
    }

    public override AIBaseState Update()
    {
        /*When mosquito is in this state could happen this:
        1-Every X seconds:
            1a-Target selection
            1b-Check global blackboard. 
                If there are less than X attacking mosquitoes
                    Calculate chances (every attacking mosquito drops x% chances) and go to Attacking Player State
                
                If there are less than X active attacks:
                    Calculate chances (every active attack drops x% chances) and shoot shot
                Else           
                    Keep patroling
        */
        if (blackboard.barrelController != null && blackboard.barrelController.currentColor == blackboard.entity.color)
            return mosquitoBlackboard.attractedToBarrelState;

        elapsedTime += Time.deltaTime;

        if(elapsedTime >= mosquitoBlackboard.mosquito.checkAttackEverySeconds)
        {
            elapsedTime = 0f;

            if (mosquitoBlackboard.player != null && mosquitoBlackboard.playerController.Alive)
            {
                if (rsc.enemyMng.bb.attackingPlayerMosquitoes + rsc.enemyMng.bb.activeMosquitoMainShots < rsc.enemyMng.mosquitoesAttackingThreshold)
                {
                    int chances = 100 - ((rsc.enemyMng.bb.attackingPlayerMosquitoes + rsc.enemyMng.bb.activeMosquitoMainShots) * rsc.enemyMng.mosquitoChancesReductionForEachAttack);

                    if (RollADice(chances))
                    {
                        return mosquitoBlackboard.attackingPlayerState;
                    }
                }

                if (rsc.enemyMng.bb.activeMosquitoWeakShots < rsc.enemyMng.mosquitoWeakShotsThreshold)
                {
                    int chances = 100 - (rsc.enemyMng.bb.activeMosquitoWeakShots * rsc.enemyMng.mosquitoChancesReductionForEachWeakShot);

                    if (RollADice(chances))
                    {
                        ShootWeak();
                    }
                }
            }
        }

        int updateResult = UpdateExecution();
        return ProcessUpdateExecutionResult(updateResult);
    }

    private bool RollADice(int chances)
    {
        int dice = Random.Range(1, 101);
        return dice <= chances;
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
