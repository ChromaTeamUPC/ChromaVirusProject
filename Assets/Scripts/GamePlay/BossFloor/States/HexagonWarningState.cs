using UnityEngine;
using System.Collections;

public class HexagonWarningState : HexagonBaseState
{
    public HexagonWarningState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        if (hex.mainAttackHexagon)
        {
            hex.plane.transform.localScale = new Vector3(1.1f, 1f, 1.1f);
            hex.SetPlaneMaterial(hex.planeMainWarningMat);
        }
        else
            hex.SetPlaneMaterial(hex.planeSecondaryWarningMat);

        hex.planeBlinkController.BlinkTransparentNoStop(hex.warningBlinkInterval, hex.warningBlinkInterval);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.planeBlinkController.StopPreviousBlinkings();

        hex.plane.transform.localScale = Vector3.one;
        hex.plane.SetActive(false);
        //hex.SetPlaneMaterial(hex.planeTransparentMat);
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        return null;
    }
}
