using UnityEngine;
using System.Collections;

public class HexagonEnterExitState : HexagonBaseState
{
    public HexagonEnterExitState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.SetPlaneMaterial(hex.planeInfectedMaterial);

        hex.columnBlinkController.InvalidateMaterials();
        hex.columnBlinkController.BlinkWhiteNoStop(hex.enterExitBlinkInterval, hex.enterExitBlinkInterval);

        if (!hex.continousPurple.isPlaying)
            hex.continousPurple.Play();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.columnBlinkController.StopPreviousBlinkings();
        hex.continousPurple.Stop();
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        return null;
    }

    public override HexagonBaseState WormTailExited()
    {
        hex.currentInfectionDuration = hex.infectionTimeAfterEnterExit;
        hex.countingInfectionTime = true;
        return hex.infectedState;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.enterExitDamage, hex.transform.position, hex.infectionForces);
        return null;
    }
}