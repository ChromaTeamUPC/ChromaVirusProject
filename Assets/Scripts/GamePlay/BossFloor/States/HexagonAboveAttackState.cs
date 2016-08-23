using UnityEngine;
using System.Collections;

public class HexagonAboveAttackState : HexagonBaseState
{
    public HexagonAboveAttackState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.SetPlaneMaterial(hex.planeInfectedMaterial);

        hex.columnBlinkController.InvalidateMaterials();
        hex.columnBlinkController.BlinkWhiteNoStop(hex.aboveAttackBlinkInterval, hex.aboveAttackBlinkInterval);

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
        hex.CurrentInfectionDuration = hex.infectionTimeAfterAttack;
        return hex.infectedState;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.aboveAttackDamage, hex.transform.position, hex.infectionForces);
        return null;
    }
}
