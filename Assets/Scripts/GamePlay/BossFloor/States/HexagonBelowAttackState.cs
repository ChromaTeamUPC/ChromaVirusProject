using UnityEngine;
using System.Collections;

public class HexagonBelowAttackState : HexagonBaseState
{
    public HexagonBelowAttackState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.StartPlaneInfectionAnimation();

        hex.columnBlinkController.InvalidateMaterials();
        hex.columnBlinkController.BlinkWhiteNoStop(hex.belowAttackBlinkInterval, hex.belowAttackBlinkInterval);

        if (!hex.continousPurple.isPlaying)
            hex.continousPurple.Play();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.LowerWallRing();

        hex.columnBlinkController.StopPreviousBlinkings();
        hex.continousPurple.Stop();

        hex.StopPlaneInfectionAnimation();
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
        player.ReceiveInfection(hex.belowAttackCentralDamage, hex.transform.position, hex.infectionForces);
        return null;
    }
}
