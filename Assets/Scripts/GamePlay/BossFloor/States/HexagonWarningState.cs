using UnityEngine;
using System.Collections;

public class HexagonWarningState : HexagonBaseState
{
    public HexagonWarningState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.SetPlaneMaterial(hex.planeWarningMat);

        hex.planeBlinkController.BlinkTransparentNoStop(hex.warningBlinkInterval, hex.warningBlinkInterval);
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
