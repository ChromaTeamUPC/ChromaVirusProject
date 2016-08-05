using UnityEngine;
using System.Collections;

public class SpiderLeadingGroupAIState : SpiderAIActionsBaseState
{
    public SpiderLeadingGroupAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        spiderBlackboard.groupInfo.leader = spiderBlackboard.spider;
        currentActionIndex = spiderBlackboard.groupInfo.leaderActionIndex;
        UpdateExecutor();
    }

    public override void OnStateExit()
    {
        spiderBlackboard.groupInfo.leader = null;
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        /*When spider is in this state could happen this:
        1-If a barrel of the same color is activated and in range:
            Go to AttractedToBarrel State
        2-If spider is the last in the group and enemies attacking player < threshold:
            Go to AttackPlayer State
        3-Any other case:
            Loop action list
        */
        if (spiderBlackboard.barrelController != null && spiderBlackboard.barrelController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.attractedToBarrelState;

        spiderBlackboard.initialCheckDelay += Time.deltaTime;
        spiderBlackboard.checkAttackingSpidersDelay += Time.deltaTime;
        spiderBlackboard.checkInfectingChipDelay += Time.deltaTime;
        if (spiderBlackboard.groupInfo.followersCount == 0 && spiderBlackboard.initialCheckDelay >= 3f)
        {
            if ((spiderBlackboard.checkInfectingChipDelay >= spiderBlackboard.spider.checkDeviceEverySeconds))
            {
                if ((rsc.enemyMng.bb.activeDevices.Count > 0)
                    && (rsc.enemyMng.bb.timeRemainingToNextDeviceInfect == 0f))
                {
                    return spiderBlackboard.infectingDeviceState;
                }

                spiderBlackboard.checkInfectingChipDelay = 0f;
            }

            if ((spiderBlackboard.checkAttackingSpidersDelay >= spiderBlackboard.spider.checkAttackEverySeconds) //Check once per second
                && (rsc.enemyMng.bb.timeRemainingToNextPlayerAttack == 0f))
            {
                if (rsc.enemyMng.bb.attackingPlayerSpiders < rsc.enemyMng.spidersAttackingThreshold)
                    return spiderBlackboard.attackingPlayerState;

                spiderBlackboard.checkAttackingSpidersDelay = 0f;
            }
        }

        int updateResult = UpdateExecution();

        if (updateResult == AIAction.LIST_FINISHED)
            return spiderBlackboard.attackingPlayerState; //Should not happen because the list has to loop
        else
        {
            AIBaseState result = ProcessUpdateExecutionResult(updateResult);
            spiderBlackboard.groupInfo.leaderActionIndex = currentActionIndex;
            return result;
        }
    }
}
