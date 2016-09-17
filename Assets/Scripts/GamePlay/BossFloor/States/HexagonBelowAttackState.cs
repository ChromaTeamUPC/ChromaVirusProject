using UnityEngine;
using System.Collections;

public class HexagonBelowAttackState : HexagonBaseState
{
    private float elapsedTime;
    private bool wallLowered;

    public HexagonBelowAttackState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        elapsedTime = 0f;
        wallLowered = false;

        hex.StartPlaneInfectionAnimation();

        hex.columnBlinkController.InvalidateMaterials();
        hex.columnBlinkController.BlinkWhiteNoStop(hex.belowAttackBlinkInterval, hex.belowAttackBlinkInterval);

        if (!hex.continousPurple.isPlaying)
            hex.continousPurple.Play();

        hex.SetAuxTimer(0.5f);
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

        if(!wallLowered)
        {
            if(elapsedTime >= hex.wallDuration)
            {
                hex.LowerWallRing();
                wallLowered = true;
            }
            else
                elapsedTime += Time.deltaTime;
        }

        if (hex.AuxTimer <= 0f)
        {
            hex.SetAuxTimer(hex.infectionTimeAfterAttack);
            return hex.infectedState;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.belowAttackCentralDamage, hex.transform.position, hex.infectionForces);
        return null;
    }

    public override HexagonBaseState EnemyStay(EnemyBaseAIBehaviour enemy)
    {
        enemy.InstantKill();
        return null;
    }
}
