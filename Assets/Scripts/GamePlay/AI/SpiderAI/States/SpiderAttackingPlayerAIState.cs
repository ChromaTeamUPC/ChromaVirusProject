using UnityEngine;
using System.Collections;

public class SpiderAttackingPlayerAIState : SpiderAIActionsBaseState
{

    public SpiderAttackingPlayerAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        rsc.enemyMng.blackboard.SpiderStartsAttacking();
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        rsc.enemyMng.blackboard.SpiderStopsAttacking();
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (spiderBlackboard.barrelController != null && spiderBlackboard.barrelController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.attractedToBarrelState;

        spiderBlackboard.checkInfectingChipDelay += Time.deltaTime;

        if ((spiderBlackboard.checkInfectingChipDelay >= spiderBlackboard.spider.checkDeviceEverySeconds))
        {
            if ((rsc.enemyMng.blackboard.activeDevices.Count > 0)
                && (rsc.enemyMng.blackboard.timeRemainingToNextDeviceInfect == 0f))
            {
                return spiderBlackboard.infectingDeviceState;
            }

            spiderBlackboard.checkInfectingChipDelay = 0f;
        }

        return base.Update();
    }
}
