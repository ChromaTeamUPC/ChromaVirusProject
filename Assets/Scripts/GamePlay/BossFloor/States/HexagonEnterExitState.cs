using UnityEngine;
using System.Collections;

public class HexagonEnterExitState : HexagonBaseState
{
    public HexagonEnterExitState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.StartPlaneInfectionAnimation();

        hex.columnBlinkController.InvalidateMaterials();
        hex.columnBlinkController.BlinkWhiteNoStop(hex.enterExitBlinkInterval, hex.enterExitBlinkInterval);

        if (!hex.continousPurple.isPlaying)
            hex.continousPurple.Play();

        hex.SetAuxTimer(0.5f);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.columnBlinkController.StopPreviousBlinkings();
        hex.continousPurple.Stop();

        hex.StopPlaneInfectionAnimation();
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        if (hex.AuxTimer <= 0f)
        {
            hex.SetAuxTimer(hex.infectionTimeAfterAttack);
            return hex.infectedState;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.enterExitDamage, hex.transform.position, hex.infectionForces);
        return null;
    }

    public override HexagonBaseState EnemyStay(EnemyBaseAIBehaviour enemy)
    {
        enemy.InstantKill();
        return null;
    }
}