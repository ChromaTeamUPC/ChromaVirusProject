using UnityEngine;
using System.Collections;

public class HexagonInfectedState : HexagonBaseState
{
    private float halfDuration;
    private Vector3 half;

    public HexagonInfectedState(HexagonController hex) : base(hex)
    {
        half = new Vector3(0.5f, 1f, 0.5f);
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.SetPlaneMaterial(hex.planeInfectedMaterial);
        halfDuration = hex.currentInfectionDuration / 2;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.countingInfectionTime = false;

        hex.plane.transform.localScale = Vector3.one;
        hex.plane.SetActive(false);
        //hex.SetPlaneMaterial(hex.planeTransparentMat);
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        if(hex.countingInfectionTime)
        {
            hex.currentInfectionDuration -= Time.deltaTime;

            if(hex.currentInfectionDuration < halfDuration)
            {
                hex.plane.transform.localScale = Vector3.Lerp(Vector3.one, half, (halfDuration - hex.currentInfectionDuration) / halfDuration);
            }

            if(hex.currentInfectionDuration <= 0f)
            {
                return hex.idleState;
            }
        }

        return null;
    }

    public override HexagonBaseState WormTailExited()
    {
        hex.countingInfectionTime = true;
        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.infectedCellDamage, hex.transform.position);
        return null;
    }
}
