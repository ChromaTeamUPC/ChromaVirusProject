using UnityEngine;
using System.Collections;

public class HexagonBelowAttackWarningState : HexagonBaseState
{
    public HexagonBelowAttackWarningState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.SetPlaneMaterial(hex.planeBelowAttackWarningMat);

        hex.planeBlinkController.BlinkTransparentNoStop(hex.belowAttackWarnInterval, hex.belowAttackWarnInterval);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.planeBlinkController.StopPreviousBlinkings();

        hex.plane.SetActive(false);
        //hex.SetPlaneMaterial(hex.planeTransparentMat);
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        return null;
    }
}
