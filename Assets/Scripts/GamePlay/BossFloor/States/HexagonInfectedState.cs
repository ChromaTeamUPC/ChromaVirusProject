using UnityEngine;
using System.Collections;

public class HexagonInfectedState : HexagonBaseState
{
    private Vector3 half;

    public HexagonInfectedState(HexagonController hex) : base(hex)
    {
        half = new Vector3(0.5f, 1f, 0.5f);
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.SetPlaneMaterial(hex.planeInfectedMaterial);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.plane.transform.localScale = Vector3.one;
        hex.plane.SetActive(false);
        //hex.SetPlaneMaterial(hex.planeTransparentMat);
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        if (hex.CurrentInfectionDuration < hex.InfectionHalfDuration)
        {
            hex.plane.transform.localScale = Vector3.Lerp(Vector3.one, half, (hex.InfectionHalfDuration - hex.CurrentInfectionDuration) / hex.InfectionHalfDuration);
        }

        if (hex.CurrentInfectionDuration <= 0f)
        {
            return hex.idleState;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.infectedCellDamage);
        return null;
    }
}
