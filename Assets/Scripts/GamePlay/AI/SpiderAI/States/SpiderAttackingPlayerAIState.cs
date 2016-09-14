using UnityEngine;
using System.Collections;

public class SpiderAttackingPlayerAIState : SpiderAIActionsBaseState
{

    public SpiderAttackingPlayerAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        rsc.enemyMng.bb.SpiderStartsAttacking();
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        rsc.enemyMng.bb.SpiderStopsAttacking();
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (spiderBlackboard.capacitorController != null && spiderBlackboard.capacitorController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.attractedToBarrelState;

        spiderBlackboard.checkInfectingChipDelay += Time.deltaTime;

        if ((spiderBlackboard.checkInfectingChipDelay >= spiderBlackboard.spider.checkDeviceEverySeconds))
        {
            if ((rsc.enemyMng.bb.activeDevices.Count > 0)
                && (rsc.enemyMng.bb.timeRemainingToNextDeviceInfect == 0f))
            {
                return spiderBlackboard.infectingDeviceState;
            }

            spiderBlackboard.checkInfectingChipDelay = 0f;
        }

        return base.Update();
    }
}
