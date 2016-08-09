using UnityEngine;
using System.Collections;

public class HexagonBelowAttackState : HexagonBaseState
{
    public HexagonBelowAttackState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.columnBlinkController.InvalidateMaterials();
        hex.columnBlinkController.BlinkWhiteNoStop(hex.belowAttackBlinkInterval, hex.belowAttackBlinkInterval);

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
        hex.currentInfectionDuration = hex.infectionTimeAfterAttack;
        hex.countingInfectionTime = true;
        return hex.infectedState;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.belowAttackDamage, hex.transform.position);
        return null;
    }
}
