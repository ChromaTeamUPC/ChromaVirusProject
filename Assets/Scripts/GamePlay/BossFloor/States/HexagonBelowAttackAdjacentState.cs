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
            hex.currentInfectionDuration = hex.infectionTimeAfterAttack;
            hex.countingInfectionTime = true;
            return hex.infectedState;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.belowAttackDamage, hex.transform.position, hex.infectionForces);
        return null;
    }
}
