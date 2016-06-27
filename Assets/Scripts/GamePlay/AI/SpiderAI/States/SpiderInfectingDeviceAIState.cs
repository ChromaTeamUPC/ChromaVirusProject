﻿using UnityEngine;
using System.Collections;

public class SpiderInfectingDeviceAIState : SpiderAIActionsBaseState
{
    public SpiderInfectingDeviceAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        rsc.enemyMng.blackboard.SpiderStartsInfectingDevice();
    }

    public override void OnStateExit()
    {
        rsc.enemyMng.blackboard.SpiderStopsInfectingDevice();
        base.OnStateExit();
    }

    public override AIBaseState Update()
    {
        if (spiderBlackboard.barrelController != null && spiderBlackboard.barrelController.currentColor == spiderBlackboard.spider.color)
            return spiderBlackboard.attractedToBarrelState;


        return base.Update();
    }
}