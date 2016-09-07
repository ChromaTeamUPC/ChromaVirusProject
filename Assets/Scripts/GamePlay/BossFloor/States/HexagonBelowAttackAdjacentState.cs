using UnityEngine;
using System.Collections;

public class HexagonBelowAttackAdjacentState : HexagonBaseState
{
    public HexagonBelowAttackAdjacentState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.buffPurpleGO.SetActive(true);
    }

    public override void OnStateExit()
    {
        hex.buffPurpleGO.SetActive(false);
        base.OnStateExit();
    }

    public override HexagonBaseState Update()
    {
        //After Fx, infect
        if (!hex.buffPurple.isPlaying)
        {
            hex.SetAuxTimer(hex.infectionTimeAfterAttack);
            return hex.infectedState;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.belowAttackAdjacentDamage, hex.transform.position, hex.infectionForces);
        return null;
    }

    public override HexagonBaseState EnemyStay(EnemyBaseAIBehaviour enemy)
    {
        enemy.ImpactedByHexagon();
        return null;
    }
}
